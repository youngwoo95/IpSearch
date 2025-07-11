﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Login;
using IpManager.Helpers;
using IpManager.Services.Login;
using IpManager.SwaggerExample;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoggerService LoggerService;
        private ILoginService LoginService;

        public LoginController(ILoggerService _loggerservice,
            ILoginService _loginservice)
        {
            this.LoggerService = _loggerservice;
            this.LoginService = _loginservice;
        }

        [HttpGet]
        [Route("v1/test")]
        [Produces("application/json")]
        public async Task<IActionResult> Test()
        {
            return Ok("테스트성공");
        }

        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/AddUser")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "회원정보 추가",
            Description = "아이디, 비밀번호 필수값, 권한제한 없음")]
        public async Task<IActionResult> AddUser([FromBody]RegistrationDTO dto)
        {
            try
            {
                ResponseUnit<bool> model = await LoginService.AddUserService(dto).ConfigureAwait(false);

                if(model is null)
                    return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 사용자 ID 검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/CheckUserId")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "사용자 ID 검사",
            Description = "아이디 필수값, 권한제한 없음")]
        public async Task<IActionResult> UserIdCheck([FromBody]UserIDCheckDTO dto)
        {
            try
            {
                //CheckUserIdService
                ResponseUnit<bool> model = await LoginService.CheckUserIdService(dto).ConfigureAwait(false);
                if (model is null)
                    return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 토큰에 대한 사용자정보 반환
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("sign/v1/GetRole")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<LoginRoleDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerGetRoleDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized,typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "토큰에 대한 권한 반환",
        Description = "Header에 JWT토큰 필수, 권한제한 없음")]
        public async Task<IActionResult> GetLoginRole()
        {
            try
            {
                var model = await User.GetUserRole(HttpContext);
                if (model is null)
                    return Unauthorized();
                else
                {
                    var result = new ResponseUnit<LoginRoleDTO>
                    {
                        message = "요청이 정상 처리되었습니다.",
                        data = model,
                        code = 200
                    };
                    return Ok(result);
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 로그인 - AccessToken 발급
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Login")]
        [Produces("application/json")]
        [SwaggerResponse(200,"성공", typeof(ResponseUnit<TokenDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerLoginDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<TokenDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "로그인",
    Description = "아이디, 비밀번호 필수값, 권한제한 없음")]
        public async Task<IActionResult> AccessToken([FromBody] LoginDTO logininfo)
        {
            try
            {
                ResponseUnit<TokenDTO>? model = await LoginService.AccessTokenService(logininfo).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex) 
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }


        /// <summary>
        /// 계정 리스트 반환
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Master")] // Master만 접근가능
        [HttpGet]
        [Route("sign/v1/AccountList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<UserListDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerAccountList))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<UserListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "계정 전체 리스트 반환",
    Description = "권한제한 있음 - Master만 가능")]
        public async Task<IActionResult> AccountList()
        {
            try
            {
                ResponseList<UserListDTO>? model = await LoginService.GetUserListService().ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 계정관리 - 수정
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Master")] // Master만 접근가능
        [HttpPost]
        [Route("sign/v1/AccountManagement")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(200, typeof(SwaggerAccountManagement))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "계정 정보 수정",
    Description = "권한제한 있음 - Master만 가능")]
        public async Task<IActionResult> AccountManagement([FromBody] UserUpdateDTO dto)
        {
            try
            { 
                ResponseUnit<bool> model = await LoginService.UpdateUserService(dto).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 계정 삭제
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="Master")] // Master만 접근가능
        [HttpPut]
        [Route("sign/v1/AccountDelete")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(200, typeof(SwaggerAccountDelete))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "계정 삭제",
    Description = "권한제한 있음 - Master만 가능")]
        public async Task<IActionResult> AccountDelete([FromBody][Required] DeleteAccountDTO dto)
        {
            try
            {
                ResponseUnit<bool> model = await LoginService.DeleteUserService(dto.pid).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }


        /// <summary>
        /// 마스터가 회원 등록
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Master")] // 마스터만 가능
        [HttpPost]
        [Route("sign/v1/MasterAddUser")]
        [Produces("application/json")]
        public async Task<IActionResult> MasterAddUser([FromBody][Required] MasterAddUserDTO dto)
        {
            try
            {
                ResponseUnit<bool> model = await LoginService.MasterAddUserService(dto).ConfigureAwait(false);

                if (model is null)
                    return BadRequest();

                if (model.code == 200)
                    return Ok(model);
                else if (model.code == 404)
                    return NotFound();
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        #region 웹전용 로그인 - Regacy
        /*
        /// <summary>
        /// 로그인 - Regacy (현재 프로젝트에서 사용되지 않음)
        /// </summary>
        /// <param name="logininfo"></param>
        /// <returns>AccessToken 발행</returns>
        [HttpPost]
        [Route("v1/WebLogin")]
        public async Task<IActionResult> WebAccessToken([FromBody]LoginDTO logininfo)
        {
            try
            {
                ResponseUnit<WebTokenDTO>? model = await LoginService.WebAccessTokenService(logininfo);
                if (model is null)
                    return BadRequest();

                if (model.Code == 204)
                    return NoContent();
                else
                    return Ok(model.data);
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// Refresh 토큰 발행 - Regacy (현재 프로젝트에서 사용되지 않음)
        /// </summary>
        /// <param name="refresh"></param>
        /// <returns>AccessToken 발행</returns>
        [HttpPost]
        [Route("v1/WebRefresh")]
        public async Task<IActionResult> WebRefreshToken([FromBody] ReTokenDTO refresh)
        {
            try
            {
                ResponseUnit<WebTokenDTO>? model = await LoginService.WebRefreshTokenService(refresh);
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
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }
        */
        #endregion
    }
}
