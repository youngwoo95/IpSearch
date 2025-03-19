namespace IpManager.DTO.Login
{
    public class LoginRoleDTO
    {
        /// <summary>
        /// Role
        /// </summary>
        public int? pId { get; set; }

        /// <summary>
        /// 유저아이디
        /// </summary>
        public string? uId { get; set; }

        /// <summary>
        /// 권한
        /// </summary>
        public string? Role { get; set; }
    }
}
