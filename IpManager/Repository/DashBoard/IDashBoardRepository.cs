using IpManager.DTO.DashBoard;
using static IpManager.Repository.DashBoard.DashBoardRepository;

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
        /// 하루 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetTodayDataAnalysis(DateTime TargetDate);

        /// <summary>
        /// 주간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetWeeksDataAnalysis(DateTime StartDate, DateTime EndDate);

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetMonthDataAnalysis(DateTime StartDate, DateTime EndDate);

        /// <summary>
        /// 년간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetYearDataAnalysis(DateTime StartDate, DateTime EndDate);

        /// <summary>
        /// 매출 1위상권 & 매출 1위매장 & 가동률1위 매장 조회
        /// </summary>
        /// <returns></returns>
        public Task<TopSalesNameDTO?> GetTopSalesNameInfo();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<List<PcroomTimeDataDto>> GetThisDayDataList();
    }
}
