using Newtonsoft.Json.Linq;

namespace IpManager.Comm.Tokens
{
    public class TokenComm : ITokenComm
    {
        private readonly string? _authSigningKey;
        private readonly ILoggerFactory LoggerFactory;

        public TokenComm(IConfiguration configuration,
            ILoggerFactory _loggerFactory)
        {
            this._authSigningKey = configuration["JWT:AuthSigningKey"];
            this.LoggerFactory = _loggerFactory;
        }

        public JObject? TokenConvert(HttpRequest? token)
        {
            throw new NotImplementedException();
        }
    }
}
