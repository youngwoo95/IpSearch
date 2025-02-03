using IpManager.Comm.Logger.LogFactory;
using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.Comm.Tokens;
using IpManager.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IpManager.Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly ILoggerModels Logger;
        private readonly IConfiguration Configuration;
        private readonly IMemoryCache MemoryCache; // 메모리캐쉬

        public LoginService(ILoggers _loggerFactory,
            IConfiguration _configuration,
            IMemoryCache _memorycache)
        {
            this.Logger = _loggerFactory.CreateLogger(false);
            this.Configuration = _configuration;
            this.MemoryCache = _memorycache;
        }


        public async Task<ResponseUnit<TokenDTO>?> AccessTokenService(LoginDTO dto)
        {
            try
            {
                /*
                 DB 조회 - ID SELECT 기능추가해야 함.
                 */
                if (String.IsNullOrWhiteSpace(dto.LoginID))
                {
                    return new ResponseUnit<TokenDTO>() { message = "아이디를 입력하지 않았습니다.", data = null, Code = 204 };
                }

                List<Claim> authClaims = new List<Claim>();
                authClaims.Add(new Claim("Id", dto.LoginID));
                authClaims.Add(new Claim("Name", "마스터유저"));
                authClaims.Add(new Claim("Role", "Admin"));

                // JWT 인증 페이로드 사인 비밀키
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:authSigningKey"]!));

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: Configuration["JWT:Issuer"],
                    audience: Configuration["JWT:Audience"],
                    expires: DateTime.Now.AddMinutes(15), // 15분 후 만료
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                // accessToken
                string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

                // RefreshToken
                string refreshToken = TokenComm.GenerateRefreshToken();

                /*
                 * 메모리 캐쉬에 Refresh토큰 저장
                 * Key : 사용자 ID
                 * Value : RefreshToken
                */
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                };
                MemoryCache.Set(dto.LoginID, refreshToken, cacheEntryOptions);

                var tokenResult = new TokenDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
                return new ResponseUnit<TokenDTO>() { message = "요청이 정상처리되었습니다.", data = tokenResult, Code = 200 };
            }
            catch(Exception ex)
            {
                Logger.ErrorMessage(ex.ToString());
                return new ResponseUnit<TokenDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, Code = 500 };
            }
        }

        public async Task<ResponseUnit<TokenDTO>?> RefreshTokenService(ReTokenDTO accesstoken)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(accesstoken.UserId))
                {
                    return new ResponseUnit<TokenDTO>() { message = "요청이 잘못되었습니다.", data = null, Code = 204 };
                }

                // 메모리 캐시에서 저장된 Refresh 토큰을 조회
                // Key는 사용자ID
                if (!MemoryCache.TryGetValue(accesstoken.UserId, out string storedRefreshToken))
                {
                    Console.WriteLine("리프레쉬 토큰이 없습니다.");
                    return new ResponseUnit<TokenDTO>() { message = "리프레쉬 토큰이 없습니다.", data = null, Code = 401 };
                }

                // 클라이언트가 보낸 Refresh 토큰과 저장된 토큰 비교
                if(storedRefreshToken != accesstoken.RefreshToken)
                {
                    return new ResponseUnit<TokenDTO>() { message = "토큰이 올바르지 않습니다.", data = null, Code = 401 };
                }

                // UserId로 DB조회
                List<Claim> authClaims = new List<Claim>();
                authClaims.Add(new Claim("Id", accesstoken.UserId)); // USERID
                authClaims.Add(new Claim("Name", "마스터유저"));
                authClaims.Add(new Claim("Role", "Admin"));

                // JWT 인증 페이로드 사인 비밀키
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:authSigningKey"]!));
                JwtSecurityToken newToken = new JwtSecurityToken(
                    issuer: Configuration["JWT:Issuer"],
                    audience: Configuration["JWT:Audience"],
                    expires: DateTime.Now.AddMinutes(15),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                string newAccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);

                // (선택 사항) Refresh Token 회전: 기존 Refresh Token 무효화 후 새 Refresh Token 발급
                string newRefreshToken = TokenComm.GenerateRefreshToken();

                // (선택사항) 기존의 Refresh 토큰 무효화 및 새 토큰으로 교체
                MemoryCache.Remove(accesstoken.UserId);
                MemoryCache.Set(accesstoken.UserId, newRefreshToken, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                });

                var tokenResult = new TokenDTO
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };

                return new ResponseUnit<TokenDTO>() { message = "요청이 정상처리되었습니다.", data = tokenResult, Code = 200 };
            }
            catch(Exception ex)
            {
                Logger.ErrorMessage(ex.ToString());
                return new ResponseUnit<TokenDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, Code = 500 };
            }
        }
    }
}
