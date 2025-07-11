namespace IpManager.DTO.DashBoard
{
    public class PeriodList
    {
        /// <summary>
        /// 분석시간
        /// </summary>
        public string? AnalyzeDT { get; set; }

        /// <summary>
        /// 분석데이터
        /// </summary>
        public List<PeriodAnayzeList> AnalyzeList { get; set; } = new List<PeriodAnayzeList>();
    }

    public class PeriodAnayzeList
    {
        /// <summary>
        /// 도시 시퀀스
        /// </summary>
        public int countryId { get; set; }

        /// <summary>
        /// 도시 명
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

        /// <summary>
        /// PC방 명칭
        /// </summary>
        public string? pcName { get; set; }

        /// <summary>
        /// 가동 PC수
        /// </summary>
        public double? usedPc { get; set; }

        /// <summary>
        /// 평균 가동률
        /// </summary>
        public double? averageRate { get; set; }

        /// <summary>
        /// PC 이용 매출
        /// </summary>
        public double? pcPrice { get; set; }

        /// <summary>
        /// 식품 기타 매출
        /// </summary>
        public double? foodPrice { get; set; }

        /// <summary>
        /// 총 매출
        /// </summary>
        public double? totalPrice { get; set; }

        /// <summary>
        /// 좌석수
        /// </summary>
        public int seatNumber { get; set; } 

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public string? pricePercent { get; set; }
    }

    public class ReturnValue
    {
        /// <summary>
        /// 도시 시퀀스
        /// </summary>
        public int countryId { get; set; }

        /// <summary>
        /// 도시 명
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

        /// <summary>
        /// PC방 명칭
        /// </summary>
        public string? pcName { get; set; }

        /// <summary>
        /// 가동 PC수
        /// </summary>
        public string? usedPc { get; set; }

        /// <summary>
        /// 평균 가동률
        /// </summary>
        public string? averageRate { get; set; }

        /// <summary>
        /// PC 이용 매출
        /// </summary>
        public string? pcPrice { get; set; }

        /// <summary>
        /// 식품 기타 매출
        /// </summary>
        public string? foodPrice { get; set; }

        /// <summary>
        /// 총 매출
        /// </summary>
        public string? totalPrice { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public string? pricePercent { get; set; }
    }
   
}
