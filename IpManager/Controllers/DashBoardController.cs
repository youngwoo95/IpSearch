using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.DashBoard;
using IpManager.Services.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly ILoggerService LoggerService;
        private readonly IDashBoardService DashBoardService;

        public DashBoardController(ILoggerService _loggerservice,
            IDashBoardService _dashboardservice)
        {
            this.LoggerService = _loggerservice;
            this.DashBoardService = _dashboardservice;
        }

        /// <summary>
        /// 현황(실시간)조회
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetThisTimeDataList")]
        public async Task<IActionResult> GetThisTimeDataList()
        {
            try
            {
                ResponseUnit<AnalysisDataDTO>? model = await DashBoardService.GetThisTimeDataService().ConfigureAwait(false);
                if (model is null)
                    return BadRequest();
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
        /// 매출 1위 상권
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetTopAnalyzeName")]
        public async Task<IActionResult> GetTopAnalyName()
        {
            try
            {

                ResponseUnit<TopSalesNameDTO>? model = await DashBoardService.GetTopSalesNameService().ConfigureAwait(false);
                if (model is null)
                    return BadRequest();
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
        /// 해당 날짜 전체 분석데이터 조회
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetThisDayDataList")]
        public async Task<IActionResult> GetThisDayDataList([FromQuery][Required] DateTime targetDate,[FromQuery]string? pcName, [FromQuery]int? countryTbId, [FromQuery]int? townTbId, [FromQuery]int? cityTbId)
        {
            try
            {

                ResponseList<PcroomTimeDataDto>? model = await DashBoardService.GetThisDayDataService(targetDate,pcName, countryTbId, townTbId, cityTbId).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

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
        /// 기간별 분석
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetPeriodList")]
        public async Task<IActionResult> GetPeriodList([FromQuery][Required]DateTime startDate, [FromQuery][Required]DateTime endDate, [FromQuery]string? pcName, [FromQuery]int? countryTbId, [FromQuery]int? townTbId, [FromQuery]int? citryTbId)
        {
            try
            {
                ResponseList<ReturnValue>? model = await DashBoardService.GetPeriodDataService(startDate, endDate, pcName, countryTbId, townTbId, citryTbId);
                if (model is null)
                    return BadRequest();

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
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <param name="pcName"></param>
        /// <param name="countryTbId"></param>
        /// <param name="townTbId"></param>
        /// <param name="cityTbId"></param>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetMonthDataList")]
        public async Task<IActionResult> GetMonthDataList([FromQuery][Required]DateTime TargetDate, [FromQuery]string? pcName, [FromQuery] int? countryTbId, [FromQuery]int? townTbId, [FromQuery] int? cityTbId)
        {
            try
            {
                ResponseList<ReturnValue>? model = await DashBoardService.GetMonthDataService(TargetDate, pcName, countryTbId, townTbId, cityTbId).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

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


        ///// <summary>
        ///// 하루 데이터 조회
        ///// </summary>
        ///// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetDaysDataList")]
        public async Task<IActionResult> GetDaysDataList([FromQuery][Required] DateTime Target,[FromQuery] string? pcName, [FromQuery]int? countryTbId, [FromQuery]int? townTbId, [FromQuery] int? cityTbId)
        {
            try
            {
                ResponseList<ReturnValue>? model = await DashBoardService.GetDaysDataService(Target,pcName,countryTbId, townTbId,cityTbId).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();

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