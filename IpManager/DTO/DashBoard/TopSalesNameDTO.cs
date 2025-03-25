namespace IpManager.DTO.DashBoard
{
    /// <summary>
    /// 매출 1위 상권명 DTO
    /// </summary>
    public class TopSalesNameDTO
    {
        /// <summary>
        /// 매출 1위 상권 명
        /// </summary>
        public string? topSalesTownName { get; set; }
        
        /// <summary>
        /// 매출 1위 매장 명
        /// </summary>
        public string? topSalesStoreName { get; set; }

        /// <summary>
        /// 가동률 1위 매장명
        /// </summary>
        public string? topUsedRateStoreName { get; set; }
    }
}
