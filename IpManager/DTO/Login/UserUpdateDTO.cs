using System.ComponentModel.DataAnnotations;

namespace IpManager.DTO.Login
{
    public class UserUpdateDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        [Required]
        public int pId { get; set; }

        /// <summary>
        /// 변경할 비밀번호
        /// </summary>
        public string? pwd { get; set; }

        /// <summary>
        /// 관리자 여부
        /// </summary>
        public bool? adminYn { get; set; }

        /// <summary>
        /// 승인여부
        /// </summary>
        public bool? useYn { get; set; }

        /// <summary>
        /// 지역 ID
        /// </summary>
        public string? countryName { get; set; }
    }
}
