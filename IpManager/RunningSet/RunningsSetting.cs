using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;

namespace IpManager.RunningSet
{
    public class RunningsSetting : DbContext
    {
        private readonly ILoggerService LoggerService;

        // 서비스 프로바이더 _ 의존성 주입
        private readonly IServiceProvider ServiceProvider;

        public RunningsSetting(IServiceProvider _serviceProvider,
            ILoggerService _loggerservice)
        {
            this.ServiceProvider = _serviceProvider;
            this.LoggerService = _loggerservice;
        }

        /// <summary>
        /// 프로그램 시작시 검사
        /// </summary>
        /// <returns></returns>
        public async Task DefaultSetting()
        {
            try
            {
#if DEBUG
                LoggerService.FileLogMessage("프로그램 실행");
#endif
                // 스코프 단위로 진행
                using (var scope = ServiceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();

                    int MasterCheck = await context.LoginTbs
                        .Where(m => m.MasterYn == true && m.DelYn != true && m.Uid == "Master").CountAsync();

                    // Master 검사시 0이면 마스터가 없는경우. --> 첫시작일 경우임.
                    if (MasterCheck == 0)
                    {
                        var model = new LoginTb
                        {
                            Uid = "Master",
                            Pwd = "1234",
                            CreateDt = DateTime.Now,
                            UpdateDt = DateTime.Now,
                            MasterYn = true,
                            AdminYn = false,
                            UseYn = true,
                            DelYn = false
                        };
                        await context.LoginTbs.AddAsync(model);
                        await context.SaveChangesAsync().ConfigureAwait(false); // 저장

                        Console.WriteLine("마스터 생성완료");
                    }
                }

                using (var scope = ServiceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();

                    /* TIME CHECK */
                    // 00:00 / 00:30 / ... / 23:30
                    List<string> timeList = new List<string>();
                    TimeOnly start = new TimeOnly(0, 0, 0);

                    // 48개의 데이터를 생성 (0분부터 47*30 = 1410분까지)
                    for (int i = 0; i < 48; i++)
                    {
                        TimeOnly time = start.AddMinutes(i * 30);
                        timeList.Add(time.ToString("HH:mm:ss"));
                    }

                    Console.WriteLine(timeList.Count);

                    // DB에서 Time 값이 있는 레코드를 "HH:mm:ss" 형식의 문자열로 조회
                    var existingTimes = await context.TimeTbs
                        .Where(x => x.Time.HasValue)
                        .Select(x => x.Time!.Value.ToString("HH:mm:ss"))
                        .ToListAsync();

                    // DB에 없는 시간 문자열 계산
                     var missingTimes = timeList.Except(existingTimes).ToList();

                    foreach (var timeStr in missingTimes)
                    {
                        // 문자열을 다시 TimeOnly로 변환
                        TimeOnly time = TimeOnly.ParseExact(timeStr, "HH:mm:ss", null);
                        var newRecord = new TimeTb
                        {
                            Time = time,
                        };
                        await context.TimeTbs.AddAsync(newRecord);
                    }

                    if (missingTimes.Any())
                    {
                        await context.SaveChangesAsync();
                        Console.WriteLine("누락된 시간이 추가되었습니다.");
                    }
                    else
                    {
                        Console.WriteLine("모든 시간이 이미 DB에 존재합니다.");
                    }
                }

            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
            }
        }


    }
}
