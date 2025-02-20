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
            }catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
            }
        }


    }
}
