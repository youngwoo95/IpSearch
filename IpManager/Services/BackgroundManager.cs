using IpManager.Comm.Logger.LogFactory.LoggerSelect;

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

                while(!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("실행?");
                    }
                    catch(TaskCanceledException)
                    {
                        // Task가 취소된 경우 안전하게 종료
                        break;
                    }
                    catch(Exception ex)
                    {
                        LoggerService.FileErrorMessage(ex.ToString());
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    catch(TaskCanceledException)
                    {
                        Console.WriteLine("Delay가 취소된 경우 처리");
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
