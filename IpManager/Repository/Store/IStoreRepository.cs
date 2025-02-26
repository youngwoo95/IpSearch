using IpManager.DBModel;
using IpManager.DTO.Store;

namespace IpManager.Repository.Store
{
    public interface IStoreRepository
    {
        /// <summary>
        /// PC방 정보 추가
        /// </summary>
        /// <param name="PcroomTB"></param>
        /// <param name="CountryTB"></param>
        /// <param name="CityTB"></param>
        /// <param name="TownTB"></param>
        /// <returns></returns>
        Task<int> AddPCRoomAsync(PcroomTb PcroomTB, CountryTb CountryTB, CityTb CityTB, TownTb TownTB);

        /// <summary>
        /// PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomListAsync(string? search, int pageIndex, int pagenumber);

        /// <summary>
        /// (도/시)별 PC방 List 반환
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomCountryListAsync(int countryid);

        /// <summary>
        /// (시/군/구)별 PC방 List 반환
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomCityListAsync(int cityid);

        /// <summary>
        /// (읍/면/동)별 PC방 List 반환
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomTownListAsync(int townid);

        /// <summary>
        /// PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        Task<List<StoreRegionDTO>?> GetPcRoomRegionCountAsync();


        /// <summary>
        /// PC방 정보 상세보기
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        Task<StoreDetailDTO?> GetPcRoomInfo(int pid);

    }
}
