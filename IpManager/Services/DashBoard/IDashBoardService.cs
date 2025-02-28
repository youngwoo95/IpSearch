using IpManager.DTO.DashBoard;

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
        /// 하루 데이터 조회
        /// </summary>
        /// <returns></returns>
        public Task<ResponseUnit<AnalysisDataDTO>?> GetTodayDataService(DateTime TargetDate);

        /// <summary>
        /// 주간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<ResponseUnit<AnalysisDataDTO>?> GetWeeksDataService(DateTime TargetDate);

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<ResponseUnit<AnalysisDataDTO>?> GetMonthDataService(DateTime TargetDate);

        /// <summary>
        /// 년간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public Task<ResponseUnit<AnalysisDataDTO>?> GetYearDataService(DateTime TargetDate);

    }
}
