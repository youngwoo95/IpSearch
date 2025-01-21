using Newtonsoft.Json.Linq;

namespace IpManager.Comm.Tokens
{
    public interface ITokenComm
    {
        /// <summary>
        /// 토큰 분해
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public JObject? TokenConvert(HttpRequest? token);
    }
}
