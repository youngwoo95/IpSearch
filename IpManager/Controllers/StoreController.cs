using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Store;
using IpManager.Helpers;
using IpManager.Services.Store;
using IpManager.SwaggerExample;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace IpManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        /// 등록된 PC방 정보 리스트보기
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        //[SwaggerResponseExample(200, typeof(StoreListDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "등록된 PC방 리스트 보기",
            Description = "권한 제한 있음 - Manager & Visitor" +
            "Manager - 등록된 전체 PC방 리스트 조회가능 / Visitor 자기지역만 가능 / 매개변수 추가시 매개변수 조건에 해당하는 검색결과 반환 [이름검색과 전체검색 API 통합 [수정완료]]")]
        public async Task<IActionResult> GetStoreList([FromQuery] string? searchPcName)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreListDTO>? model = await StoreService.GetPCRoomListService(Pid, userType, searchPcName).ConfigureAwait(false);
                if (model is null)
                    return BadRequest();
                if (model.code == 200)
                    return Ok(model);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return Problem("서버에서 처리할 수 없는 요청입니다.", statusCode: 500);
            }
        }

        /// <summary>
        /// PC 방 PING SEND
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/SendIpPing")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<StorePingDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerStorePingDTO))]
        [SwaggerResponseExample(200, typeof(StoreListDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<StorePingDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 단일 PING SEND",
      Description = "권한 제한 있음 - Manager & Visitor" +
      "Manager - 등록된 전체 PC방 PING SEND 가능 / Visitor 자기지역만 가능")]
        public async Task<IActionResult> GetUsedPcCount([FromQuery][Required] int pId)
        {
            try
            {
                if (pId == 0)
                    return BadRequest();

                ResponseUnit<StorePingDTO>? model = await StoreService.GetUsedPcCountService(pId).ConfigureAwait(false);
                if (model is null)
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
        /// PC방 정보 등록
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles ="Master")] // 마스터만 등록가능
        [HttpPost]
        [Route("sign/v1/AddStore")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 정보 등록",
    Description = "권한제한 있음 - Master만 가능")]
        public async Task<IActionResult> AddStore([FromBody]StoreDTO dto)
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
        /// PC방 주소로 PC방 검색 - 이거 왜 만들었는지 기억이안남.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreSearchAddress")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerSearchAddrStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 주소로 PC방 검색",
    Description = "권한제한 있음 - Manager는 전체 지역 다검색가능 / Visitor는 자기지역만 [API 왜 만들었는지 기억이안남.]")]
        public async Task<IActionResult> GetStoreSearchAddress([FromQuery][Required]string searchAddress)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomSearchAddressListService(Pid, userType, searchAddress).ConfigureAwait(false);
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
        /// PC방 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreGroupList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreRegionDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerStoreGroupListDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreRegionDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 그룹핑 개수 카운팅",
Description = "권한제한 있음 - Manager는 전체 카운팅 / Visitor는 자기지역만 Counting")]
        public async Task<IActionResult> GetStoreGroupList()
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreRegionDTO>? model = await StoreService.GetPcRoomRegionListService(Pid, userType).ConfigureAwait(false);
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
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreDetail")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<StoreDetailDTO>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<StoreDetailDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 상세정보 보기",
Description = "권한제한 있음 - Manger는 전체리스트에 대해서 정보보기 가능 / Visitor는 할당된 지역만 가능")]
        public async Task<IActionResult> GetStoreDetail([FromQuery][Required] int pId)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int UserPid = User.GetUserPid();
                if (UserPid == -1)
                    return Unauthorized();

                ResponseUnit<StoreDetailDTO>? model = await StoreService.GetPCRoomDetailService(pId, UserPid, userType).ConfigureAwait(false);
                
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
        /// (도/시) 별 조회 Pc방 List
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCountryStoreList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "(도/시) 별 Pc방 List 조회",
        Description = "Manager - 전체 대상 / Visitor - 자기지역만")]
        public async Task<IActionResult> GetCountryStoreList([FromQuery][Required]int countryId)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomCountryListService(countryId, Pid, userType).ConfigureAwait(false);
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
        /// (시/군/구) 별 조회 Pc방 List
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCityStoreList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "(시/군/구) 별 Pc방 List 조회",
        Description = "Manager - 전체 대상 / Visitor - 자기지역만")]
        public async Task<IActionResult> GetCityStoreList([FromQuery][Required]int cityId)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomCityListService(cityId, Pid, userType).ConfigureAwait(false);
                
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
        /// (읍/면/동) 별 조회 Pc방 List
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetTownStoreList")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseList<StoreListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "(읍/면/동) 별 Pc방 List 조회",
        Description = "Manager - 전체 대상 / Visitor - 자기지역만")]
        public async Task<IActionResult> GetTownStoreList([FromQuery][Required]int townId)
        {
            try
            {
                // 권한 검사
                int userType = User.GetUserType();
                if (userType == -1)
                    return Unauthorized();

                int Pid = User.GetUserPid();
                if (Pid == -1)
                    return Unauthorized();

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomTownListService(townId, Pid, userType).ConfigureAwait(false);
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
        /// PC방 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manager,Visitor")]
        [HttpPut]
        [Route("sign/v1/UpdateStore")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 정보 수정",
        Description = "권한제한 있음 - Manager, Visitor만 가능")]
        public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreDTO dto)
        {
            try
            {
                ResponseUnit<bool> model = await StoreService.UpdateStoreService(dto).ConfigureAwait(false);
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
        /// PC방 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Master,Manger,Visitor")]
        [HttpPut]
        [Route("sign/v1/DeleteStore")]
        [Produces("application/json")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        //[SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseUnit<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "PC방 정보 삭제",
        Description = "권한제한 있음 - Manager, Visitor만 가능")]
        public async Task<IActionResult> DeleteStoreInfo([FromBody][Required]DeleteStoreDTO dto)
        {
            try
            {
                if (dto.pId == 0)
                    return NoContent();

                ResponseUnit<bool> model = await StoreService.DeleteStoreService(dto.pId).ConfigureAwait(false);
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
