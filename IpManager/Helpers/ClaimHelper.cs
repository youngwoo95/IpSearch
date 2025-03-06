using System.Security.Claims;

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
