namespace IpManager.DTO.Store
{
    public class StoreListDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        public int pId { get; set; }
        
        /// <summary>
        /// 아이피
        /// </summary>
        public string ip { get; set; } = null!;

        /// <summary>
        /// 포트
        /// </summary>
        public int port { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        public string name { get; set; } = null!;

        /// <summary>
        /// 주소
        /// </summary>
        public string addr { get; set; } = null!;


        /// <summary>
        ///  좌석수
        /// </summary>
        public int seatNumber { get; set; }

        /// <summary>
        /// 요금제 가격
        /// </summary>
        public float price { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        public float pricePercent { get; set; }

        /// <summary>
        /// 사양
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
        /// 도시
        /// </summary>
        public string? region { get; set; }
        
        /// <summary>
        /// 대 ID
        /// </summary>
        public int countryTbId { get; set; }

        /// <summary>
        /// 중 ID
        /// </summary>
        public int cityTbId { get; set; }

        /// <summary>
        /// 소 ID
        /// </summary>
        public int townTbId { get; set; }
    }
}
