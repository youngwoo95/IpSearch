using IpManager.Comm.Tokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IpManager
{
    public class TokenMiddleWare
    {
        private readonly RequestDelegate Next;
        private ITokenComm TokenComm;
        private readonly string? _authSigningkey;

        public TokenMiddleWare(RequestDelegate _next, ITokenComm _tokencomm, IConfiguration configuration)
        {
            this.Next = _next;
            this.TokenComm = _tokencomm;
            this._authSigningkey = configuration["JWT:AuthSigningKey"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Headers.TryGetValue("Authorization", out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //await context.Response.WriteAsync("Api Key was not provided. (Using ApiKeyMiddleware)");
                return;
            }

            string? accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if(String.IsNullOrWhiteSpace(accessToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //await context.Response.WriteAsync("Jwt Token Validation Failed");
                return;
            }

            if (String.IsNullOrWhiteSpace(_authSigningkey))
                return;

            var tokenHandler = new JwtSecurityTokenHandler();
            var authSigningKey = Encoding.UTF8.GetBytes(_authSigningkey);

            try
            {
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

                if (jobj["UserIdx"] == null ||
                    jobj["jti"] == null ||
                    jobj["Role"] == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }

                context.Items.Add("UserIdx", jobj["UserIdx"]!.ToString());
                context.Items.Add("jti", jobj["jti"]!.ToString());
                context.Items.Add("Role", jobj["Role"]!.ToString());
                
                await Next(context);

                return;
            }
            catch(SecurityTokenExpiredException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

        }

    }
}
