namespace IpManager.DTO
{
    public class RefreshTokenDTO
    {
        /// <summary>
        /// 사용자 ID
        /// </summary>
        public string UserId { get; set; } = String.Empty;

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string RefreshToken { get; set; } = String.Empty;
    }
}
