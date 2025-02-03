using IpManager.Comm.Logger.LogFactory;
using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace IpManager.Comm.Tokens
{
    public class TokenComm : ITokenComm
    {
        private readonly string? _authSigningKey;
        private readonly ILoggerModels Logger;

        public TokenComm(IConfiguration configuration,
            ILoggers _loggerFactory)
        {
            this._authSigningKey = configuration["JWT:AuthSigningKey"];
            this.Logger = _loggerFactory.CreateLogger(false);
        }

        public JObject? TokenConvert(HttpRequest? token)
        {
            try
            {
                if (token is not null && !String.IsNullOrWhiteSpace(_authSigningKey))
                {
                    string? accessToken = token.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                    var authSigningKey = Encoding.UTF8.GetBytes(_authSigningKey);

                    var tokenHandler = new JwtSecurityTokenHandler();
                    tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(authSigningKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validateToken);

                    int split = validateToken.ToString()!.IndexOf('.') + 1;

                    string payload = validateToken.ToString()!.Substring(split, validateToken.ToString()!.Length - split);
                    var jobj = JObject.Parse(payload.ToString());
                    return jobj;
                }
                else
                    return null;
            }
            catch(Exception ex)
            {
                Logger.ErrorMessage(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// RefreshToken 용 랜덤코드 생성
        /// </summary>
        /// <returns></returns>
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
