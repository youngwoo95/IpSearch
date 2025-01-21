using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ILoggerFactory = IpManager.Comm.Logger.ILoggerFactory;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoggerFactory LoggerFactory;

        public LoginController(ILoggerFactory _loggerFactory)
        {
            this.LoggerFactory = _loggerFactory;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var consoleLogger = LoggerFactory.CreateLogger(false);
            consoleLogger.LogMessage("Logging to console.");
            //consoleLogger.ErrorMessage("Logging to console.");

            return Ok("123123");
        }

    }
}
