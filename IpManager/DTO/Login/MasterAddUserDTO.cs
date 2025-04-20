using System.ComponentModel.DataAnnotations;

namespace IpManager.DTO.Login
{
    public class MasterAddUserDTO
    {
        /// <summary>
        /// 사용자 ID
        /// </summary>
        [Required]
        public string userId { get; set; } = null!;

        /// <summary>
        /// 사용자 비밀번호
        /// </summary>
        [Required]
        public string password { get; set; } = null!;

        /// <summary>
        /// 관리자 유무
        /// </summary>
        public bool adminYn { get; set; } = false;

        /// <summary>
        /// 1레이어 지역 ID
        /// </summary>
        [Required]
        public int countryId { get; set; }
    }
}
