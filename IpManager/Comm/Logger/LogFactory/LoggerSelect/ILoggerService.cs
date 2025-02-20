namespace IpManager.Comm.Logger.LogFactory.LoggerSelect
{
    public interface ILoggerService
    {
        /// <summary>
        /// 파일 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void FileLogMessage(string message);

        /// <summary>
        /// 파일 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        public void FileErrorMessage(string message);

        /// <summary>
        /// 콘솔 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void ConsoleLogMessage(string message);

        /// <summary>
        /// 콘솔 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        public void ConsoleErrorMessage(string message);
    }
}
