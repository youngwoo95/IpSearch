﻿using IpManager.DTO.Store;

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
        /// PC방 정보 상세조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public Task<ResponseUnit<StoreDetailDTO>?> GetPCRoomDetailService(int pid);

    }
}
