namespace IpManager.DTO.DashBoard
{
    public class PcroomTimeDataDto
    {
        public int pcRoomId { get; set; }

        /// <summary>
        /// 도시 시퀀스
        /// </summary>
        public int countryId { get; set; }

        /// <summary>
        /// 도시 이름
        /// </summary>
        public string? countryName { get; set; }

        /// <summary>
        /// 시군구 시퀀스
        /// </summary>
        public int cityId { get; set; }

        /// <summary>
        /// 시군구 명
        /// </summary>
        public string? cityName { get; set; }

        /// <summary>
        /// 읍면동 시퀀스
        /// </summary>
        public int townId { get; set; }

        /// <summary>
        /// 읍면동 명
        /// </summary>
        public string? townName { get; set; }


        public string pcRoomName { get; set; } = string.Empty;
        // Key: 시간(예: "00:00"), Value: UsedPc 값
        public List<ThisAnayzeList> analyList { get; set; } = new List<ThisAnayzeList>();
    }
}
