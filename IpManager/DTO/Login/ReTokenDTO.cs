namespace IpManager.DTO.Login
{
    public class ReTokenDTO
    {
        /// <summary>
        /// 사용자 ID
        /// </summary>
        public string? userId { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string? refreshToken { get; set; }
    }
}
