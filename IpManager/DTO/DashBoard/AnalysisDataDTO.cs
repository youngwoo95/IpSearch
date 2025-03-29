namespace IpManager.DTO.DashBoard
{
    public class AnalysisDataDTO
    {
        /// <summary>
        /// 가동률이 가장 높은 매장
        /// </summary>
        public string? bestName { get; set; }
        
        /// <summary>
        /// 마지막 분석 시간
        /// </summary>
        public DateTime? analysisDate { get; set; }

        /// <summary>
        /// 분석 결과
        /// </summary>
        public List<ResultData> datas { get; set; } = new List<ResultData>();
    }

    public class ResultData
    {
        /// <summary>
        /// 매장 이름
        /// </summary>
        public string? pcRoomName { get; set; }

        /// <summary>
        /// 가동률
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// PC 대수
        /// </summary>
        public int totalCount { get; set; }

        /// <summary>
        /// 가동률
        /// </summary>
        public float? rate { get; set; }

        /// <summary>
        /// 가동률 - 반환해줄것
        /// </summary>
        public string? returnRate { get; set; }
    }
}
