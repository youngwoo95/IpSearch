namespace IpManager.Comm.Logger
{
    public interface ILoggerFactory
    {
        ICustomLogger CreateLogger(bool logType);
    }
}
