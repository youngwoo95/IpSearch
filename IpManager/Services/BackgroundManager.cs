using IpManager.Comm.Logger.LogFactory.LoggerSelect;
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

                        DateTime CurrentTime = DateTime.Now;
                        int slotMinute = CurrentTime.Minute < 30 ? 0 : 30;
                        // 2) TimeOnly 으로 변환
                        TimeOnly targetSlot = new TimeOnly(CurrentTime.Hour, slotMinute, 0);

                        using (var scope = ScopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();
    
                            var timeTb = await context.TimeTbs.Where(t => t.Time == targetSlot).FirstOrDefaultAsync();
                            if (timeTb == null)
                            {
                                LoggerService.FileErrorMessage($"[{targetSlot:HH:mm:ss}] 슬롯이 없습니다.");
                            }
                            else
                            {
                                Console.WriteLine($"Pid:  {timeTb.Pid}");
                                Console.WriteLine($"Time: {timeTb.Time:HH:mm:ss}");
                            }
                            Console.WriteLine(timeTb!.Pid);
                            Console.WriteLine(timeTb.Time);

                            /* PING 쏘는 로직 */
                            // PC방 리스트를 받는다.
                            var PCRoomList = await context.PcroomTbs.Where(m => m.DelYn != true).ToListAsync();

                            // 여기에 담아서 한번에 Add해야할듯. -- 메모리 최적화
                            List<PinglogTb> LogTB = new List<PinglogTb>();
                            foreach (var room in PCRoomList)
                            {
                                string prefix = room.Ip;
                                int lastPart = 0;

                                var segments = room.Ip.Split('.');

                                if (segments.Length == 4 &&
                                    int.TryParse(segments[0], out _) &&
                                    int.TryParse(segments[1], out _) &&
                                    int.TryParse(segments[2], out _) &&
                                    int.TryParse(segments[3], out lastPart))
                                {
                                    prefix = $"{segments[0]}.{segments[1]}.{segments[2]}";
                                }
                                else
                                {
                                    prefix = room.Ip;
                                    lastPart = 1;
                                }

                                int start = Math.Max(1, lastPart);
                                int end = Math.Min(254, start + room.Seatnumber - 1);
                                int count = end - start + 1;
                                
                                //string target = GetIpPrefix(room.Ip);
                                Console.WriteLine($"타겟 접두어: {prefix}");

                                // 0 ~ 255 범위의 IP에 대해 병렬 Ping 작업 생성
                                var pingTasks = Enumerable
                                .Range(start, count)
                                .Select(i =>
                                    PingHostAsync($"{prefix}.{i}", room.Port, stoppingToken)
                                )
                                .ToList();

                                // 병렬로 실행 후 결과 집계
                                var results = await Task.WhenAll(pingTasks);

                                var resultipCount = results.Where(r => !String.IsNullOrWhiteSpace(r)).Count();

                                LogTB.Add(new PinglogTb
                                {
                                    UsedPc = resultipCount, // 사용대수
                                    Price = (room.Price/2)*resultipCount, // 총금액 = 요금제/2 * 사용대수
                                    PcCount = room.Seatnumber, // PC방 인덱스
                                    PcRate = ((float)resultipCount / room.Seatnumber) * 100, // 가동률
                                    CreateDt = DateTime.Now,
                                    UpdateDt = DateTime.Now,
                                    DelYn = false,
                                    PcroomtbId = room.Pid, // ROOM TB PID
                                    CountrytbId = room.CountrytbId,
                                    CitytbId = room.CitytbId,
                                    TowntbId = room.TowntbId,
                                    TimetbId = timeTb.Pid, // TIMETB ID
                                });
                            }

                            await context.PinglogTbs.AddRangeAsync(LogTB);
                            await context.SaveChangesAsync().ConfigureAwait(false); // 저장
                            Console.WriteLine("저장완료");

                            // 여기서 12시인 경우 로직도 있어야할듯.

                           
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
            var addresses = await Dns.GetHostAddressesAsync(ipAddress, cancellationToken);
            if (addresses.Length == 0) return null;
            var endpoint = new IPEndPoint(addresses[0], port);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            try
            {
                // 1) 핸드셰이크 시도
                await socket.ConnectAsync(endpoint, cts.Token);

                // 2) 스트림 생성
                using var stream = new NetworkStream(socket, ownsSocket: false);

                // 3) 쓰기 테스트: 0바이트를 보내거나, 실제 프로토콜 바이트 하나를 보내 봅니다.
                //    여기서는 0바이트로도 쓰기가 가능하면 write 경로가 열려 있다고 간주.
                await stream.WriteAsync(Array.Empty<byte>(), 0, 0, cts.Token);

                // 쓰기 성공 시
                return ipAddress;
            }
            catch (OperationCanceledException)
            {
                // 타임아웃
                return null;
            }
            catch (SocketException)
            {
                // 연결 실패 혹은 쓰기 실패
                return null;
            }
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
