using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private readonly ILoggerService LoggerService;



        public PingController(ILoggerService _loggerservice)
        {
            this.LoggerService = _loggerservice;
        }


    }
}
