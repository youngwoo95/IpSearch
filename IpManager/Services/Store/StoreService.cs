using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Store;
using IpManager.Repository.Login;
using IpManager.Repository.Store;

namespace IpManager.Services.Store
{
    public class StoreService : IStoreService
    {
        private readonly ILoggerService LoggerService;
        private readonly IStoreRepository StoreRepository;

        public StoreService(ILoggerService _loggerservice,
            IStoreRepository _storerepository)
        {
            this.LoggerService = _loggerservice;
            this.StoreRepository = _storerepository;
        }

        public async Task<ResponseUnit<bool>> AddPCRoomService(StoreDTO dto)
        {
            try
            {
                if (dto is null)
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // BadRequest

                var PCRoomModel = new PcroomTb
                {
                    Ip = dto.Ip,
                    Port = dto.Port,
                    Name = dto.Name,
                    Addr = dto.Addr,
                    Seatnumber = dto.Seatnumber,
                    Price = dto.Price,
                    PricePercent = dto.Pricepercent,
                    PcSpec = dto.Pcspec,
                    Telecom = dto.Telecom,
                    Memo = dto.Memo,
                    CreateDt = DateTime.Now
                };

                var CountryModel = new CountryTb
                {
                    Name = dto.CountryName!,
                    CreateDt = DateTime.Now
                };

                var CityModel = new CityTb
                {
                    Name = dto.CityName!,
                    CreateDt = DateTime.Now
                };

                var TownModel = new TownTb
                {
                    Name = dto.TownName!,
                    CreateDt = DateTime.Now
                };

                int result = await StoreRepository.AddPCRoomAsync(PCRoomModel, CountryModel, CityModel, TownModel).ConfigureAwait(false);
                if(result > 0) 
                {
                    // 저장성공
                    return new ResponseUnit<bool>() { message = "저장되었습니다.", data = true, code = 200 };
                }
                else if (result == 0)
                {
                    return new ResponseUnit<bool>() { message = "이미 등록된 데이터입니다.", data = false, code = 200 };
                }
                else if(result == 99)
                {
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
                }
                else
                {
                    return new ResponseUnit<bool>() { message = "이미 등록된 데이터입니다.", data = false, code = 200 };
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

  
        /// <summary>
        /// 검색조건에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPCRoomListService(string? search, int pageIndex, int pagenumber)
        {
            try
            {
                var model = await StoreRepository.GetPcRoomListAsync(search, pageIndex, pagenumber).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// PC방 이름으로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomSearchNameListService(string? search)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(search))
                    return new ResponseList<StoreListDTO>() { message = "검색조건이 올바르지 않습니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomSearchNameListAsync(search.Trim()).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// PC방 주소로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseList<StoreListDTO>?> GetPcROomSearchAddressListService(string? search)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(search))
                    return new ResponseList<StoreListDTO>() { message = "검색조건이 올바르지 않습니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomSearchAddressListAsync(search.Trim()).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }


        /// <summary>
        /// PC방 정보 상세조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<StoreDetailDTO>?> GetPCRoomDetailService(int pid)
        {
            try
            {
                if (pid == 0)
                    return new ResponseUnit<StoreDetailDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomInfoDTO(pid).ConfigureAwait(false);
                if (model is null)
                    return new ResponseUnit<StoreDetailDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<StoreDetailDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<StoreDetailDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

     

        /// <summary>
        /// PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseList<StoreRegionDTO>?> GetPcRoomRegionListService()
        {
            try
            {
                var RegionList = await StoreRepository.GetPcRoomRegionCountAsync();
                if (RegionList is null)
                    return new ResponseList<StoreRegionDTO>() { message = "데이터가 존재하지 않습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreRegionDTO>() { message = "조회가 성공하였습니다.", data = RegionList, code = 200 };

            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreRegionDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// (도/시)별 PC방 리스트 반환
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomCountryListService(int countryid)
        {
            try
            {
                if (countryid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomCountryListAsync(countryid).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// (시/군/구)별 PC방 리스트 반환
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomCityListService(int cityid)
        {
            try
            {
                if (cityid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomCityListAsync(cityid).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// (읍/면/동)별 PC방 리스트 반환
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomTownListService(int townid)
        {
            try
            {
                if (townid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                var model = await StoreRepository.GetPcRoomTownListAsync(townid).ConfigureAwait(false);
                if (model is null)
                    return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<StoreListDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<StoreListDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// PC방 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> UpdateStoreService(UpdateStoreDTO dto)
        {
            try
            {
                if (dto is null)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if (dto.CountryId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if (dto.CityId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if (dto.TownId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                var PcroomTB = await StoreRepository.GetPcRoomInfoTB(dto.PID).ConfigureAwait(false);
                if (PcroomTB is null)
                    return new ResponseUnit<bool>() { message = "존재하지 않는 PC방 정보입니다.", data = false, code = 200 };

                
                PcroomTB.Ip = dto.Ip; // IP
                PcroomTB.Port = dto.Port; // PORT
                PcroomTB.Name = dto.Name; // 상호명
                PcroomTB.Addr = dto.Addr; // 주소
                PcroomTB.Seatnumber = dto.Seatnumber; // 좌석수
                PcroomTB.Price = dto.Price; // 요금제
                PcroomTB.PricePercent = dto.Pricepercent; // 요금제 비율
                PcroomTB.PcSpec = dto.Pcspec; // PC 사양
                PcroomTB.Telecom = dto.Telecom; // 통신사
                PcroomTB.Memo = dto.Memo; // 메모
                PcroomTB.UpdateDt = DateTime.Now;
                PcroomTB.CountrytbId = dto.CountryId; // (도/시) ID
                PcroomTB.CitytbId = dto.CityId; // (시/군/구) ID 
                PcroomTB.TowntbId = dto.TownId; // (읍/면/동) ID

                int result = await StoreRepository.EditPcRoomInfo(PcroomTB);
                if (result != -1)
                    return new ResponseUnit<bool>() { message = "수정이 완료되었습니다.", data = true, code = 200 };
                else
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

        /// <summary>
        /// PC방 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> DeleteStoreService(int pid)
        {
            try
            {
                if (pid == 0)
                    return new ResponseUnit<bool>() { message = "필수값이 누락되었습니다.", data = false, code = 200 };

                var PcroomTB = await StoreRepository.GetPcRoomInfoTB(pid);
                if (PcroomTB is null)
                    return new ResponseUnit<bool>() { message = "해당 아이디가 존재하지 않습니다.", data = false, code = 200 };

                PcroomTB.UpdateDt = DateTime.Now;
                PcroomTB.DelYn = true;
                PcroomTB.DeleteDt = DateTime.Now;

                int result = await StoreRepository.DeletePcRoomInfo(PcroomTB);
                if (result != -1)
                    return new ResponseUnit<bool>() { message = "수정이 완료되었습니다.", data = true, code = 200 };
                else
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

   
    }
}
