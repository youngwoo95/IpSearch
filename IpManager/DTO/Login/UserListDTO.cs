namespace IpManager.DTO.Login
{
    public class UserListDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int pId { get; set; }

        /// <summary>
        /// 사용자ID
        /// </summary>
        public string? uId { get; set; }

        /// <summary>
        /// 매니저 여부
        /// </summary>
        public bool adminYn { get; set; }

        /// <summary>
        /// 승인 여부
        /// </summary>
        public bool useYn { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public string? createDt { get; set; }

        /// <summary>
        /// 도시 명
        /// </summary>
        public string? CountryName { get; set; }
    }
}
