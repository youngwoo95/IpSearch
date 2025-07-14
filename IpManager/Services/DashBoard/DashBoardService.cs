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
        /// 매출 1위상권 & 매출 1위매장 & 가동률1위 매장 조회
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseUnit<TopSalesNameDTO>?> GetTopSalesNameService()
        {
            try
            {
                var model = await DashBoardRepository.GetTopSalesNameInfo();
                if (model is null)
                    return new ResponseUnit<TopSalesNameDTO>() { message = "요청이 정상 처리되었습니다.", data = null, code = 200 };
                else
                    return new ResponseUnit<TopSalesNameDTO>() { message = "요청이 정상 처리되었습니다.", data = model, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseUnit<TopSalesNameDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        public async Task<ResponseList<PcroomTimeDataDto>> GetThisDayDataService(DateTime targetDate,string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                var model = await DashBoardRepository.GetThisDayDataList(targetDate, pcName, countrytbid, towntbid, citytbid);
                return new ResponseList<PcroomTimeDataDto>() { message = "요청이 정상 처리되었습니다.", data = model, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<PcroomTimeDataDto>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        public async Task<ResponseList<ReturnValue>> GetPeriodDataService(DateTime startDate, DateTime endDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                var model = await DashBoardRepository.GetPeriodDataList(startDate,endDate,pcName, countrytbid,towntbid, citytbid);
                return new ResponseList<ReturnValue>() { message = "요청이 정상처리되었습니다.", data = model, code = 200};
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<ReturnValue>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }


        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        public async Task<ResponseList<ReturnValue>?> GetMonthDataService(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                var result = await DashBoardRepository.GetMonthDataAnalysis(TargetDate, pcName, countrytbid, towntbid, citytbid).ConfigureAwait(false);

                if (result is null)
                    return new ResponseList<ReturnValue>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<ReturnValue>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<ReturnValue>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        public async Task<ResponseList<ReturnValue>?> GetDaysDataService(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                var result = await DashBoardRepository.GetDaysDataAnalysis(TargetDate, pcName, countrytbid, towntbid, citytbid).ConfigureAwait(false);

                if (result is null)
                    return new ResponseList<ReturnValue>() { message = "조회결과가 없습니다.", data = null, code = 200 };
                else
                    return new ResponseList<ReturnValue>() { message = "조회가 성공하였습니다.", data = result, code = 200 };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<ReturnValue>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }

        public async Task<ResponseList<XlsxDTO>?> GetXslxDataService(DateTime startDate, DateTime endDate,List<int> pcId, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                var result = await DashBoardRepository.GetXlsxDataList(startDate, endDate, pcId, pcName, countrytbid, towntbid, citytbid).ConfigureAwait(false);

                return new ResponseList<XlsxDTO>() 
                { 
                    message = result is null ? "조회결과가 없습니다." : "조회가 성공하였습니다.", 
                    data = result, 
                    code = result is null ? 200 : 200 
                };
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return new ResponseList<XlsxDTO>() { message = "서버에서 요청을 처리하지 못하였습니다.", data = null, code = 500 };
            }
        }
    }
}
