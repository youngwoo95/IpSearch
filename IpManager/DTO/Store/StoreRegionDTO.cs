namespace IpManager.DTO.Store
{
    public class StoreRegionDTO
    {
        /// <summary>
        /// (도/시) PID
        /// </summary>
        public int countryPid { get; set; }

        /// <summary>
        /// (도/시) 명칭
        /// </summary>
        public string? countryName { get; set; }

        /// <summary>
        /// (시/군/구) PID
        /// </summary>
        public int cityPid { get; set; }

        /// <summary>
        /// (시/군/구) 명칭
        /// </summary>
        public string? cityName { get; set; }

        /// <summary>
        /// (읍/면/동) PID
        /// </summary>
        public int townPid { get; set; }

        /// <summary>
        /// (읍/면/동) 명칭
        /// </summary>
        public string? townName { get; set; }

        /// <summary>
        /// (도/시) + (시/군/구) + (읍/면/동) 명칭
        /// </summary>
        public string? region { get; set; }

        /// <summary>
        /// 해당 지역의 등록된 PC방 개수
        /// </summary>
        public int count { get; set; }

    }
}
