namespace IpManager.DTO.Login
{
    public class UserListDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int PID { get; set; }

        /// <summary>
        /// 사용자ID
        /// </summary>
        public string? UID { get; set; }

        /// <summary>
        /// 매니저 여부
        /// </summary>
        public bool AdminYN { get; set; }

        /// <summary>
        /// 승인 여부
        /// </summary>
        public bool UseYN { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public string? CreateDT { get; set; }

    }
}
