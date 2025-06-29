using IpManager.DTO.Country;

namespace IpManager.Services.Country
{
    public interface ICountryService
    {
        /// <summary>
        /// 전체 (도/시) 리스트 반환
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<CountryDataDTO>> GetCountryListService();

        /// <summary>
        /// 도시정보 삭제
        /// </summary>
        /// <param name="countrypid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> DeleteCountryListService(List<int> countrypid);

        public Task<ResponseList<RegionDataDTO>?> GetRegionListService();

    }
}
