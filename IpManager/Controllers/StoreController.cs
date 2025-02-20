using IpManager.Comm.Logger.LogFactory;
using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpManager.Controllers
{
    public class StoreController : Controller
    {
        private readonly ILoggerService LoggerService;

        public StoreController(ILoggerService _loggerservice)
        {
            this.LoggerService = _loggerservice;
        }


        [Authorize(Roles ="Admin")]
        [HttpGet]
        [Route("sign/v1/AddStore")]
        public async Task<IActionResult> AddStore()
        {
            return Ok("dsafasdf");
        }

    }
}
