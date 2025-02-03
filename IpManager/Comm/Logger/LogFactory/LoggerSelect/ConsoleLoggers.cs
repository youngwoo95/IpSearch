namespace IpManager.Comm.Logger.LogFactory.LoggerSelect
{
    public class ConsoleLoggers : ILoggerModels
    {
        /// <summary>
        /// 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void LogMessage(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            // 로그 출력
            Console.WriteLine($"[Info] {message}");

            Console.ResetColor();
        }

        /// <summary>
        /// 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ErrorMessage(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"[Error] {message}");

            Console.ResetColor();

        }

    }
}
