using IpManager.DTO.DashBoard;
using static IpManager.Repository.DashBoard.DashBoardRepository;

namespace IpManager.Services.DashBoard
{
    public interface IDashBoardService
    {
        /// <summary>
        /// 현황(실시간)조회
        /// </summary>
        /// <returns></returns>
        public Task<ResponseUnit<AnalysisDataDTO>?> GetThisTimeDataService();

        /// <summary>
        /// 매출 1위상권 & 매출 1위매장 & 가동률1위 매장 조회
        /// </summary>
        /// <returns></returns>
        public Task<ResponseUnit<TopSalesNameDTO>?> GetTopSalesNameService();

        /// <summary>
        /// 전체 분석기록
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<PcroomTimeDataDto>> GetThisDayDataService(DateTime targetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        /// <summary>
        /// 기간별 분석기록
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<ReturnValue>> GetPeriodDataService(DateTime startDate, DateTime endDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<ResponseList<ReturnValue>?> GetMonthDataService(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);

        /// <summary>
        /// 하루 데이터 조회
        /// </summary>
        /// <returns></returns>
        public Task<ResponseList<ReturnValue>?> GetDaysDataService(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid);
    }
}
