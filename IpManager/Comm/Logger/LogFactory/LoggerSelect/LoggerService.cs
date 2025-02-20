namespace IpManager.Comm.Logger.LogFactory.LoggerSelect
{
    public class LoggerService : ILoggerService
    {
        /// <summary>
        /// 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void FileLogMessage(string message)
        {
            try
            {
                DateTime Today = DateTime.Now;
                string dir_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemLog", Today.Year.ToString());

                DirectoryInfo di = new DirectoryInfo(dir_path);

                // 년도 파일 없으면 생성
                if (!di.Exists)
                {
                    di.Create();
                }

                dir_path = Path.Combine(dir_path, Today.Month.ToString());
                di = new DirectoryInfo(dir_path);

                // 월 파일 없으면 생성
                if (!di.Exists)
                {
                    di.Create();
                }

                // 일
                dir_path = Path.Combine(dir_path, $"{Today.Year}_{Today.Month}_{Today.Day}.txt");

                // 일.txt + 로그내용
                using (StreamWriter writer = new StreamWriter(dir_path, true))
                {
                    System.Diagnostics.StackTrace objStackTrace = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(1));
                    var s = objStackTrace.ToString(); // 호출한 함수 위치
                    writer.WriteLine($"[INFO]_[{Today.ToString()}]\t{message}");

#if DEBUG
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;

                    // 로그 출력
                    Console.WriteLine($"[INFO] {message}");

                    Console.ResetColor();
#endif
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        public void FileErrorMessage(string message)
        {
            try
            {
                DateTime Today = DateTime.Now;
                string dir_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemLog", Today.Year.ToString());

                DirectoryInfo di = new DirectoryInfo(dir_path);

                // 년도 파일 없으면 생성
                if (!di.Exists)
                {
                    di.Create();
                }

                dir_path = Path.Combine(dir_path, Today.Month.ToString());
                di = new DirectoryInfo(dir_path);

                // 월 파일 없으면 생성
                if (!di.Exists)
                {
                    di.Create();
                }

                // 일
                dir_path = Path.Combine(dir_path, $"{Today.Year}_{Today.Month}_{Today.Day}.txt");

                // 일.txt + 로그내용
                using (StreamWriter writer = new StreamWriter(dir_path, true))
                {
                    System.Diagnostics.StackTrace objStackTrace = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(1));
                    var s = objStackTrace.ToString(); // 호출한 함수 위치
                    writer.WriteLine($"[ERROR]_[{Today.ToString()}]\t{message}");

#if DEBUG
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;

                    // 로그 출력
                    Console.WriteLine($"[ERROR] {message}");

                    Console.ResetColor();
#endif
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void ConsoleLogMessage(string message)
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
        public void ConsoleErrorMessage(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"[Error] {message}");

            Console.ResetColor();

        }
    }
}
