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
