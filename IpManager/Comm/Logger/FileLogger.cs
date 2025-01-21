
namespace IpManager.Comm.Logger
{
    public class FileLogger : ICustomLogger
    {
        private readonly string FilePath;

        public FileLogger(string _filepath)
        {
            this.FilePath = _filepath;
        }

        /// <summary>
        /// 로그 메시지
        /// </summary>
        /// <param name="message"></param>
        public void LogMessage(string message)
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
                    writer.WriteLine($"[{Today.ToString()}]\t{message}");

#if DEBUG
              
#endif
                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// 에러 메시지
        /// </summary>
        /// <param name="message"></param>
        public void ErrorMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}
