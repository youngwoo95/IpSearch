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


        public Task<ResponseUnit<AnalysisDataDTO>?> GetMonthDataService(DateTime TargetDate)
        {
            throw new NotImplementedException();
        }

        

      

        public Task<ResponseUnit<AnalysisDataDTO>?> GetWeeksDataService(DateTime TargetDate)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseUnit<AnalysisDataDTO>?> GetYearDataService(DateTime TargetDate)
        {
            throw new NotImplementedException();
        }
    }
}
