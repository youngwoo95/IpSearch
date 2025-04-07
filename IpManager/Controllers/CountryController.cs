using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Country;
using IpManager.Services.Country;
using IpManager.SwaggerExample;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

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
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCountryInfo")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<CountryDataDTO>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<CountryDataDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "도시 LIST 반환",
Description = "권한제한 있음 - Manager, Visitor만 가능")]
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
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "도시 정보 삭제",
Description = "권한제한 있음 - Master만 가능")]
        public async Task<IActionResult> DeleteCountryInfo([FromBody][Required]List<int> pId)
        {
            try
            {
                ResponseUnit<bool>? model = await CountryService.DeleteCountryListService(pId).ConfigureAwait(false);
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
