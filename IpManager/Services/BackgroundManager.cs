﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;

namespace IpManager.Services
{
    public class BackgroundManager : BackgroundService
    {
        private readonly IServiceScopeFactory ScopeFactory;
        private readonly ILoggerService LoggerService;

        public BackgroundManager(IServiceScopeFactory _scopeFactory,
            ILoggerService _loggerservice)
        {
            this.LoggerService = _loggerservice;
            this.ScopeFactory = _scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("백그라운드 타이머 시작 / 간격 30분");
                
                // 서비스 시작부 - 전역 동시처리 한계
                int globalMax = Math.Clamp(Environment.ProcessorCount * 2, 5, 20);
                var globalSemaphore = new SemaphoreSlim(globalMax);

                while (!stoppingToken.IsCancellationRequested)
                {
                    // 현재 시간 구하기
                    DateTime now = DateTime.Now;
                    DateTime nextRun;

                    // 현재 시간이 xx:00 ~ xx:29:59 인 경우 다음 정각은 xx:30:00,
                    // 현재 시간이 xx:30 ~ xx:59:59 인 경우 다음 정각은 (xx+1):00:00
                    if (now.Minute < 30)
                    {
                        nextRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, 30, 0);
                    }
                    else
                    {
                        // xx:30 이상이면 다음 시각의 정각
                        nextRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
                    }

                    #region 개발하는 동안 주석 
                    // 남은 대기 시간 계산
                    TimeSpan delay = nextRun - now;
                    if (delay < TimeSpan.Zero)
                    {
                        delay = TimeSpan.Zero;
                    }


                    Console.WriteLine($"다음 실행 시간: {nextRun:yyyy-MM-dd HH:mm:ss}");
                    #endregion
                    //delay = TimeSpan.FromSeconds(10); // 얘를지우고 위를 살리면됨.

                    // 다음 정각까지 대기
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("Delay가 취소됨");
                        break;
                    }

