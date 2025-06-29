using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.Country;
using IpManager.Repository.Country;
using System.Diagnostics;

namespace IpManager.Services.Country
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository CountryRepository;
        private readonly ILoggerService LoggerService;

        public CountryService(ICountryRepository _countryrepository,
            ILoggerService _loggerservice)
        {
            this.CountryRepository = _countryrepository;
            this.LoggerService = _loggerservice;
        }

        /// <summary>
        /// 전체 (도/시) 리스트 반환
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseList<CountryDataDTO>> GetCountryListService()
        {
            try
            {
                var CountryTB = await CountryRepository.GetCountryListAsync();
                if (CountryTB is null)
                    return new ResponseList<CountryDataDTO>() { message = "조회가 성공하였습니다.", data = new List<CountryDataDTO>(), code = 200 };

                var model = CountryTB.Select(m => new CountryDataDTO()
                {
                    pId = m.Pid,
                    countryName = m.Name
                }).ToList();

                return new ResponseList<CountryDataDTO>() { message = "조회가 성공하였습니다.", data = model, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<CountryDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        public async Task<ResponseList<RegionDataDTO>?> GetRegionListService()
        {
            try
            {
                var model = await CountryRepository.GetRegionListAsync().ConfigureAwait(false);
                if(model is null)
                    return new ResponseList<RegionDataDTO>
                    {
                        code = 200,
                        message = "요청이 정상 처리되었습니다",
                        data = null
                    };


                // 2) 엔티티를 DTO로 매핑
                var dtoList = model.Select(c => new RegionDataDTO
                {
                    countryId = c.Pid,      // CountryTb.Pid
                    countryName = c.Name,     // CountryTb.Name

                    cityDatas = c.CityTbs     // City 컬렉션
                        .Select(city => new cityDataDTO
                        {
                            cityId = city.Pid,
                            cityName = city.Name,

                            townDatas = city.TownTbs  // Town 컬렉션
                                .Select(town => new townDataDTO
                                {
                                    townId = town.Pid,
                                    townName = town.Name
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

                return new ResponseList<RegionDataDTO>
                {
                    code = 200,
                    message = "요청이 정상 처리되었습니다",
                    data = dtoList
                };

            }
            catch(Exception ex)
            {
                LoggerService.FileLogMessage(ex.ToString());
                return new ResponseList<RegionDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// 도시정보 삭제
        /// </summary>
        /// <param name="countrypid"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<bool>> DeleteCountryListService(List<int> countrypid)
        {
            try
            {
                var result = await CountryRepository.DeleteCountListAsync(countrypid);
                if(result == -1)
                {
                    return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
                }
                else if(result == 0)
                {
                    return new ResponseUnit<bool>() { message = "해당 주소가 할당된 데이터가 있습니다 데이터를 먼저 비우고 진행해주세요.", data = false, code = 200 };
                }
                else
                {
                    return new ResponseUnit<bool>() { message = "요청이 정상 되었습니다..", data = false, code = 200 };
                }
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<bool>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = false, code = 500 };
            }
        }

      
    }
}
