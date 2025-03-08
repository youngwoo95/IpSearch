using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Store;
using IpManager.Repository.Login;
using IpManager.Repository.Store;
using Microsoft.OpenApi.Validations;
using System.Net.NetworkInformation;

namespace IpManager.Services.Store
{
    public class StoreService : IStoreService
    {
        private readonly ILoggerService LoggerService;
        private readonly IStoreRepository StoreRepository;
        private readonly IUserRepository UserRepository;

        public StoreService(ILoggerService _loggerservice,
            IStoreRepository _storerepository,
            IUserRepository _userrepository)
        {
            this.LoggerService = _loggerservice;
            this.StoreRepository = _storerepository;
            this.UserRepository = _userrepository;
        }

        public async Task<ResponseUnit<bool>> AddPCRoomService(StoreDTO dto)
        {
            try
            {
                if (dto is null)
                    return new ResponseUnit<bool>() { message = "잘못된 입력값이 존재합니다.", data = false, code = 200 }; // BadRequest

                var PCRoomModel = new PcroomTb
                {
                    Ip = dto.ip,
                    Port = dto.port,
                    Name = dto.name,
                    Addr = dto.addr,
                    Seatnumber = dto.seatNumber,
                    Price = dto.price,
                    PricePercent = dto.pricePercent,
                    PcSpec = dto.pcSpec,
                    Telecom = dto.telecom,
                    Memo = dto.memo,
                    CreateDt = DateTime.Now
                };

                var CountryModel = new CountryTb
                {
                    Name = dto.countryName!,
                    CreateDt = DateTime.Now
                };

                var CityModel = new CityTb
                {
                    Name = dto.cityName!,
                    CreateDt = DateTime.Now
                };

                var TownModel = new TownTb
                {
                    Name = dto.townName!,
                    CreateDt = DateTime.Now
                };

                int pcroomCheck = await StoreRepository.GetPcRoomCheck(dto.name, dto.ip, dto.port, dto.addr);
                if(pcroomCheck != 0)
                    return new ResponseUnit<bool>() { message = "이미 등록된 데이터입니다.", data = false, code = 200 };

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
        public async Task<ResponseList<StoreListDTO>?> GetPCRoomListService(int userpid, int userType, string? search, int pageIndex, int pagenumber)
        {
            try
            {
                List<StoreListDTO>? model = default;
                if (userType == 0)
                {
                    // Visitor - 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid);
                    if (userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 404 };

                    int countryId = userModel.CountryId ?? -1;
                    if (countryId == -1)
                        return new ResponseList<StoreListDTO>() { message = "할당된 지역이 없습니다.", data = null, code = 200 };

                    model = await StoreRepository.GetMyPcRoomListAsync(search, pageIndex, pagenumber, countryId);
                }
                else
                {
                    // Manager - 전체
                    model = await StoreRepository.GetAllPcRoomListAsync(search, pageIndex, pagenumber).ConfigureAwait(false);
                }

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
        /// PC방 이름으로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomSearchNameListService(int userpid, int userType, string? search)
        {
            try
            {
                List<StoreListDTO>? model = default;

                if(userType == 0)
                {
                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid);
                    if (userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 404 };

                    int countryId = userModel.CountryId ?? -1;
                    if (countryId == -1)
                        return new ResponseList<StoreListDTO>() { message = "할당된 지역이 없습니다.", data = null, code = 200 };

                    model = await StoreRepository.GetMyPcRoomSearchNameListAsync(search.Trim(),countryId).ConfigureAwait(false);
                }
                else
                {
                    // Manager 전체
                    if (String.IsNullOrWhiteSpace(search))
                        return new ResponseList<StoreListDTO>() { message = "검색조건이 올바르지 않습니다.", data = null, code = 200 };

                    model = await StoreRepository.GetAllPcRoomSearchNameListAsync(search.Trim()).ConfigureAwait(false);
                }

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
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomSearchAddressListService(int userpid, int userType, string? search)
        {
            try
            {
                List<StoreListDTO>? model = default;

                if(userType == 0)
                {
                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid);
                    if (userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 404 };

                    int countryId = userModel.CountryId ?? -1;
                    if (countryId == -1)
                        return new ResponseList<StoreListDTO>() { message = "할당된 지역이 없습니다.", data = null, code = 200 };

                    if (String.IsNullOrWhiteSpace(search))
                        return new ResponseList<StoreListDTO>() { message = "검색조건이 올바르지 않습니다.", data = null, code = 200 };

                    model = await StoreRepository.GetPcRoomMySearchAddressLisyAsync(search.Trim(), countryId).ConfigureAwait(false);
                }
                else
                {
                    // Manager 전체
                    if (String.IsNullOrWhiteSpace(search))
                        return new ResponseList<StoreListDTO>() { message = "검색조건이 올바르지 않습니다.", data = null, code = 200 };

                    model = await StoreRepository.GetPcRoomAllSearchAddressListAsync(search.Trim()).ConfigureAwait(false);
                }
               
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
        public async Task<ResponseUnit<StoreDetailDTO>?> GetPCRoomDetailService(int pid, int userpid, int userType)
        {
            try
            {
                if (pid == 0)
                    return new ResponseUnit<StoreDetailDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                StoreDetailDTO? model = default;

                if(userType == 0)
                {
                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid);
                    if (userModel is null)
                        return new ResponseUnit<StoreDetailDTO>() { message = "잘못된 요청입니다.", data = null, code = 404 };

                    int countryId = userModel.CountryId ?? -1;

                    // 해당 countryId에 할당된 PC방이 있는지?
                    List<StoreListDTO>? PcroomList = await StoreRepository.GetPcRoomCountryListAsync(countryId).ConfigureAwait(false);
                    if (PcroomList is null || PcroomList.Count == 0)
                        return new ResponseUnit<StoreDetailDTO>() { message = "데이터가 존재하지 않습니다.", data = null, code = 200 };
                            
                    // 이게 진짜 데이터
                    model = await StoreRepository.GetPcRoomInfoDTO(pid).ConfigureAwait(false);
                    if(model is null)
                        return new ResponseUnit<StoreDetailDTO>() { message = "데이터가 존재하지 않습니다.", data = null, code = 200 };

                    // countryId가 조회된 PC방 정보의 countryId에 포함되어있는지?
                    var filter = PcroomList.FirstOrDefault(m => m.countryTbId == model.countryTbId);
                    if(filter is null) // 여기서 걸리면 잘못조회한거임
                        return new ResponseUnit<StoreDetailDTO>() { message = "데이터가 존재하지 않습니다.", data = null, code = 200 };
                }
                else
                {
                    // Manager 전체
                    model = await StoreRepository.GetPcRoomInfoDTO(pid).ConfigureAwait(false);
                }
                
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
        public async Task<ResponseList<StoreRegionDTO>?> GetPcRoomRegionListService(int userpid, int userType)
        {
            try
            {
                if (userpid == 0)
                    return new ResponseList<StoreRegionDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                List<StoreRegionDTO>? RegionList = default;

                if(userType == 0)
                {
                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid).ConfigureAwait(false);
                    if (userModel is null)
                        return new ResponseList<StoreRegionDTO>() { message = "잘못된 요청입니다.", data = null, code = 400 };

                    int countryId = userModel.CountryId ?? -1;
                    if (countryId == -1)
                        return new ResponseList<StoreRegionDTO>() { message = "잘못된 요청입니다.", data = null, code = 400 };

                    RegionList = await StoreRepository.GetPcRoomMyRegionCountAsync(countryId).ConfigureAwait(false);
                }
                else
                {
                    // Manager 전체
                    RegionList = await StoreRepository.GetPcRoomAllRegionCountAsync().ConfigureAwait(false);
                }

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
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomCountryListService(int countryid, int userpid, int userType)
        {
            try
            {
                if (countryid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                if(userpid == 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                List<StoreListDTO>? model = default;

                if(userType == 0)
                {
                    // 어차피 Visitor 같은 경우 countryId가 하나이기 때문에 조회 조건으로 온 countryId가 그 User의 countryId와 다르면 return 시키면됨.

                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid).ConfigureAwait(false);
                    if(userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                    if(userModel.CountryId != countryid)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };
                }
                
                // Manager 전체
                model = await StoreRepository.GetPcRoomCountryListAsync(countryid).ConfigureAwait(false);
                
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
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomCityListService(int cityid, int userpid, int userType)
        {
            try
            {
                if (cityid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                if (userpid == 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                List<StoreListDTO>? model = default;
                model = await StoreRepository.GetPcRoomCityListAsync(cityid).ConfigureAwait(false);

                if (userType == 0)
                {
                    // Visitor 자기 지역만
                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid).ConfigureAwait(false);
                    if(userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                    int countryId = userModel.CountryId ?? -1;
                    if(countryId == -1)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                    if (model is null)
                        return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };

                    model = model.Where(m => m.countryTbId == countryId).ToList();
                }

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
        public async Task<ResponseList<StoreListDTO>?> GetPcRoomTownListService(int townid, int userpid, int userType)
        {
            try
            {
                if (townid <= 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                if (userpid == 0)
                    return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                List<StoreListDTO>? model = default;
                model = await StoreRepository.GetPcRoomTownListAsync(townid).ConfigureAwait(false);

                if (userType == 0)
                {
                    // Visitor 자기 지역만 필터링

                    var userModel = await UserRepository.GetUserInfoAsyncById(userpid).ConfigureAwait(false);
                    if (userModel is null)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                    int countryId = userModel.CountryId ?? -1;
                    if (countryId == -1)
                        return new ResponseList<StoreListDTO>() { message = "잘못된 요청입니다.", data = null, code = 200 };

                    if(model is null)
                        return new ResponseList<StoreListDTO>() { message = "조회된 데이터가 없습니다.", data = null, code = 200 };

                    // 자기지역만 filtering
                    model = model.Where(m => m.countryTbId == countryId).ToList();
                }

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

                if (dto.countryId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if (dto.cityId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if (dto.townId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                var PcroomTB = await StoreRepository.GetPcRoomInfoTB(dto.pId).ConfigureAwait(false);
                if (PcroomTB is null)
                    return new ResponseUnit<bool>() { message = "존재하지 않는 PC방 정보입니다.", data = false, code = 200 };

                
                PcroomTB.Ip = dto.ip; // IP
                PcroomTB.Port = dto.port; // PORT
                PcroomTB.Name = dto.name; // 상호명
                PcroomTB.Addr = dto.addr; // 주소
                PcroomTB.Seatnumber = dto.seatNumber; // 좌석수
                PcroomTB.Price = dto.price; // 요금제
                PcroomTB.PricePercent = dto.pricePercent; // 요금제 비율
                PcroomTB.PcSpec = dto.pcSpec; // PC 사양
                PcroomTB.Telecom = dto.telecom; // 통신사
                PcroomTB.Memo = dto.memo; // 메모
                PcroomTB.UpdateDt = DateTime.Now;
                PcroomTB.CountrytbId = dto.countryId; // (도/시) ID
                PcroomTB.CitytbId = dto.cityId; // (시/군/구) ID 
                PcroomTB.TowntbId = dto.townId; // (읍/면/동) ID

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

                var PcroomTB = await StoreRepository.GetPcRoomInfoTB(pid).ConfigureAwait(false);
                if (PcroomTB is null)
                    return new ResponseUnit<bool>() { message = "해당 아이디가 존재하지 않습니다.", data = false, code = 200 };

                PcroomTB.Name = $"{PcroomTB.Pid}_{PcroomTB.Name}";
                PcroomTB.UpdateDt = DateTime.Now;
                PcroomTB.DelYn = true;
                PcroomTB.DeleteDt = DateTime.Now;

                int result = await StoreRepository.DeletePcRoomInfo(PcroomTB).ConfigureAwait(false);
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
        /// 현재 PC방 사용중 PC / 꺼진 PC 반환
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<StorePingDTO>> GetUsedPcCountService(int pid)
        {
            try
            {
                // 해당 IP를 얻기위해 조회
                StoreDetailDTO? dto = await StoreRepository.GetPcRoomInfoDTO(pid).ConfigureAwait(false);
                if (dto is null)
                    return new ResponseUnit<StorePingDTO>() { message = "잘못된 요청입니다.", data = null, code = 500 };

                string targetIp = default;

                int dotCount = 0;
                int thirdDotIndex = -1;
                for (int i = 0; i < dto.ip.Length; i++)
                {
                    if (dto.ip[i] == '.')
                    {
                        dotCount++;
                        if(dotCount == 3)
                        {
                            thirdDotIndex = i;
                            break;
                        }
                    }
                }
                targetIp = (dotCount >= 3 && thirdDotIndex > 0) ? dto.ip.Substring(0, thirdDotIndex) : dto.ip;
                Console.WriteLine($"타겟 접두어: {targetIp}");

                var pingTasks = Enumerable.Range(0, 256)
                    .Select(i =>
                    {
                        string ipAddress = $"{targetIp}.{i}";
                        return PingHostAsync(ipAddress);
                    }).ToList();

                // 병렬로 실행 후 결과 집계
                var results = await Task.WhenAll(pingTasks);

                var resultipCount = results.Where(r => !String.IsNullOrWhiteSpace(r)).Count();
                var model = new StorePingDTO
                {
                    used = resultipCount,
                    unUsed = dto.seatNumber - resultipCount
                };

                return new ResponseUnit<StorePingDTO>() { message = "요청이 정상 처리되었습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<StorePingDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// PING SEND
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string?> PingHostAsync(string ipAddress)
        {
            using (var ping = new Ping())
            {
                try
                {
                    // 타임아웃을 1000ms로 설정
                    PingReply reply = await ping.SendPingAsync(ipAddress, 1000);
                    if(reply != null && reply.Status == IPStatus.Success)
                    {
                        return ipAddress;
                    }
                    else
                    {
                        return null;
                    }    
                }
                catch(Exception ex)
                {
                    LoggerService.FileErrorMessage($"Ping error for {ipAddress}: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
