using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Store;
using IpManager.Repository.Store;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;

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

                var model = await StoreRepository.GetPcRoomInfo(pid).ConfigureAwait(false);
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

                if(dto.CountryId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                if(dto.CityId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };
                
                if(dto.TownId is 0)
                    return new ResponseUnit<bool>() { message = "잘못된 요청입니다.", data = false, code = 200 };

                var StoreTB = await StoreRepository.GetPcRoomInfo(dto.PID).ConfigureAwait(false);
                if (StoreTB is null)
                    return new ResponseUnit<bool>() { message = "존재하지 않는 PC방 정보입니다.", data = false, code = 200 };

                // CountryTB 없으면 INSERT 있으면 SELECT 해야함
                StoreTB.CountryTbId = dto.CountryId; // (도/시) ID SELECT 먼저
                
                // CityTB 없으면 INSERT 있으면 SELECT 해야함

                // TownTB 없으면 INSERT 있으면 SELECT 해야함.

                StoreTB.Ip = dto.Ip; // IP
                StoreTB.Port = dto.Port; // PORT
                StoreTB.Name = dto.Name; // 상호명
                StoreTB.Addr = dto.Addr; // 주소
                StoreTB.Seatnumber = dto.Seatnumber; // 좌석수
                StoreTB.Price = dto.Price; // 요금제
                StoreTB.Pricepercent = dto.Pricepercent; // 요금제 비율
                StoreTB.Pcspec = dto.Pcspec; // PC 사양
                StoreTB.Telecom = dto.Telecom; // 통신사
                StoreTB.Memo = dto.Memo; // 메모


                return new ResponseUnit<bool>() { message = "요청이 정상 처리되었습니다.", data = true, code = 200 };



            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

    }
}
