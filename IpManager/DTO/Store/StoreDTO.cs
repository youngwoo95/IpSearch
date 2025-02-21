namespace IpManager.DTO.Store
{
    public class StoreDTO
    {
        /// <summary>
        /// IP
        /// </summary>
        public string Ip { get; set; } = null!;

        /// <summary>
        /// 포트
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 상호명
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 주소
        /// </summary>
        public string Addr { get; set; } = null!;

        /// <summary>
        /// 좌석수
        /// </summary>
        public int Seatnumber { get; set; }

        /// <summary>
        /// 요금제
        /// </summary>
        public float price { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public string? Pricepercent { get; set; }

        /// <summary>
        /// PC 사양
        /// </summary>
        public string? Pcspec { get; set; }

        /// <summary>
        /// 통신사
        /// </summary>
        public string? Telecom { get; set; }

        /// <summary>
        /// 메모
        /// </summary>
        public string? Memo { get; set; }

        /// <summary>
        /// (도/시) 명
        /// </summary>
        public string? CountryName { get; set; }

        /// <summary>
        /// (시/군/구) 명
        /// </summary>
        public string? CityName { get; set; }

        /// <summary>
        /// (읍/면/동) 명
        /// </summary>
        public string? TownName { get; set; }


    }
}
