namespace IpManager.DTO.Store
{
    public class StoreListDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int Pid { get; set; }
        
        /// <summary>
        /// 아이피
        /// </summary>
        public string Ip { get; set; } = null!;

        /// <summary>
        /// 포트
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 주소
        /// </summary>
        public string Addr { get; set; } = null!;


        /// <summary>
        ///  좌석수
        /// </summary>
        public int SeatNumber { get; set; }

        /// <summary>
        /// 요금제 가격
        /// </summary>
        public float Price { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public string? PricePercent { get; set; }

        /// <summary>
        /// 사양
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
        /// 도시
        /// </summary>
        public string? Region { get; set; }
        
        /// <summary>
        /// 대 ID
        /// </summary>
        public int CountryTbId { get; set; }

        /// <summary>
        /// 중 ID
        /// </summary>
        public int CityTbId { get; set; }

        /// <summary>
        /// 소 ID
        /// </summary>
        public int TownTbId { get; set; }
    }
}
