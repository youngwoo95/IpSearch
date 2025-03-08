using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Country;
using IpManager.Services.Country;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService CountryService;
        private readonly ILoggerService LoggerService;

        public CountryController(ICountryService _countryservice,
            ILoggerService _loggerservice)
        {
            this.CountryService = _countryservice;
            this.LoggerService = _loggerservice;
        }

        /// <summary>
        /// 도시 리스트 반환
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCountryInfo")]
        public async Task<IActionResult> GetCountryInfo()
        {
            try
            {
                ResponseList<CountryDataDTO>? model = await CountryService.GetCountryListService().ConfigureAwait(false);
                if(model is null)
                    return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master")]
        [HttpPut]
        [Route("sign/v1/DeleteCountryInfo")]
        public async Task<IActionResult> DeleteCountryInfo([FromBody]List<int> pid)
        {
            try
            {
                ResponseUnit<bool>? model = await CountryService.DeleteCountryListService(pid).ConfigureAwait(false);
                if(model is null)
                    return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);

                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }
    }
}
