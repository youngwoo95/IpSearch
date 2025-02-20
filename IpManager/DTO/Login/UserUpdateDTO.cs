namespace IpManager.DTO.Login
{
    public class UserUpdateDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int PID { get; set; }

        /// <summary>
        /// UserID - 변경대상 아님
        /// </summary>
        public string? UID { get; set; } 

        /// <summary>
        /// 변경할 비밀번호
        /// </summary>
        public string? PWD { get; set; }

        /// <summary>
        /// 관리자 여부
        /// </summary>
        public bool AdminYN { get; set; } = false;

        /// <summary>
        /// 승인여부
        /// </summary>
        public bool UseYN { get; set; } = false;

    }
}
