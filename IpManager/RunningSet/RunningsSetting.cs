using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;

namespace IpManager.RunningSet
{
    public class RunningsSetting : DbContext
    {
        // 서비스 프로바이더 _ 의존성 주입
        private readonly IServiceProvider ServiceProvider;

        public RunningsSetting(IServiceProvider _serviceProvider)
        {
            this.ServiceProvider = _serviceProvider;
        }

        /// <summary>
        /// 프로그램 시작시 검사
        /// </summary>
        /// <returns></returns>
        public async Task DefaultSetting()
        {
            // 스코프 단위로 진행
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();
                
                int MasterCheck = await context.LoginTbs
                    .Where(m => m.MasterYn == true && m.DelYn != true).CountAsync();

                // Master 검사시 0이면 마스터가 없는경우. --> 첫시작일 경우임.
                if(MasterCheck == 0)
                {
                    var model = new LoginTb
                    {
                        Uid = "Master",
                        Pwd = "1234",
                        CreateDt = DateTime.Now,
                        MasterYn = true,
                        AdminYn = false,
                        UseYn = true,
                    };
                    await context.LoginTbs.AddAsync(model);
                    await context.SaveChangesAsync().ConfigureAwait(false); // 저장
                    
                    Console.WriteLine("마스터 생성완료");
                }
            }
        }


    }
}
