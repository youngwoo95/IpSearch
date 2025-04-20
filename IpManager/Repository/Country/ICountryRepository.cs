using IpManager.DBModel;

namespace IpManager.Repository.Country
{
    public interface ICountryRepository
    {
        /// <summary>
        /// (도/시) 정보 리스트 반환
        /// </summary>
        /// <returns></returns>
        public Task<List<CountryTb>?> GetCountryListAsync();

        /// <summary>
        /// 지역명에 해당하는 CountryTB 반환
        /// </summary>
        /// <returns></returns>
        public Task<CountryTb?> GetCountryInfoAsync(string countryName);

        /// <summary>
        /// 1레이어 도시명 추가
        /// </summary>
        /// <returns></returns>
        public Task<CountryTb?> AddCountryInfoAsync(string countryName);

        /// <summary>
        /// 도시정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<int> DeleteCountListAsync(List<int> countrypid);
    }
}
