using System.ComponentModel.DataAnnotations;

namespace IpManager.DTO.Login
{
    /// <summary>
    /// 사용자 ID 검사 DTO
    /// </summary>
    public class UserIDCheckDTO
    {
        /// <summary>
        /// 사용자 ID
        /// </summary>
        [Required]
        public string? userId { get; set; }
    }
}
