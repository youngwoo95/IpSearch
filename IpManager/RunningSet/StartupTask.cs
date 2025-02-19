
using IpManager.Repository;

namespace IpManager.RunningSet
{
    public class StartupTask : IHostedService
    {
        private readonly RunningsSetting Setting;

        public StartupTask(RunningsSetting _setting)
        {
            this.Setting = _setting;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Setting.DefaultSetting();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
