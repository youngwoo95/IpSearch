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
        public Task<AnalysisDataDTO?> GetWeeksDataAnalysis(DateTime TargetDate);

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetMonthDataAnalysis(DateTime TargetDate);

        /// <summary>
        /// 년간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<AnalysisDataDTO?> GetYearDataAnalysis(DateTime TargetDate);

    }
}
