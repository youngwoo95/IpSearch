namespace IpManager.DTO.DashBoard
{
    public class AnalysisDataDTO
    {
        /// <summary>
        /// 가3동률이 가장 높은 매장
        /// </summary>
        public string? BestName { get; set; }
        
        /// <summary>
        /// 마지막 분석 시간
        /// </summary>
        public DateTime? AnalysisDate { get; set; }

        /// <summary>
        /// 분석 결과
        /// </summary>
        public List<ResultData> Datas { get; set; } = new List<ResultData>();
    }

    public class ResultData
    {
        /// <summary>
        /// 매장 이름
        /// </summary>
        public string? PcRoomName { get; set; }

        /// <summary>
        /// 가동률
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// PC 대수
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 가동률
        /// </summary>
        public float? Rate { get; set; }

        /// <summary>
        /// 가동률 - 반환해줄것
        /// </summary>
        public string? ReturnRate { get; set; }
    }
}
