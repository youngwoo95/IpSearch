using IpManager.DBModel;
using IpManager.DTO;
using IpManager.DTO.Login;

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

        /// <summary>
        /// PID에 해당하는 LoginModel 반환
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        Task<LoginTb?> GetUserInfoAsyncById(int pid);

        /// <summary>
        /// 사용자 전체리스트 반환 - PageNation
        /// </summary>
        /// <returns></returns>
        Task<List<LoginTb>?> GetUserListAsync(int pageIndex, int pagenumber);

        /// <summary>
        /// 사용자 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<int> EditUserAsync(LoginTb model);

        /// <summary>
        /// 사용자 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        Task<int> DeleteUserAsync(LoginTb model);

    }
}
