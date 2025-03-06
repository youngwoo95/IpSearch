namespace IpManager.DTO.Login
{
    /// <summary>
    /// 로그인 DTO
    /// </summary>
    public class LoginDTO
    {
        private string? loginid;
        private string? loginpw;

        public string? loginId
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

        public string? loginPw
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
