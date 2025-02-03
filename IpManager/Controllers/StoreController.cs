using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpManager.Controllers
{
    public class StoreController : Controller
    {
        [Authorize(Roles ="Admin")]
        [HttpGet]
        [Route("sign/v1/AddStore")]
        public async Task<IActionResult> AddStore()
        {
            return Ok("dsafasdf");
        }

    }
}
