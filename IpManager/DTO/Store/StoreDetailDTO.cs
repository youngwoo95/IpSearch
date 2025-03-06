namespace IpManager.DTO.Store
{
    public class StoreDetailDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int pId { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string ip { get; set; } = null!;

        /// <summary>
        /// 포트
        /// </summary>
        public int port { get; set; }

        /// <summary>
        /// 상호명
        /// </summary>
        public string name { get; set; } = null!;

        /// <summary>
        /// 주소
        /// </summary>
        public string addr { get; set; } = null!;

        /// <summary>
        /// 좌석수
        /// </summary>
        public int seatNumber { get; set; }

        /// <summary>
        /// 요금제
        /// </summary>
        public float price { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public string? pricePercent { get; set; }

        /// <summary>
        /// PC 사양
        /// </summary>
        public string? pcSpec { get; set; }

        /// <summary>
        /// 통신사
        /// </summary>
        public string? telecom { get; set; }

        /// <summary>
        /// 메모
        /// </summary>
        public string? memo { get; set; }

        /// <summary>
        /// 저장된 (도/시) ID
        /// </summary>
        public int countryTbId { get; set; }

        /// <summary>
        /// (도/시) 명
        /// </summary>
        public string? countryName { get; set; }

        /// <summary>
        /// 저장된 (시/군/구) ID
        /// </summary>
        public int cityTbId { get; set; }

        /// <summary>
        /// (시/군/구) 명
        /// </summary>
        public string? cityName { get; set; }

        /// <summary>
        /// 저장된 (읍/면/동) ID
        /// </summary>
        public int townTbId { get; set; }

        /// <summary>
        /// (읍/면/동) 명
        /// </summary>
        public string? townName { get; set; }


    }
}
