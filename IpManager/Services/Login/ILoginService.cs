using IpManager.Controllers;
using IpManager.DTO;

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
        /// REFRESH_TOKEN 발행 _ 로그인 서비스
        /// </summary>
        /// <param name="accesstoken"></param>
        /// <returns></returns>
        public Task<ResponseUnit<TokenDTO>?> RefreshTokenService(ReTokenDTO accesstoken);

        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> AddUserService(RegistrationDTO dto);
    }
}
