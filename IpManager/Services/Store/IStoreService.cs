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
        public Task<ResponseList<StoreListDTO>?> GetPCRoomListService(int userpid, int usertype, string? search);

        /// <summary>
        /// 검색조건(NAME)에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomSearchNameListService(int userpid, int userType, string? search);

        /// <summary>
        /// 검색조건(주소에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomSearchAddressListService(int userpid, int userType, string? search);

        /// <summary>
        /// PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<StoreRegionDTO>?> GetPcRoomRegionListService(int userpid, int userType);

        /// <summary>
        /// (도/시)별 PC방 리스트 반환
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomCountryListService(int countryid, int userpid, int userType);

        /// <summary>
        /// (시/군/구)별 PC방 리스트 반환
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomCityListService(int cityid, int userpid, int userType);

        /// <summary>
        /// (읍/면/동)별 PC방 리스트 반환
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        public Task<ResponseList<StoreListDTO>?> GetPcRoomTownListService(int townid, int userpid, int userType);


        /// <summary>
        /// PC방 정보 상세조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<StoreDetailDTO>?> GetPCRoomDetailService(int pid, int userpid, int userType);

        /// <summary>
        /// PC방 정보 수정
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> UpdateStoreService(UpdateStoreDTO dto);

        /// <summary>
        /// PC방 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<bool>> DeleteStoreService(int pid);

        /// <summary>
        /// 현재 PC방 사용중 PC / 꺼진 PC 반환
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<StorePingDTO>> GetUsedPcCountService(int pid);

    }
}
