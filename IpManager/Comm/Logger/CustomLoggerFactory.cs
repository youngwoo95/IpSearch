namespace IpManager.Comm.Logger
{
    public class CustomLoggerFactory : ILoggerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomLoggerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICustomLogger CreateLogger(bool loggerType)
        {
            return loggerType switch
            {
                true => _serviceProvider.GetRequiredService<ConsoleLogger>(),
                false => _serviceProvider.GetRequiredService<FileLogger>()
            };
        }
    }
}
