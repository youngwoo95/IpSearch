using IpManager.DTO.DashBoard;

namespace IpManager.Repository.DashBoard
{
    public interface IDashBoardRepository
    {
        /// <summary>
        /// 현황(실시간)조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetThisTimeDataAnalysis(DateTime TargetDate);

  
   

        /// <summary>
        /// 매출 1위상권 & 매출 1위매장 & 가동률1위 매장 조회
        /// </summary>
        /// <returns></returns>
        public Task<TopSalesNameDTO?> GetTopSalesNameInfo();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<List<PcroomTimeDataDto>> GetThisDayDataList(DateTime targetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        public Task<List<ReturnValue>> GetPeriodDataList(DateTime startDate, DateTime endDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<List<ReturnValue>?> GetMonthDataAnalysis(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        /// <summary>
        /// 하루 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<List<ReturnValue>?> GetDaysDataAnalysis(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);


        public Task<List<XlsxDTO>?> GetXlsxDataList(DateTime startDate, DateTime endDate, List<int> pcId, string? pcName, int? countrytbId, int? towntbId, int? citytbid);

    }
}
