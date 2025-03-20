using IpManager.DTO.Login;

namespace IpManager.Services.Login
{
    public interface ILoginService
    {
        /// <summary>
        /// ACCESS_TOKEN 발행 _ 로그인 서비스
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<TokenDTO>?> AccessTokenService(LoginDTO dto);

        /// <summary>
        /// ACCESS_TOKEN 발행 _ 로그인 서비스 (레거시)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //public Task<ResponseUnit<WebTokenDTO>?> WebAccessTokenService(LoginDTO dto);

        /// <summary>
        /// REFRESH_TOKEN 발행 _ 로그인 서비스
        /// </summary>
        /// <param name="accesstoken"></param>
        /// <returns></returns>
        //public Task<ResponseUnit<WebTokenDTO>?> WebRefreshTokenService(ReTokenDTO accesstoken);

        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> AddUserService(RegistrationDTO dto);

        /// <summary>
        /// 사용자 ID 검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> CheckUserIdService(UserIDCheckDTO dto);

        /// <summary>
        /// 사용자 전체 LIST 반환
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<UserListDTO>?> GetUserListService();

        /// <summary>
        /// 사용자 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> UpdateUserService(UserUpdateDTO dto);

        /// <summary>
        /// 사용자 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> DeleteUserService(int pid);
    }
}
