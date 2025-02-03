using IpManager.Comm.Logger.LogFactory.LoggerSelect;

namespace IpManager.Comm.Logger.LogFactory
{
    public interface ILoggers
    {
        ILoggerModels CreateLogger(bool logType);
    }
}
