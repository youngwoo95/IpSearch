﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
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
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreList")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerStoreListDTO))]
        public async Task<IActionResult> GetStoreList([FromQuery] string? search)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPCRoomListService(Pid, userType, search).ConfigureAwait(false);
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
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/SendIpPing")]
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<StorePingDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerStorePingDTO))]
        public async Task<IActionResult> GetUsedPcCount([FromQuery][Required] int pid)
        {
            try
            {
                if (pid == 0)
                    return BadRequest();

                ResponseUnit<StorePingDTO>? model = await StoreService.GetUsedPcCountService(pid).ConfigureAwait(false);
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
        [SwaggerResponse(200, "성공", typeof(ResponseUnit<bool>))]
        [SwaggerResponseExample(200, typeof(SwaggerAddStoreDTO))]
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
        /// PC방 이름으로 PC방 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreSearchName")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerSearchNameStoreDTO))]
        public async Task<IActionResult> GetStoreSearchName([FromQuery]string? search)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomSearchNameListService(Pid, userType, search).ConfigureAwait(false);
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
        /// PC방 주소로 PC방 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreSearchAddress")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreListDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerSearchAddrStoreDTO))]
        public async Task<IActionResult> GetStoreSearchAddress([FromQuery]string? search)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomSearchAddressListService(Pid, userType, search).ConfigureAwait(false);
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
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreGroupList")]
        [SwaggerResponse(200, "성공", typeof(ResponseList<StoreRegionDTO>))]
        [SwaggerResponseExample(200, typeof(SwaggerStoreGroupListDTO))]
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
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetStoreDetail")]
        public async Task<IActionResult> GetStoreDetail([FromQuery]int pid)
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

                ResponseUnit<StoreDetailDTO>? model = await StoreService.GetPCRoomDetailService(pid, UserPid, userType).ConfigureAwait(false);
                
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

        // (도/시) 별 조회 Pc방 List
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCountryStoreList")]
        public async Task<IActionResult> GetCountryStoreList([FromQuery]int countryid)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomCountryListService(countryid, Pid, userType).ConfigureAwait(false);
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

        // (시/군/구) 별 조회 Pc방 List
        [Authorize(Roles = "Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetCityStoreList")]
        public async Task<IActionResult> GetCityStoreList([FromQuery]int cityid)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomCityListService(cityid, Pid, userType).ConfigureAwait(false);
                
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



        // (읍/면/동) 별 조회 Pc방 List
        [Authorize(Roles ="Manager,Visitor")]
        [HttpGet]
        [Route("sign/v1/GetTownStoreList")]
        public async Task<IActionResult> GetTownStoreList([FromQuery]int townid)
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

                ResponseList<StoreListDTO>? model = await StoreService.GetPcRoomTownListService(townid, Pid, userType).ConfigureAwait(false);
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
        [Authorize(Roles = "Manager,Visitor")]
        [HttpPut]
        [Route("sign/v1/UpdateStore")]
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


        // Delete
        [Authorize(Roles = "Manger,Visitor")]
        [HttpPut]
        [Route("sign/v1/DeleteStore")]
        public async Task<IActionResult> DeleteStoreInfo([FromBody]int pid)
        {
            try
            {
                ResponseUnit<bool> model = await StoreService.DeleteStoreService(pid).ConfigureAwait(false);
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
