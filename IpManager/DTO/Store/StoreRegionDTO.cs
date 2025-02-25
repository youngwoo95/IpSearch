namespace IpManager.DTO.Store
{
    public class StoreRegionDTO
    {
        /// <summary>
        /// (도/시) PID
        /// </summary>
        public int Country_PID { get; set; }

        /// <summary>
        /// (도/시) 명칭
        /// </summary>
        public string? Country_Name { get; set; }

        /// <summary>
        /// (시/군/구) PID
        /// </summary>
        public int City_PID { get; set; }

        /// <summary>
        /// (시/군/구) 명칭
        /// </summary>
        public string? City_Name { get; set; }

        /// <summary>
        /// (읍/면/동) PID
        /// </summary>
        public int Town_PID { get; set; }

        /// <summary>
        /// (읍/면/동) 명칭
        /// </summary>
        public string? Town_Name { get; set; }

        /// <summary>
        /// (도/시) + (시/군/구) + (읍/면/동) 명칭
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// 해당 지역의 등록된 PC방 개수
        /// </summary>
        public int Count { get; set; }

    }
}
