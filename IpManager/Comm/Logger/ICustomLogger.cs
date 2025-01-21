namespace IpManager.Comm.Logger
{
    public interface ICustomLogger
    {
        /// <summary>
        /// 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void LogMessage(string message);

        /// <summary>
        /// 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        public void ErrorMessage(string message);
    }
}
