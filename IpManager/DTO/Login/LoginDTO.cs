namespace IpManager.DTO.Login
{
    /// <summary>
    /// 로그인 DTO
    /// </summary>
    public class LoginDTO
    {
        private string? loginid;
        private string? loginpw;

        public string? LoginID
        {
            get
            {
                return loginid;
            }
            set
            {
                // 소문자로 변환
                loginid = value?.ToLower();
            }
        }

        public string? LoginPW
        {
            get
            {
                return loginpw;
            }
            set
            {
                // 소문자로 변환
                loginpw = value?.ToLower();
            }
        }

    }
}
