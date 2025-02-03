using IpManager.Comm.Logger.LogFactory.LoggerSelect;

namespace IpManager.Comm.Logger.LogFactory
{
    public class CustomLoggerFactorys : ILoggers
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomLoggerFactorys(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILoggerModels CreateLogger(bool loggerType)
        {
            if (loggerType)
            {
                // * Console Log 반환
                return _serviceProvider.GetRequiredService<ConsoleLoggers>();
            }
            else
            {
                // * File Log 반환
                return _serviceProvider.GetRequiredService<FileLoggers>();
            }
        }
    }
}
