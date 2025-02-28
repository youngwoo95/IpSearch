﻿using IpManager.DBModel;
using IpManager.DTO.Store;

namespace IpManager.Repository.Store
{
    public interface IStoreRepository
    {
#region 추가
        /// <summary>
        /// PC방 정보 추가
        /// </summary>
        /// <param name="PcroomTB"></param>
        /// <param name="CountryTB"></param>
        /// <param name="CityTB"></param>
        /// <param name="TownTB"></param>
        /// <returns></returns>
        Task<int> AddPCRoomAsync(PcroomTb PcroomTB, CountryTb CountryTB, CityTb CityTB, TownTb TownTB);
#endregion

#region 조회
        /// <summary>
        /// PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomListAsync(string? search, int pageIndex, int pagenumber);

        /// <summary>
        /// PC방 이름에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        Task<List<StoreListDTO>?> GetPcRoomSearchNameListAsync(string search);

        /// <summary>
        /// PC방 주소에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>

        Task<List<StoreListDTO>?> GetPcRoomSearchAddressListAsync(string search);

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
        Task<StoreDetailDTO?> GetPcRoomInfoDTO(int pid);

        /// <summary>
        /// PID로 PC방 테이블 조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        Task<PcroomTb?> GetPcRoomInfoTB(int pid);
#endregion

#region 수정
        /// <summary>
        /// PC방 정보 수정
        /// </summary>
        /// <param name="PcroomTB"></param>
        /// <returns></returns>
        Task<int> EditPcRoomInfo(PcroomTb PcroomTB);
#endregion

#region 삭제
        /// <summary>
        /// PC방 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        Task<int> DeletePcRoomInfo(PcroomTb PcroomTB);
#endregion

    }
}
