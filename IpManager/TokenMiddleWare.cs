using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.Comm.Tokens;
using IpManager.DTO.Login;
using IpManager.Repository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace IpManager
{
    public class TokenMiddleWare
    {
        private readonly RequestDelegate Next;
        private ITokenComm TokenComm;
        private readonly string? AuthSigningkey;
        private readonly string? Issuer;
        private readonly string? Audience;
        private readonly ILoggerService LoggerService;
        private readonly IUserRepository UserRepository;


        public TokenMiddleWare(RequestDelegate _next,
            ITokenComm _tokencomm,
            IConfiguration configuration,
            ILoggerService _loggerservice,
            IUserRepository _userrepository)
        {
            this.Next = _next;
            this.TokenComm = _tokencomm;
            this.LoggerService = _loggerservice;
            this.AuthSigningkey = configuration["JWT:AuthSigningKey"];
            this.Issuer = configuration["JWT:Issuer"];
            this.Audience = configuration["JWT:Audience"];
            this.UserRepository = _userrepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Headers.TryGetValue("Authorization", out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            string? accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if(String.IsNullOrWhiteSpace(accessToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (String.IsNullOrWhiteSpace(AuthSigningkey))
                return;

            var tokenHandler = new JwtSecurityTokenHandler();
            var authSigningKey = Encoding.UTF8.GetBytes(AuthSigningkey);

            try
            {
                // 만료되면 여기서 Catch로 던짐
                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(authSigningKey),
                    ValidateIssuer = false,
                    ValidateAudience =  false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // 토큰분해
                var jobj = TokenComm.TokenConvert(context.Request);

                if (jobj is null)
                    return;

                if (jobj["UserID"] == null || 
                    jobj["Role"] == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }

                context.Items.Add("UserID", jobj["UserID"]!.ToString());
                context.Items.Add("Role", jobj["Role"]!.ToString());
                
                await Next(context);

                return;
            }
            catch(SecurityTokenExpiredException ex) // 토큰만료 - 재발급 로직
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(authSigningKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // 만료 여부는 무시
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    // 만료된 토큰이라도 서명 및 기타 조건은 검증되어 유효한 클레임을 얻을 수 있음
                    var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken _);

                    // 토큰에서 UserID를 추출
                    var UserId = principal.Claims.FirstOrDefault(m => m.Type == "UserID")?.Value;
                    if(String.IsNullOrWhiteSpace(UserId)) // 기존 토큰에서 UserID가 없다?? --> 변조된 토큰임. 400 BadRequest
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    // UserRepository
                    // [1]. UserID 검사 -- 1이어야함. --> ID가 있다는 뜻임.
                    int UserCheck = await UserRepository.CheckUserIdAsync(UserId).ConfigureAwait(false); 
                    if(UserCheck != 1) // 없다? --> 변조된 토큰임.
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    // [2]. 승인여부 다시조회 - 1이어야함 아니면 로그인허용상태가 바뀐거임.
                    int LoginPermissionCheck = await UserRepository.GetLoginPermission(UserId);
                    if(LoginPermissionCheck != 1)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    // [3]. 마스터, 관리자, 일반 Visitor 인지 권한검사
                    var model = await UserRepository.GetUserInfoAsync(UserId).ConfigureAwait(false);
                    // 기존 principal.Claims를 리스트로 반환
                    var claims = principal.Claims.ToList();
                    // 역할 관련 클레임 제거 (ClaimTypes.Role 또는 "Role")
                    claims.RemoveAll(c => c.Type == ClaimTypes.Role || c.Type == "Role");
                    claims.RemoveAll(c => c.Type == JwtRegisteredClaimNames.Aud);

                    // 새로이 권한정보 생성
                    if (model.MasterYn)
                        claims.Add(new Claim("Role", "Master")); // 마스터
                    else if (model.AdminYn)
                        claims.Add(new Claim("Role", "Manager")); // 매니저
                    else
                        claims.Add(new Claim("Role", "Visitor")); // 방문자

                    // 여기서 principal.Claims를 기반으로 새로운 토큰을 생성합니다.
                    var newAccessToken = GenerateNewJwtToken(claims);

                    // 새 토큰을 클라이언트에 반환합니다.
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    context.Response.ContentType = "application/json";

                    var dto = new TokenDTO()
                    {
                        AccessToken = newAccessToken
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new ResponseUnit<TokenDTO>()
                    {
                        message = "토큰이 재발급되었습니다.",
                        data = dto,
                        Code = 201
                    }));
                    return;
                }
                catch (Exception innerEx)
                {
                    // 재발급 시에도 오류가 발생하면 401 처리
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    LoggerService.FileErrorMessage(innerEx.ToString());
                    return;
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
        }

        public string GenerateNewJwtToken(IEnumerable<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthSigningkey!));

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                //expires: DateTime.Now.AddDays(30), // 한달 뒤 만료
                expires: DateTime.Now.AddSeconds(30), // 한달 뒤 만료
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
