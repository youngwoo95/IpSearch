using IpManager.DTO.Store;

namespace IpManager.Services.Store
{
    public interface IStoreService
    {
        /// <summary>
        /// PC방 정보 등록
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> AddPCRoomService(StoreDTO dto);

        /// <summary>
        /// 검색조건에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPCRoomListService(string? search, int pageIndex, int pagenumber);

        /// <summary>
        /// PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<StoreRegionDTO>?> GetPcRoomRegionListService();

        /// <summary>
        /// (도/시)별 PC방 리스트 반환
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomCountryListService(int countryid);

        /// <summary>
        /// (시/군/구)별 PC방 리스트 반환
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomCityListService(int cityid);

        /// <summary>
        /// (읍/면/동)별 PC방 리스트 반환
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomTownListService(int townid);
            

        /// <summary>
        /// PC방 정보 상세조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<StoreDetailDTO>?> GetPCRoomDetailService(int pid);

        /// <summary>
        /// PC방 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> UpdateStoreService(UpdateStoreDTO dto);

    }
}
