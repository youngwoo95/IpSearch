using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Store;
using IpManager.Services.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpManager.Controllers
{
    public class StoreController : Controller
    {
        private readonly ILoggerService LoggerService;
        private readonly IStoreService StoreService;

        public StoreController(ILoggerService _loggerservice,
            IStoreService _storeservice)
        {
            this.LoggerService = _loggerservice;
            this.StoreService = _storeservice;
        }

        /// <summary>
        /// PC방 정보 등록
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles ="Manager")] // 매니저만 등록가능
        [HttpGet]
        [Route("sign/v1/AddStore")]
        public async Task<IActionResult> AddStore(StoreDTO dto)
        {
            try
            {
                ResponseUnit<bool> model = await StoreService.AddPCRoomService(dto).ConfigureAwait(false);

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
        /// 등록된 PC방 정보 리스트보기
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreList")]
        public async Task<IActionResult> GetStoreList([FromQuery]string? search, [FromQuery]int pagenumber)
        {
            try
            {
                if (pagenumber == 0)
                    return BadRequest();

                ResponseList<StoreListDTO>? model = await StoreService.GetPCRoomListService(search, 15, pagenumber - 1).ConfigureAwait(false);
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
        /// 상세정보 보기
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreDetail")]
        public async Task<IActionResult> GetStoreDetail([FromQuery]int pid)
        {
            try
            {
                ResponseUnit<StoreDetailDTO>? model = await StoreService.GetPCRoomDetailService(pid);
                
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


        // Update
        // Delete




    }
}
