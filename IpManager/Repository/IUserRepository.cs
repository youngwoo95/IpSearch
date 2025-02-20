using IpManager.DBModel;
using IpManager.DTO;

namespace IpManager.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<int> AddUserAsync(LoginTb model);

        /// <summary>
        /// 사용자 ID 존재유무 검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        Task<int> CheckUserIdAsync(string userid);

        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        Task<LoginTb?> GetLoginAsync(string userid, string pw);

        /// <summary>
        /// 해당 ID 로그인가능 권한검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        Task<int> GetLoginPermission(string userid);

        /// <summary>
        /// USERID에 해당하는 UserModel 반환
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        Task<LoginTb?> GetUserInfoAsync(string userid);
    }
}
