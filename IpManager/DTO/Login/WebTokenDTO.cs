namespace IpManager.DTO.Login
{
    /// <summary>
    /// 웹 전용 로그인
    /// AccessToken & RefreshToken
    /// - Regacy (현재 프로젝트에서 사용되지 않음)
    /// </summary>
    public class WebTokenDTO
    {
        /// <summary>
        /// AccessToken
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