                    // 정각에 실행될 코드 및 예정된 정각 시간 출력
                    try
                    {
                        Console.WriteLine($"정각 실행: {nextRun:yyyy-MM-dd HH:mm:ss}");

                        var wakeTime = DateTime.Now;
                        var slotMinute = wakeTime.Minute < 30 ? 0 : 30;
                        var slotHour = wakeTime.Hour;

                        // 2) TimeSpan 생성
                        var targetSpan = new TimeSpan(slotHour, slotMinute, 0);
                        Console.WriteLine($"[DEBUG] targetSpan = {targetSpan}");

                        using (var scope = ScopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();

                            // 3) EF 쿼리: TimeSpan 비교
                            // 2) 시·분 프로퍼티로 비교
                            var timeTb = await context.TimeTbs
                                .Where(t =>
                                    t.Time.Value.Hour == slotHour &&
                                    t.Time.Value.Minute == slotMinute
                                )
                                .FirstOrDefaultAsync();

                            if (timeTb == null)
                            {
                                LoggerService.FileErrorMessage($"[{slotHour}:{slotMinute}] 슬롯이 없습니다.");
                            }
                            else
                            {
                                Console.WriteLine($"Pid:  {timeTb.Pid}");
                                Console.WriteLine($"Time: {slotHour}:{slotMinute}");
                            }
                            Console.WriteLine(timeTb!.Pid);
                            Console.WriteLine(timeTb.Time);

                            /* PING 쏘는 로직 */
                            // PC방 리스트를 받는다.
                            var PCRoomList = await context.PcroomTbs.Where(m => m.DelYn != true).ToListAsync();

                            // 여기에 담아서 한번에 Add해야할듯. -- 메모리 최적화
                            foreach (var room in PCRoomList)
                            {
                                // 1) IPv4 포맷 검사; 유효하지 않으면 다음 room 으로 스킵
                                if (!IPAddress.TryParse(room.Ip, out var ipAddress)
                                    || ipAddress.AddressFamily != AddressFamily.InterNetwork)
                                {
                                    // (원하면 로그 남기기)
                                    Console.WriteLine($"잘못된 IP 포맷 스킵: {room.Ip}");
                                    continue;
                                }

                                // 2) IP 바이트 분해
                                var bytes = ipAddress.GetAddressBytes();  // [A, B, C, D]
                                string prefix = $"{bytes[0]}.{bytes[1]}.{bytes[2]}";
                                
                                int start = bytes[3];
                                int total = Math.Min(room.Seatnumber, 254 - start + 1);
                                int end = start + total - 1;

                                // TCP 포트 스캔을 비동기로 병렬 수행
                                int openCount = 0;
                                var addresses = Enumerable.Range(start, total)
                                                          .Select(i => $"{prefix}.{i}");

                                await Parallel.ForEachAsync(addresses,
                                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                                    async (addr, ct) =>
                                    {
                                        using var tcp = new TcpClient();
                                        try
                                        {
                                            var connectTask = tcp.ConnectAsync(addr, room.Port);
                                            if (await Task.WhenAny(connectTask, Task.Delay(500, ct)) == connectTask)
                                            {
                                                Interlocked.Increment(ref openCount);
                                            }
                                        }
                                        catch
                                        {
                                            // 연결 실패 시 무시
                                        }
                                    });
                                // 로그 엔티티 추가
                                await context.PinglogTbs.AddAsync(new PinglogTb
                                {
                                    UsedPc = openCount,                              // 사용 중인 PC 수
                                    Price = (room.Price / 2) * openCount,           // 요금제/2 * 사용대수
                                    PcCount = room.Seatnumber,                        // 총 PC 수
                                    PcRate = ((float)openCount / room.Seatnumber) * 100, // 가동률(%)
                                    CreateDt = DateTime.Now,
                                    UpdateDt = DateTime.Now,
                                    DelYn = false,
                                    PcroomtbId = room.Pid,
                                    CountrytbId = room.CountrytbId,
                                    CitytbId = room.CitytbId,
                                    TowntbId = room.TowntbId,
                                    TimetbId = timeTb.Pid // timeTb는 외부에서 할당된 현재 시간 기준 테이블 ID
                                });
                                await context.SaveChangesAsync().ConfigureAwait(false); // 저장
                                Console.WriteLine(prefix.ToString());
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        LoggerService.FileErrorMessage(ex.ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
            }
        }

        /// <summary>
        /// PING SEND
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <summary>
        /// PING SEND
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string?> PingHostAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
        {
            // 1) IP 리터럴 분기
            IPAddress address;
            if (!IPAddress.TryParse(ipAddress, out address))
            {
                // 호스트네임일 때만 DNS 조회
                var addresses = await Dns.GetHostAddressesAsync(ipAddress, cancellationToken);
                if (addresses.Length == 0)
                    return null;
                address = addresses[0];
            }

            var endpoint = new IPEndPoint(address, port);

            // 3회 시도
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                // 시도별 타임아웃 분리
                var timeout = attempt switch
                {
                    1 => TimeSpan.FromMilliseconds(300),
                    2 => TimeSpan.FromSeconds(1),
                    _ => TimeSpan.FromSeconds(2),
                };
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 1) 핸드셰이크
                    await socket.ConnectAsync(endpoint, linkedCts.Token);

                    // 2) 스트림 생성 및 쓰기 테스트
                    using var stream = new NetworkStream(socket, ownsSocket: false);
                    await stream.WriteAsync(new byte[] { 0 }, 0, 1, linkedCts.Token);

                    // 성공 시 즉시 IP 반환
                    return ipAddress;
                }
                catch
                {
                    // 실패 시 살짝 대기 후 다음 시도
                    if (attempt < 3)
                        await Task.Delay(50, cancellationToken);
                }
            }

            // 두 번 모두 실패
            return null;
        }

        /// <summary>
        /// IP xxx.xxx.xxx.xxx 마지막 .xxx 제거
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string GetIpPrefix(string ip)
        {
            if(string.IsNullOrEmpty(ip))
            {
                return ip;
            }

            int dotCount = 0;
            int thirdDotIndex = -1;
            for(int i = 0; i < ip.Length; i++)
            {
                if (ip[i] == '.')
                {
                    dotCount++;
                    if(dotCount == 3)
                    {
                        thirdDotIndex = i;
                        break;
                    }
                }
            }
            return (dotCount >= 3 && thirdDotIndex > 0) ? ip.Substring(0, thirdDotIndex) : ip;
        }

    }
}
