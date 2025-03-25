using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Login;
using IpManager.Repository.Login;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IpManager.Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly ILoggerService LoggerService;
        private readonly IConfiguration Configuration;
        private readonly IMemoryCache MemoryCache; // 메모리캐쉬
        private readonly IUserRepository UserRepository;

        public LoginService(ILoggerService _loggerservice,
            IConfiguration _configuration,
            IMemoryCache _memorycache,
            IUserRepository _userrepository)
        {
            this.LoggerService = _loggerservice;
            this.Configuration = _configuration;
            this.MemoryCache = _memorycache;
            this.UserRepository = _userrepository;
        }


        /// <summary>
        /// AccessToken 반환 [로그인]
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<TokenDTO>?> AccessTokenService(LoginDTO dto)
        {
            try
            {
                if (dto is null)
                    return new ResponseUnit<TokenDTO>() { message = "잘못된 입력값이 존재합니다.", data = null, code = 200 };

                if (dto.loginId is null || dto.loginPw is null)
                    return new ResponseUnit<TokenDTO>() { message = "잘못된 입력값이 존재합니다.", data = null, code = 200 };

                if (!String.IsNullOrEmpty(dto.loginId) && dto.loginId.Any(char.IsWhiteSpace)) // NULL 검사 + 공백검사
                {
                    return new ResponseUnit<TokenDTO>() { message = "잘못된 입력값이 존재합니다.", data = null, code = 200 };
                }

                // 사용허가 검사
                int LoginPermission = await UserRepository.GetLoginPermission(dto.loginId).ConfigureAwait(false);
                if (LoginPermission < 1)
                    return new ResponseUnit<TokenDTO>() { message = "승인되지 않은 아이디입니다.", data = null, code = 200 };

                LoginTb? model = await UserRepository.GetLoginAsync(dto.loginId, dto.loginPw).ConfigureAwait(false);
                if(model is null)
                    return new ResponseUnit<TokenDTO>() { message = "해당 아이디가 존재하지 않습니다.", data = null, code = 200};


                // Claim 생성
                List<Claim> authClaims = new List<Claim>();
                authClaims.Add(new Claim("userPid", model.Pid.ToString())); // 사용자 PID
                authClaims.Add(new Claim("userId", model.Uid)); // 사용자 ID
                if(model.MasterYn)
                    authClaims.Add(new Claim("Role", "Master")); // 마스터
                    //authClaims.Add(new Claim(ClaimTypes.Role, "Master")); // 마스터
                else if (model.AdminYn)
                    authClaims.Add(new Claim("Role", "Manager")); // 매니저
                    //authClaims.Add(new Claim(ClaimTypes.Role, "Manager")); // 매니저
                else
                    authClaims.Add(new Claim("Role", "Visitor")); // 방문자
                    //authClaims.Add(new Claim(ClaimTypes.Role, "Visitor")); // 방문자

                // JWT 인증 페이로드 사인 비밀키
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:authSigningKey"]!));

                // JWT 객체 생성
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: Configuration["JWT:Issuer"],
                    audience: Configuration["JWT:Audience"],
                    expires: DateTime.Now.AddDays(30), // 30일 후 만료
                    //expires: DateTime.Now.AddSeconds(30), // 테스트 30초
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                // accessToken
                string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                var tokenResult = new TokenDTO
                {
                    accessToken = accessToken
                };

                return new ResponseUnit<TokenDTO>() { message = "요청이 정상 처리되었습니다.", data = tokenResult, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<TokenDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        #region Regacy
        
        //public async Task<ResponseUnit<WebTokenDTO>?> WebAccessTokenService(LoginDTO dto)
        //{
        //    try
        //    {
        //        /*
        //         DB 조회 - ID SELECT 기능추가해야 함.
        //         */
        //        if (String.IsNullOrWhiteSpace(dto.LoginID))
        //        {
        //            return new ResponseUnit<WebTokenDTO>() { message = "아이디를 입력하지 않았습니다.", data = null, Code = 204 };
        //        }
        //
        //        List<Claim> authClaims = new List<Claim>();
        //        authClaims.Add(new Claim("Id", dto.LoginID));
        //        authClaims.Add(new Claim("Name", "마스터유저"));
        //        authClaims.Add(new Claim("Role", "Admin"));
        //
        //        // JWT 인증 페이로드 사인 비밀키
        //        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:authSigningKey"]!));
        //
        //        JwtSecurityToken token = new JwtSecurityToken(
        //            issuer: Configuration["JWT:Issuer"],
        //            audience: Configuration["JWT:Audience"],
        //            expires: DateTime.Now.AddMinutes(15), // 15분 후 만료
        //            claims: authClaims,
        //            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
        //
        //        // accessToken
        //        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        //
        //        // RefreshToken
        //        string refreshToken = TokenComm.GenerateRefreshToken();
        //
        //        /*
        //         * 메모리 캐쉬에 Refresh토큰 저장
        //         * Key : 사용자 ID
        //         * Value : RefreshToken
        //        */
        //        var cacheEntryOptions = new MemoryCacheEntryOptions
        //        {
        //            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        //        };
        //        MemoryCache.Set(dto.LoginID, refreshToken, cacheEntryOptions);
        //
        //        var tokenResult = new WebTokenDTO
        //        {
        //            AccessToken = accessToken,
        //            RefreshToken = refreshToken
        //        };
        //        return new ResponseUnit<WebTokenDTO>() { message = "요청이 정상처리되었습니다.", data = tokenResult, Code = 200 };
        //    }
        //    catch(Exception ex)
        //    {
        //        LoggerService.FileErrorMessage(ex.ToString());
        //        return new ResponseUnit<WebTokenDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, Code = 500 };
        //    }
        //}
        //
        //
        //
        //public async Task<ResponseUnit<WebTokenDTO>?> WebRefreshTokenService(ReTokenDTO accesstoken)
        //{
        //    try
        //    {
        //        if (String.IsNullOrWhiteSpace(accesstoken.UserId))
        //        {
        //            return new ResponseUnit<WebTokenDTO>() { message = "요청이 잘못되었습니다.", data = null, Code = 204 };
        //        }
        //
        //        // 메모리 캐시에서 저장된 Refresh 토큰을 조회
        //        // Key는 사용자ID
        //        if (!MemoryCache.TryGetValue(accesstoken.UserId, out string storedRefreshToken))
        //        {
        //            Console.WriteLine("리프레쉬 토큰이 없습니다.");
        //            return new ResponseUnit<WebTokenDTO>() { message = "리프레쉬 토큰이 없습니다.", data = null, Code = 401 };
        //        }
        //
        //        // 클라이언트가 보낸 Refresh 토큰과 저장된 토큰 비교
        //        if(storedRefreshToken != accesstoken.RefreshToken)
        //        {
        //            return new ResponseUnit<WebTokenDTO>() { message = "토큰이 올바르지 않습니다.", data = null, Code = 401 };
        //        }
        //
        //        // UserId로 DB조회
        //        List<Claim> authClaims = new List<Claim>();
        //        authClaims.Add(new Claim("Id", accesstoken.UserId)); // USERID
        //        authClaims.Add(new Claim("Name", "마스터유저"));
        //        authClaims.Add(new Claim("Role", "Admin"));
        //
        //        // JWT 인증 페이로드 사인 비밀키
        //        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:authSigningKey"]!));
        //        JwtSecurityToken newToken = new JwtSecurityToken(
        //            issuer: Configuration["JWT:Issuer"],
        //            audience: Configuration["JWT:Audience"],
        //            expires: DateTime.Now.AddMinutes(15),
        //            claims: authClaims,
        //            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
        //
        //        string newAccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);
        //
        //        // (선택 사항) Refresh Token 회전: 기존 Refresh Token 무효화 후 새 Refresh Token 발급
        //        string newRefreshToken = TokenComm.GenerateRefreshToken();
        //
        //        // (선택사항) 기존의 Refresh 토큰 무효화 및 새 토큰으로 교체
        //        MemoryCache.Remove(accesstoken.UserId);
        //        MemoryCache.Set(accesstoken.UserId, newRefreshToken, new MemoryCacheEntryOptions
        //        {
        //            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        //        });
        //
        //        var tokenResult = new WebTokenDTO
        //        {
        //            AccessToken = newAccessToken,
        //            RefreshToken = newRefreshToken
        //        };
        //
        //        return new ResponseUnit<WebTokenDTO>() { message = "요청이 정상처리되었습니다.", data = tokenResult, Code = 200 };
        //    }
        //    catch(Exception ex)
        //    {
        //        LoggerService.FileErrorMessage(ex.ToString());
        //        return new ResponseUnit<WebTokenDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, Code = 500 };
        //    }
        //}
        #endregion

        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> AddUserService(RegistrationDTO dto)
        {
            try
            {
                if(dto is null)
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // Bad Request

                if (!string.IsNullOrEmpty(dto.userId) && dto.userId.Any(char.IsWhiteSpace)) // NULL 검사 + 공백검사
                {
                    // 안에 공백이든 NULL임.
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // Bad Request
                }

                if (!string.IsNullOrEmpty(dto.passWord) && dto.passWord.Any(char.IsWhiteSpace)) // NULL 검사 + 공백검사
                {
                    // 안에 공백이든 NULL임.
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // Bad Request
                }

                DateTime ThisDate = DateTime.Now;

                // UserModel 생성
                var model = new LoginTb
                {
                    Uid = dto.userId!.ToLower(),
                    Pwd = dto.passWord!,
                    MasterYn = false,
                    AdminYn = false,
                    CreateDt = ThisDate,
                    UpdateDt = ThisDate,
                    DelYn = false,
                    UseYn = false
                };

                /* 사용자 ID 중복검사 */
                int UesrIDCheck = await UserRepository.CheckUserIdAsync(model.Uid).ConfigureAwait(false);
                if (UesrIDCheck > 0)
                    return new ResponseUnit<bool>() { message = "이미 존재하는 아이디입니다.", data = false, code = 200 };
                else if(UesrIDCheck < 0)
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
                
                /* 사용자 ID 등록 */
                int result = await UserRepository.AddUserAsync(model).ConfigureAwait(false);
                if (result > 0)
                    return new ResponseUnit<bool>() { message = "회원가입이 완료되었습니다.", data = true, code = 200 };
                else if (result == 0)
                    return new ResponseUnit<bool>() { message = "회원가입에 실패했습니다.", data = false, code = 200 };
                else
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

        /// <summary>
        /// 사용자 ID 검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> CheckUserIdService(UserIDCheckDTO dto)
        {
            try
            {
                if(dto is null)
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // Bad Request

                if (!string.IsNullOrEmpty(dto.userId) && dto.userId.Any(char.IsWhiteSpace)) // NULL 검사 + 공백검사
                {
                    // 안에 공백이든 NULL임.
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // Bad Request
                }

                int result = await UserRepository.CheckUserIdAsync(dto.userId!).ConfigureAwait(false);
                if (result > 0)
                    return new ResponseUnit<bool>() { message = "이미 존재하는 아이디입니다.", data = false, code = 200 };
                else if (result < 0)
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
                else
                    return new ResponseUnit<bool>() { message = "사용가능한 아이디입니다.", data = true, code = 200 };
            }
            catch (Exception ex) 
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

        /// <summary>
        /// 사용자 전체 리스트 반환
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseList<UserListDTO>?> GetUserListService()
        {
            try
            {
                var model = await UserRepository.GetUserListAsync().ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<UserListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };

                List<UserListDTO> dto = model.Select( m => new UserListDTO
                {
                    pId = m.Pid,
                    uId = m.Uid,
                    adminYn = m.AdminYn,
                    useYn = m.UseYn,
                    createDt = m.CreateDt.ToString("HH:mm:ss")
                }).ToList();

                return new ResponseList<UserListDTO>() { message = "요청이 정상 처리되었습니다.", data = dto, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<UserListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// 사용자 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> UpdateUserService(UserUpdateDTO dto)
        {
            try
            {
                if (dto is null)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                if(dto.pId == 0)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                if(dto.uId is null)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                if (dto.pwd is null)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                var UserModelCheck = await UserRepository.GetUserInfoAsyncById(dto.pId).ConfigureAwait(false);
                if (UserModelCheck is null)
                    return new ResponseUnit<bool>() { message = "해당 아이디가 존재하지 않습니다.", data = false, code = 200 };

                if(UserModelCheck.Uid != dto.uId)
                    return new ResponseUnit<bool>() { message = "해당 아이디가 존재하지 않습니다.", data = false, code = 200 };

                UserModelCheck.CountryId = dto.countryId;
                UserModelCheck.Pwd = dto.pwd;
                UserModelCheck.AdminYn = dto.adminYn;
                UserModelCheck.UseYn = dto.useYn;
                UserModelCheck.UpdateDt = DateTime.Now;
              
                int result = await UserRepository.EditUserAsync(UserModelCheck).ConfigureAwait(false);
                if (result != -1)
                    return new ResponseUnit<bool>() { message = "수정이 완료되었습니다.", data = true, code = 200 };
                else
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

        /// <summary>
        /// 사용자 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> DeleteUserService(int pid)
        {
            try
            {
                if(pid == 0)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                var UserModelCheck = await UserRepository.GetUserInfoAsyncById(pid).ConfigureAwait(false);
                if (UserModelCheck is null)
                    return new ResponseUnit<bool>() { message = "해당 아이디가 존재하지 않습니다.", data = false, code = 200 };

                UserModelCheck.Uid = $"{UserModelCheck.Pid}_{UserModelCheck.Uid}";
                UserModelCheck.UpdateDt = DateTime.Now;
                UserModelCheck.DelYn = true;
                UserModelCheck.DeleteDt = DateTime.Now;

                int result = await UserRepository.DeleteUserAsync(UserModelCheck).ConfigureAwait(false);
                if (result != -1)
                    return new ResponseUnit<bool>() { message = "수정이 완료되었습니다.", data = true, code = 200 };
                else
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }
    }
}
