namespace IpManager.DTO.Login
{
    public class ReTokenDTO
    {
        /// <summary>
        /// 사용자 ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
