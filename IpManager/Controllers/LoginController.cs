using IpManager.Comm.Logger.LogFactory;
using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO;
using IpManager.Services.Login;
using Microsoft.AspNetCore.Mvc;


namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoggerModels Logger;
        private ILoginService LoginService;


        public LoginController(ILoggers _loggerFactory,
            ILoginService _loginservice)
        {
            this.Logger = _loggerFactory.CreateLogger(false);
            this.LoginService = _loginservice;
        }

        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="logininfo"></param>
        /// <returns>AccessToken 발행</returns>
        [HttpPost]
        [Route("v1/Login")]
        public async Task<IActionResult> AccessToken([FromBody]LoginDTO logininfo)
        {
            try
            {
                ResponseUnit<TokenDTO>? model = await LoginService.AccessTokenService(logininfo);
                if (model is null)
                    return BadRequest();

                if (model.Code == 204)
                    return NoContent();
                else
                    return Ok(model.data);
            }
            catch(Exception ex)
            {
                Logger.ErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// Refresh 토큰 발행
        /// </summary>
        /// <param name="refresh"></param>
        /// <returns>AccessToken 발행</returns>
        [HttpPost]
        [Route("sign/v1/Refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refresh)
        {
            try
            {
                ResponseUnit<TokenDTO>? model = await LoginService.RefreshTokenService(refresh);
                if (model is null)
                    return BadRequest();

                if (model.Code == 204)
                    return NoContent();
                else if (model.Code == 401)
                    return Unauthorized(new { Error = "인증 토큰이 만료되었습니다." });
                else
                    return Ok(model.data);
            }
            catch(Exception ex)
            {
                Logger.ErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }


    }
}
