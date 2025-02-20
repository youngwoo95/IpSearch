namespace IpManager.DTO.Login
{
    /// <summary>
    /// 회원가입 DTO
    /// </summary>
    public class RegistrationDTO
    {
        /// <summary>
        /// 등록할 ID
        /// </summary>
        public string? UserID { get; set; }

        /// <summary>
        /// 등록할 비밀번호
        /// </summary>
        public string? PassWord { get; set; }
    }
}
