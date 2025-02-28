using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.DashBoard;
using IpManager.Services.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// 하루 데이터 조회
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetTodayDataList")]
        public async Task<IActionResult> GetTodayDataList([FromQuery] DateTime Target)
        //public async Task<IActionResult> GetTodayDataList([FromQuery]DateTime Target)
        {
            try
            {
                //DateTime Target = DateTime.Now; // FromQuery절로 올라가야함.
                //DateTime Target = DateTime.Now.AddDays(-1); // FromQuery절로 올라가야함.

                ResponseUnit<AnalysisDataDTO>? model = await DashBoardService.GetTodayDataService(Target).ConfigureAwait(false);
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
        /// 주간 데이터 조회
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetWeeksDataList")]
        public async Task<IActionResult> GetWeeksDataList([FromQuery] DateTime Target)
        //public async Task<IActionResult> GetWeeksDataList([FromQuery] DateTime Target)
        {
            try
            {
                //DateTime Target = DateTime.Now.AddDays(-1);
                //DateTime Target = DateTime.Now;

                ResponseUnit<AnalysisDataDTO>? model = await DashBoardService.GetWeeksDataService(Target).ConfigureAwait(false);
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

        // 월간 데이터 조회
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetMonthDataList")]
        public async Task<IActionResult> GetMonthDataList([FromQuery] DateTime Target)
        {
            try
            {
                //DateTime Target = DateTime.Now.AddDays(-1);
                //DateTime Target = DateTime.Now;

                ResponseUnit<AnalysisDataDTO>? model = await DashBoardService.GetMonthDataService(Target).ConfigureAwait(false);
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

        // 연간 데이터 조회
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetYearDataList")]
        public async Task<IActionResult> GetYearDataList([FromQuery] DateTime Target)
        {
            try
            {
                //DateTime Target = DateTime.Now;

                ResponseUnit<AnalysisDataDTO>? model = await DashBoardService.GetYearDataService(Target).ConfigureAwait(false);
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