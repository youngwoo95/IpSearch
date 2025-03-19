using IpManager.DTO.Login;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IpManager.Helpers
{
    public static class ClaimHelper
    {
        /// <summary>
        /// 사용자 역할(Visitor, Manager 등)을 추출해 int로 반환
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetUserType(this ClaimsPrincipal user)
        {
            try
            {
                if (user == null) return -1;

                string? role = user.Claims
                    .Where(m => m.Type == "Role")
                    .Select(m => m.Value)
                    .FirstOrDefault();

                if (role == "Visitor") return 0;
                else if (role == "Manager") return 1;
                else return -1;
            }catch(Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Role 반환
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<LoginRoleDTO?> GetUserRole(this ClaimsPrincipal user, HttpContext context)
        {
            try
            {
                if (user == null) return null;

                string? accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (accessToken is null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("jwt token validation failed");
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var authSigningKey = Encoding.UTF8.GetBytes("d5b2e5c7a657f134f879f66f0712578416bed6d698a68d01fbde730b64c45e98");

                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(authSigningKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;

                if(jwtToken is null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return null;
                }

                var pid = jwtToken.Claims.FirstOrDefault(m => m.Type == "userPid")?.Value;
                var uid = jwtToken.Claims.FirstOrDefault(m => m.Type == "userId")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(m => m.Type == "Role")?.Value;

                if (pid == null || !int.TryParse(pid, out int pId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return null;
                }

                var model = new LoginRoleDTO
                {
                    pId = pId,
                    uId = uid,
                    Role = role,
                };

                return model;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// userPid 클레임을 추출해 int로 반환. 변환 실패 시 -1
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetUserPid(this ClaimsPrincipal user)
        {
            try
            {
                if (user == null) return -1;

                string? pidString = user.Claims
                    .Where(m => m.Type == "userPid")
                    .Select(m => m.Value)
                    .FirstOrDefault();

                if (String.IsNullOrWhiteSpace(pidString))
                    return -1;
                
                if (!int.TryParse(pidString, out int pid))
                    return -1;

                return pid < 1 ? -1 : pid;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }


    }
}
