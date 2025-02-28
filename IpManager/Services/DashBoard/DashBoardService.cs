using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DTO.DashBoard;
using IpManager.Repository.DashBoard;

namespace IpManager.Services.DashBoard
{
    public class DashBoardService : IDashBoardService
    {
        private ILoggerService LoggerService;
        private IDashBoardRepository DashBoardRepository;

        public DashBoardService(ILoggerService _loggerservice,
            IDashBoardRepository _dashboardrepository)
        {
            this.LoggerService = _loggerservice;
            this.DashBoardRepository = _dashboardrepository;

        }

        /// <summary>
        /// 실시간 데이터 조회
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseUnit<AnalysisDataDTO>?> GetThisTimeDataService()
        {
            try
            {
                DateTime NowDate = DateTime.Now;
                var result = await DashBoardRepository.GetThisTimeDataAnalysis(NowDate).ConfigureAwait(false);
                if (result is null)
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<AnalysisDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// 하루 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<AnalysisDataDTO>?> GetTodayDataService(DateTime TargetDate)
        {
            try
            {
                var result = await DashBoardRepository.GetTodayDataAnalysis(TargetDate).ConfigureAwait(false);
                if (result is null)
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<AnalysisDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// 주간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<AnalysisDataDTO>?> GetWeeksDataService(DateTime TargetDate)
        {
            try
            {
                // 해당 주의 월요일 계산
                int diff = (7 + (TargetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime StartDate = TargetDate.Date.AddDays(-diff); // 월요일 00시 00분 00초


                DateTime EndDate = TargetDate.Date.Add(new TimeSpan(23, 59, 59)); // 오늘꺼 제외시키기 위함.

                var result = await DashBoardRepository.GetWeeksDataAnalysis(StartDate, EndDate).ConfigureAwait(false);
                if (result is null)
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<AnalysisDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<AnalysisDataDTO>?> GetMonthDataService(DateTime TargetDate)
        {
            try
            {
                // 해당 월의 첫 번째 날 (00:00:00)
                DateTime StartDate = new DateTime(TargetDate.Year, TargetDate.Month, 1);

                // 해당 월의 마지막 날 계산 (마지막 날의 23시 59분 59초)
                int lastDay = DateTime.DaysInMonth(TargetDate.Year, TargetDate.Month);
                DateTime EndDate = new DateTime(TargetDate.Year, TargetDate.Month, lastDay, 23, 59, 59);


                var result = await DashBoardRepository.GetMonthDataAnalysis(StartDate, EndDate).ConfigureAwait(false);

                if (result is null)
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<AnalysisDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        
        /// <summary>
        /// 년간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public async Task<ResponseUnit<AnalysisDataDTO>?> GetYearDataService(DateTime TargetDate)
        {
            try
            {
                // 해당 년도의 1월 1일 00시:00분:00초 생성
                DateTime StartDate = new DateTime(TargetDate.Year, 1, 1, 0, 0, 0);

                // 해당 년도의 12월 마지막 날을 구한 후, 그 날의 23시 59분 59초 생성
                int lastDayOfDecember = DateTime.DaysInMonth(TargetDate.Year, 12);
                DateTime EndDate = new DateTime(TargetDate.Year, 12, lastDayOfDecember, 23, 59, 59);


                var result = await DashBoardRepository.GetYearDataAnalysis(StartDate, EndDate).ConfigureAwait(false);

                if (result is null)
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<AnalysisDataDTO>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<AnalysisDataDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }
    }
}
