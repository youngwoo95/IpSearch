using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

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
                    //TimeSpan delay = TimeSpan.FromSeconds(1); // 얘를지우고 위를 살리면됨.

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

                        DateTime CurrentTime = DateTime.Now; // 실제로 DB에 박힐 데이터
                        string ThisTime = CurrentTime.ToString("HH") + (CurrentTime.Minute < 30 ? ":00:00" : ":30:00");


                        using (var scope = ScopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();

                            // 로직 동작시간과 같은걸 찾음
                            var existingTimes = await context.TimeTbs
                                    .Where(x => x.Time.HasValue)
                                    .ToListAsync(); // 여기서 DB 쿼리 수행

                            TimeTb? TimeTB = existingTimes.Where(m => m.Time!.Value.ToString("HH:mm:ss") == ThisTime).FirstOrDefault();

                            Console.WriteLine(TimeTB.Pid);
                            Console.WriteLine(TimeTB.Time);

                            /* PING 쏘는 로직 */
                            // PC방 리스트를 받는다.
                            var PCRoomList = await context.PcroomTbs.Where(m => m.DelYn != true).ToListAsync();

                            // 여기에 담아서 한번에 Add해야할듯. -- 메모리 최적화
                            List<PinglogTb> LogTB = new List<PinglogTb>();
                            foreach (var room in PCRoomList)
                            {
                                string target = GetIpPrefix(room.Ip);
                                Console.WriteLine($"타겟 접두어: {target}");

                                // 0 ~ 255 범위의 IP에 대해 병렬 Ping 작업 생성
                                var pingTasks = Enumerable.Range(0, 256)
                                    .Select(i =>
                                    {
                                        string ipAddress = $"{target}.{i}";
                                        return PingHostAsync(ipAddress, stoppingToken);
                                    })
                                    .ToList();

                                // 병렬로 실행 후 결과 집계
                                var results = await Task.WhenAll(pingTasks);

                                var resultipCount = results.Where(r => !String.IsNullOrWhiteSpace(r)).Count();

                                LogTB.Add(new PinglogTb
                                {
                                    UsedPc = resultipCount, // 사용대수
                                    Price = (room.Price/2)*resultipCount, // 총금액 = 요금제/2 * 사용대수
                                    CreateDt = DateTime.Now,
                                    UpdateDt = DateTime.Now,
                                    DelYn = false,
                                    PcroomtbId = room.Pid, // ROOM TB PID
                                    CountrytbId = room.CountrytbId,
                                    CitytbId = room.CitytbId,
                                    TowntbId = room.TowntbId,
                                    TimetbId = TimeTB.Pid, // TIMETB ID
                                });
                            }

                            await context.PinglogTbs.AddRangeAsync(LogTB);
                            await context.SaveChangesAsync().ConfigureAwait(false); // 저장
                            Console.WriteLine("저장완료");
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
        private async Task<string?> PingHostAsync(string ipAddress, CancellationToken cancellationToken)
        {
            using (var ping = new Ping())
            {
                try
                {
                    // 타임아웃을 1000ms로 설정
                    PingReply reply = await ping.SendPingAsync(ipAddress, 1000);
                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        return ipAddress;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.FileErrorMessage($"Ping error for {ipAddress}: {ex.Message}");
                    return null;
                }
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
            int thirdDotIndex = 01;
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
