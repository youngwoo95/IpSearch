using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.DashBoard;
using Microsoft.EntityFrameworkCore;


namespace IpManager.Repository.DashBoard
{
    public partial class DashBoardRepository : IDashBoardRepository
    {
        private readonly ILoggerService LoggerService;
        private readonly IpanalyzeContext context;

        public DashBoardRepository(IpanalyzeContext _context,
            ILoggerService _loggerservice)
        {
            this.context = _context;
            this.LoggerService = _loggerservice;
        }

        /// <summary>
        /// 현황 (실시간) 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AnalysisDataDTO?> GetThisTimeDataAnalysis(DateTime TargetDate)
        {
            try
            {
                var pcroomtb = await context.PcroomTbs.Where(m => m.DelYn != true).ToListAsync();

                // 먼저 조건에 맞는 데이터를 메모리로 로드
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value.Date == TargetDate.Date &&
                                m.CreateDt <= TargetDate)
                    .ToListAsync();

                // 3. PC룸별로 30분 단위 그룹핑(여기서는 PC룸ID와 시간 그룹을 함께 키로 사용)
                var groupedData = data
                    .GroupBy(m => new {
                        PcroomId = m.PcroomtbId, // PC룸과 연관된 외래키
                        GroupTime = new DateTime(
                            m.CreateDt.Value.Year,
                            m.CreateDt.Value.Month,
                            m.CreateDt.Value.Day,
                            m.CreateDt.Value.Hour,
                            (m.CreateDt.Value.Minute / 30) * 30,
                            0)
                    })
                    .Select(g => new
                    {
                        g.Key.PcroomId,
                        GroupTime = g.Key.GroupTime, // 그룹의 시작 시각 (예: 10:00:00, 10:30:00 등)
                        Count = g.Count(),           // 해당 그룹에 속한 레코드 수
                        Items = g.ToList()           // 그룹에 속한 항목들
                    })
                    .ToList();

                // 4. 각 PC룸별로 최신 그룹만 선택합니다.
                var latestGroupByRoom = groupedData
                    .GroupBy(x => x.PcroomId)
                    .Select(g => g.OrderByDescending(x => x.GroupTime).FirstOrDefault())
                    .ToList();

                // 5. PC룸과 최신 그룹을 왼쪽 조인합니다.
                //    (즉, PinglogTbs 그룹이 없는 PC룸도 결과에 포함됩니다.)
                var result = from room in pcroomtb
                             join lg in latestGroupByRoom
                                on room.Pid equals lg.PcroomId into lgJoin
                             from latest in lgJoin.DefaultIfEmpty()
                             select new
                             {
                                 Pcroom = room,
                                 LatestGroup = latest // 해당 PC룸의 최신 그룹(없으면 null)
                             };

                List<ResultData> resultData = new List<ResultData>();
                foreach(var analysis in result)
                {
                    var item = new ResultData();
                    item.pcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.totalCount = analysis.Pcroom.Seatnumber; // 총대수
                    if (analysis.LatestGroup != null && analysis.LatestGroup.Items != null)
                    {
                        foreach (var temp in analysis.LatestGroup.Items)
                        {
                            item.count = temp.UsedPc;
                        }
                    }
                    else
                    {
                        // 분석 결과가 없는것
                        item.count = 0;
                    }

                    
                    item.rate = ((float)item.count / item.totalCount) * 100;
                    item.returnRate = $"{item.count}/{item.totalCount} ({((double)item.count / item.totalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.bestName = best.pcRoomName; // 가장높은 매장명
                AnalysisData.analysisDate = DateTime.Now;
                AnalysisData.datas = resultData;


                return AnalysisData;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 매출 1위상권 & 매출 1위매장 & 가동률1위 매장 조회
        /// </summary>
        /// <returns></returns>
        public async Task<TopSalesNameDTO?> GetTopSalesNameInfo()
        {
            try
            {
                var topTown = await (
                     from a in context.AnalyzeTbs
                     join t in context.TownTbs on a.TowntbId equals t.Pid
                     group new { a, t } by new { a.TowntbId, t.Name } into grp
                     orderby grp.Count() descending
                     select new
                     {
                         TownTbId = grp.Key.TowntbId,
                         TownName = grp.Key.Name,
                         Count = grp.Count()
                     }
                 ).FirstOrDefaultAsync()
                 .ConfigureAwait(false);

                var topSales = await (
                    from a in context.AnalyzeTbs
                    join p in context.PcroomTbs on a.TopSalesPcroomtbId equals p.Pid
                    group new { a, p } by new { a.TopSalesPcroomtbId, p.Name } into grp
                    orderby grp.Count() descending
                    select new
                    {
                        PcroomTbId = grp.Key.TopSalesPcroomtbId,
                        PcroomName = grp.Key.Name,
                        Count = grp.Count()
                    }
                ).FirstOrDefaultAsync()
                .ConfigureAwait(false);

                var topUsed = await (
                    from a in context.AnalyzeTbs
                    join p in context.PcroomTbs on a.TopOpratePcroomtbId equals p.Pid
                    group new { a, p } by new { a.TopOpratePcroomtbId, p.Name } into grp
                    orderby grp.Count() descending
                    select new
                    {
                        PcroomTbId = grp.Key.TopOpratePcroomtbId,
                        PcroomName = grp.Key.Name,
                        Count = grp.Count()
                    }
                    ).FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                var model = new TopSalesNameDTO
                {
                    topSalesTownName = topTown.TownName,
                    topSalesStoreName = topSales.PcroomName,
                    topUsedRateStoreName = topUsed.PcroomName
                };

                return model;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        public async Task<List<PcroomTimeDataDto>> GetThisDayDataList(DateTime targetDate,string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                // 모든 시간 문자열 목록 ("HH:mm" 형식)
                var allTimes = await context.TimeTbs
               .OrderBy(t => t.Time)
               .ToListAsync();
              

                // 기본 쿼리
                var query = from p in context.PinglogTbs
                            join pc in context.PcroomTbs on p.PcroomtbId equals pc.Pid
                            join t in context.TimeTbs on p.TimetbId equals t.Pid
                            where p.DelYn != true &&
                                  p.CreateDt.HasValue &&
                                  p.CreateDt.Value.Date == targetDate.Date
                            select new
                            {
                                PcroomId = p.PcroomtbId,
                                PcroomName = pc.Name,
                                TimeString = t.Time.HasValue ? t.Time.Value.ToString("HH:mm") : "",
                                p.UsedPc,
                                // 가정: PC방 테이블에 국가, 구, 도시 정보가 있을 경우
                                CountryId = pc.CountrytbId,
                                TownId = pc.TowntbId,
                                CityId = pc.CitytbId
                            };

                // 조건별로 동적 필터 추가
                if (!string.IsNullOrEmpty(pcName))
                {
                    query = query.Where(x => x.PcroomName.Contains(pcName));
                }
                if (countrytbid.HasValue)
                {
                    query = query.Where(x => x.CountryId == countrytbid.Value);
                }
                if (towntbid.HasValue)
                {
                    query = query.Where(x => x.TownId == towntbid.Value);
                }
                if (citytbid.HasValue)
                {
                    query = query.Where(x => x.CityId == citytbid.Value);
                }

                // 데이터를 가져온 후 그룹화 처리
                var pingLogs = await query.ToListAsync();

                var groupedData = pingLogs
                    .GroupBy(x => new { x.PcroomId, x.PcroomName })
                    .Select(g => new
                    {
                        PcroomId = g.Key.PcroomId,
                        PcroomName = g.Key.PcroomName,
                        TimeMap = g.GroupBy(x => x.TimeString)
                                   .ToDictionary(
                                       tg => tg.Key,
                                       tg => tg.Sum(x => x.UsedPc)
                                   )
                    })
                    .ToList();

                // 모든 시간대를 기준으로 없는 시간은 0으로 채워 DTO 매핑
                var result = groupedData.Select(pc => new PcroomTimeDataDto
                {
                    pcRoomId = pc.PcroomId,
                    pcRoomName = pc.PcroomName,
                    analyList = allTimes.Select(timeTb => {
                        // key, display 모두 HH:mm 으로 통일
                        var key = timeTb.Time.Value.ToString("HH:mm");
                        return new ThisAnayzeList
                        {
                            time = key,
                            count = pc.TimeMap.TryGetValue(key, out var c) ? c : 0
                        };
                    }).ToList()
                }).ToList();

                return result;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        public async Task<List<ReturnValue>> GetPeriodDataList(DateTime startDate, DateTime endDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                // 1. 기간 설정 (날짜만 비교하기 위해 Date 사용)
                startDate = startDate.Date;
                endDate = endDate.Date;

                // 2. 전체 기간의 PingLogTbs 데이터를 조회
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value.Date >= startDate.Date &&
                                m.CreateDt.Value.Date <= endDate.Date)
                    .ToListAsync();

                // 3. 전체 PC룸 정보 조회 (PCTable 혹은 PcroomTbs)
                IQueryable<PcroomTb> query = from room in context.PcroomTbs
                                             where room.DelYn != true
                                             select room;

                if (!string.IsNullOrEmpty(pcName))
                {
                    query = query.Where(m => m.Name == pcName);
                }
                if(countrytbid is not null)
                {
                    query = query.Where(m => m.CountrytbId == countrytbid);
                }
                if (towntbid is not null)
                {
                    query = query.Where(m => m.TowntbId == towntbid);
                }
                if(citytbid is not null)
                {
                    query = query.Where(m => m.CitytbId == citytbid);
                }

                var pcrooms = await query.ToListAsync();

                // 4. 지정한 기간의 날짜별 그룹 생성 및 데이터 처리
                List<PeriodList> resultPeriodLists = new List<PeriodList>();

                // startDate부터 endDate 전날까지 각 날짜에 대해 처리
                for (var dt = startDate; dt < endDate; dt = dt.AddDays(1))
                {
                    // 해당 날짜에 해당하는 PingLogTbs 데이터를 필터링 (YYYY-MM-dd 비교)
                    var dailyData = data.Where(m => m.CreateDt.Value.Date == dt.Date).ToList();

                    // 날짜 그룹 내에서 PC룸별로 그룹핑
                    var groupedData = dailyData
                        .GroupBy(m => m.PcroomtbId)
                        .Select(g => new
                        {
                            PcroomId = g.Key,
                            SumUsedPc = g.Sum(x => x.UsedPc),
                            SumPricePc = g.Sum(x => x.Price)
                        })
                        .ToList();

                    // PC룸 테이블과 조인 (PCRoomTBId와 PC룸 테이블의 Pid가 일치)
                    var dailyGroupedData = from room in pcrooms
                                           join gd in groupedData on room.Pid equals gd.PcroomId into gdJoin
                                           from gd in gdJoin.DefaultIfEmpty()
                                           select new
                                           {
                                               Pcroom = room,
                                               TotalUsedPc = gd != null ? gd.SumUsedPc : 0,
                                               TotalPricePc = gd != null ? gd.SumPricePc : 0
                                           };

                    // 해당 날짜에 대해 분석 결과 생성
                    List<PeriodAnayzeList> dailyAnalyzeList = new List<PeriodAnayzeList>();
                    foreach (var analysis in dailyGroupedData)
                    {
                        // 만약 해당 PC룸에 관련 데이터가 없더라도(0으로 처리) 필요한 경우 추가하거나, 필터링할 수 있습니다.
                        var item = new PeriodAnayzeList();
                        item.pcName = analysis.Pcroom.Name;

                        
                        item.usedPc = analysis.TotalUsedPc / 48.0;
                        item.seatNumber = analysis.Pcroom.Seatnumber;
                        // 평균 가동률 계산 (Seatnumber가 0이면 0%로 처리)
                        double rate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber)*100;
                        item.averageRate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber) * 100;

                        // PC 이용 매출 (PingLogTb의 Price 합계)
                        int pcPrice = (int)analysis.TotalPricePc;
                        // 식품 및 기타 매출 계산: (전체 Price * ((100 - PricePercent) / PricePercent))
                        int foodPrice = (int)Math.Round(analysis.TotalPricePc * ((100.0 - analysis.Pcroom.PricePercent) / analysis.Pcroom.PricePercent));
                        int totalPrice = pcPrice + foodPrice;

                        item.pcPrice = pcPrice;
                        item.foodPrice = foodPrice;
                        item.totalPrice = totalPrice;
                        item.pricePercent = analysis.Pcroom.PricePercent+"%";

                        dailyAnalyzeList.Add(item);
                    }

                    // 해당 날짜에 하나 이상의 분석 데이터가 있다면 결과에 추가
                    if (dailyAnalyzeList.Any())
                    {
                        PeriodList periodList = new PeriodList
                        {
                            AnalyzeDT = dt.ToString("yyyy-MM-dd"),
                            AnalyzeList = dailyAnalyzeList
                        };

                        resultPeriodLists.Add(periodList);
                    }
                }

                int dayCount = resultPeriodLists.Count;

                // PeriodList의 AnalyzeList를 모두 평탄화한 후, PC방 상호별로 그룹화
                List<ReturnValue> aggregatedReturnValues = resultPeriodLists
                    .SelectMany(p => p.AnalyzeList)
                     .GroupBy(item => new { item.pcName, item.seatNumber })
                    .Select(g => new ReturnValue
                    {
                        pcName = g.Key.pcName,
                        // 각 항목의 합을 날짜 수로 나눈 평균값을 소수점 둘째자리까지 문자열로 변환
                        usedPc = (g.Sum(x => x.usedPc ?? 0) / dayCount).ToString("F2") + "/" + g.Key.seatNumber,
                        averageRate = (g.Sum(x => x.averageRate ?? 0) / dayCount).ToString("F2") + "%",
                        pcPrice = (g.Sum(x => x.pcPrice ?? 0) / dayCount).ToString("F2") + "원",
                        foodPrice = (g.Sum(x => x.foodPrice ?? 0) / dayCount).ToString("F2") + "원",
                        totalPrice = (g.Sum(x => x.totalPrice ?? 0) / dayCount).ToString("F2") + "원",
                        // pricePercent는 날짜마다 동일하다고 가정하여 그룹의 첫번째 값 사용
                        pricePercent = g.First().pricePercent
                    })
                    .ToList();

                return aggregatedReturnValues;
          

            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 월간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ReturnValue>?> GetMonthDataAnalysis(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {
                // 해당 월의 첫 번째 날 (00:00:00)
                DateTime StartDate = new DateTime(TargetDate.Year, TargetDate.Month, 1);

                // 해당 월의 마지막 날 계산 (마지막 날의 23시 59분 59초)
                int lastDay = DateTime.DaysInMonth(TargetDate.Year, TargetDate.Month);
                DateTime EndDate = new DateTime(TargetDate.Year, TargetDate.Month, lastDay, 23, 59, 59);

                // 전체 기간의 PingLogTbs 데이터를 조회
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                    m.CreateDt.Value.Date >= StartDate.Date &&
                    m.CreateDt.Value.Date <= EndDate.Date)
                    .ToListAsync();

                // 전체 PC룸 정보 조회
                IQueryable<PcroomTb> query = from room in context.PcroomTbs
                                             where room.DelYn != true
                                             select room;

                if(!string.IsNullOrEmpty(pcName))
                {
                    query = query.Where(m => m.Name == pcName);
                }

                if(countrytbid is not null)
                {
                    query = query.Where(m => m.CountrytbId == countrytbid);
                }

                if(towntbid is not null)
                {
                    query = query.Where(m => m.TowntbId == towntbid);
                }

                if(citytbid is not null)
                {
                    query = query.Where(m => m.CitytbId == citytbid);
                }

                var pcrooms = await query.ToListAsync();


                // 지정한 기간의 날짜별 그룹 생성 및 데이터 처리
                List<PeriodList> resultPeriodLists = new List<PeriodList>();

                // startDate부터 endDate 전날까지 각 날짜에 대해 처리
                for(var dt = StartDate; dt < EndDate; dt=dt.AddDays(1))
                {
                    // 해당 날짜에 해당하는 PingLogTbs 데이터를 필터링 (YYYY-MM-dd 비교)
                    var dailyData = data.Where(m => m.CreateDt.Value.Date == dt.Date).ToList();

                    // 날짜 그룹 내에서 PC룸별로 그룹핑
                    var groupedData = dailyData
                        .GroupBy(m => m.PcroomtbId)
                        .Select(g => new
                        {
                            PcroomId = g.Key,
                            SumUsedPc = g.Sum(x => x.UsedPc),
                            SumPricePc = g.Sum(x => x.Price)
                        }).ToList();

                    // PC룸 테이블과 조인 (PcRoomTBId와 PC룸 테이블 Pid가 일치)
                    var dailyGroupedData = from room in pcrooms
                                           join gd in groupedData on room.Pid equals gd.PcroomId into gdJoin
                                           from gd in gdJoin.DefaultIfEmpty()
                                           select new
                                           {
                                               Pcroom = room,
                                               TotalUsedPc = gd != null ? gd.SumUsedPc : 0,
                                               TotalPricePc = gd != null ? gd.SumPricePc : 0
                                           };


                    // 해당 날짜에 대해 분석 결과 생성
                    List<PeriodAnayzeList> dailyAnalyzeList = new List<PeriodAnayzeList>();
                    foreach (var analysis in dailyGroupedData)
                    {
                        // 만약 해당 PC룸에 관련 데이터가 없더라도(0으로 처리) 필요한 경우 추가하거나, 필터링할 수 있습니다.
                        var item = new PeriodAnayzeList();
                        item.pcName = analysis.Pcroom.Name;


                        item.usedPc = analysis.TotalUsedPc / 48.0;
                        item.seatNumber = analysis.Pcroom.Seatnumber;
                        // 평균 가동률 계산 (Seatnumber가 0이면 0%로 처리)
                        double rate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber) * 100;
                        item.averageRate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber) * 100;

                        // PC 이용 매출 (PingLogTb의 Price 합계)
                        int pcPrice = (int)analysis.TotalPricePc;
                        // 식품 및 기타 매출 계산: (전체 Price * ((100 - PricePercent) / PricePercent))
                        int foodPrice = (int)Math.Round(analysis.TotalPricePc * ((100.0 - analysis.Pcroom.PricePercent) / analysis.Pcroom.PricePercent));
                        int totalPrice = pcPrice + foodPrice;

                        item.pcPrice = pcPrice;
                        item.foodPrice = foodPrice;
                        item.totalPrice = totalPrice;
                        item.pricePercent = analysis.Pcroom.PricePercent + "%";

                        dailyAnalyzeList.Add(item);
                    }

                    // 해당 날짜에 하나 이상의 분석 데이터가 있다면 결과에 추가
                    if (dailyAnalyzeList.Any())
                    {
                        PeriodList periodList = new PeriodList
                        {
                            AnalyzeDT = dt.ToString("yyyy-MM-dd"),
                            AnalyzeList = dailyAnalyzeList
                        };

                        resultPeriodLists.Add(periodList);
                    }
                }

                int dayCount = resultPeriodLists.Count;

                // PeriodList의 AnalyzeList를 모두 평탄화한 후, PC방 상호별로 그룹화
                List<ReturnValue> aggregatedReturnValues = resultPeriodLists
                    .SelectMany(p => p.AnalyzeList)
                     .GroupBy(item => new { item.pcName, item.seatNumber })
                    .Select(g => new ReturnValue
                    {
                        pcName = g.Key.pcName,
                        // 각 항목의 합을 날짜 수로 나눈 평균값을 소수점 둘째자리까지 문자열로 변환
                        usedPc = (g.Sum(x => x.usedPc ?? 0) / dayCount).ToString("F2") + "/" + g.Key.seatNumber,
                        averageRate = (g.Sum(x => x.averageRate ?? 0) / dayCount).ToString("F2") + "%",
                        pcPrice = (g.Sum(x => x.pcPrice ?? 0) / dayCount).ToString("F2") + "원",
                        foodPrice = (g.Sum(x => x.foodPrice ?? 0) / dayCount).ToString("F2") + "원",
                        totalPrice = (g.Sum(x => x.totalPrice ?? 0) / dayCount).ToString("F2") + "원",
                        // pricePercent는 날짜마다 동일하다고 가정하여 그룹의 첫번째 값 사용
                        pricePercent = g.First().pricePercent
                    })
                    .ToList();

                return aggregatedReturnValues;

            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 하루 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <param name="pcName"></param>
        /// <param name="countrytbid"></param>
        /// <param name="towntbid"></param>
        /// <param name="citytbid"></param>
        /// <returns></returns>
        public async Task<List<ReturnValue>?> GetDaysDataAnalysis(DateTime TargetDate, string? pcName, int? countrytbid, int? towntbid, int? citytbid)
        {
            try
            {

                // 2. 전체 기간의 PingLogTbs 데이터를 조회
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value.Date == TargetDate.Date)
                    .ToListAsync();

                // 3. 전체 PC룸 정보 조회 (PCTable 혹은 PcroomTbs)
                IQueryable<PcroomTb> query = from room in context.PcroomTbs
                                             where room.DelYn != true
                                             select room;

                if (!string.IsNullOrEmpty(pcName))
                {
                    query = query.Where(m => m.Name == pcName);
                }
                if (countrytbid is not null)
                {
                    query = query.Where(m => m.CountrytbId == countrytbid);
                }
                if (towntbid is not null)
                {
                    query = query.Where(m => m.TowntbId == towntbid);
                }
                if (citytbid is not null)
                {
                    query = query.Where(m => m.CitytbId == citytbid);
                }

                var pcrooms = await query.ToListAsync();

                // 4. 지정한 기간의 날짜별 그룹 생성 및 데이터 처리
                List<PeriodList> resultPeriodLists = new List<PeriodList>();

                // startDate부터 endDate 전날까지 각 날짜에 대해 처리
                for (var dt = TargetDate; dt < TargetDate.AddDays(1); dt = dt.AddDays(1))
                {
                    // 해당 날짜에 해당하는 PingLogTbs 데이터를 필터링 (YYYY-MM-dd 비교)
                    var dailyData = data.Where(m => m.CreateDt.Value.Date == dt.Date).ToList();

                    // 날짜 그룹 내에서 PC룸별로 그룹핑
                    var groupedData = dailyData
                        .GroupBy(m => m.PcroomtbId)
                        .Select(g => new
                        {
                            PcroomId = g.Key,
                            SumUsedPc = g.Sum(x => x.UsedPc),
                            SumPricePc = g.Sum(x => x.Price)
                        })
                        .ToList();

                    // PC룸 테이블과 조인 (PCRoomTBId와 PC룸 테이블의 Pid가 일치)
                    var dailyGroupedData = from room in pcrooms
                                           join gd in groupedData on room.Pid equals gd.PcroomId into gdJoin
                                           from gd in gdJoin.DefaultIfEmpty()
                                           select new
                                           {
                                               Pcroom = room,
                                               TotalUsedPc = gd != null ? gd.SumUsedPc : 0,
                                               TotalPricePc = gd != null ? gd.SumPricePc : 0
                                           };

                    // 해당 날짜에 대해 분석 결과 생성
                    List<PeriodAnayzeList> dailyAnalyzeList = new List<PeriodAnayzeList>();
                    foreach (var analysis in dailyGroupedData)
                    {
                        // 만약 해당 PC룸에 관련 데이터가 없더라도(0으로 처리) 필요한 경우 추가하거나, 필터링할 수 있습니다.
                        var item = new PeriodAnayzeList();
                        item.pcName = analysis.Pcroom.Name;


                        item.usedPc = analysis.TotalUsedPc / 48.0;
                        item.seatNumber = analysis.Pcroom.Seatnumber;
                        // 평균 가동률 계산 (Seatnumber가 0이면 0%로 처리)
                        double rate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber) * 100;
                        item.averageRate = ((analysis.TotalUsedPc / 48.0) / analysis.Pcroom.Seatnumber) * 100;

                        // PC 이용 매출 (PingLogTb의 Price 합계)
                        int pcPrice = (int)analysis.TotalPricePc;
                        // 식품 및 기타 매출 계산: (전체 Price * ((100 - PricePercent) / PricePercent))
                        int foodPrice = (int)Math.Round(analysis.TotalPricePc * ((100.0 - analysis.Pcroom.PricePercent) / analysis.Pcroom.PricePercent));
                        int totalPrice = pcPrice + foodPrice;

                        item.pcPrice = pcPrice;
                        item.foodPrice = foodPrice;
                        item.totalPrice = totalPrice;
                        item.pricePercent = analysis.Pcroom.PricePercent + "%";

                        dailyAnalyzeList.Add(item);
                    }

                    // 해당 날짜에 하나 이상의 분석 데이터가 있다면 결과에 추가
                    if (dailyAnalyzeList.Any())
                    {
                        PeriodList periodList = new PeriodList
                        {
                            AnalyzeDT = dt.ToString("yyyy-MM-dd"),
                            AnalyzeList = dailyAnalyzeList
                        };

                        resultPeriodLists.Add(periodList);
                    }
                }

                int dayCount = resultPeriodLists.Count;

                // PeriodList의 AnalyzeList를 모두 평탄화한 후, PC방 상호별로 그룹화
                List<ReturnValue> aggregatedReturnValues = resultPeriodLists
                    .SelectMany(p => p.AnalyzeList)
                     .GroupBy(item => new { item.pcName, item.seatNumber })
                    .Select(g => new ReturnValue
                    {
                        pcName = g.Key.pcName,
                        // 각 항목의 합을 날짜 수로 나눈 평균값을 소수점 둘째자리까지 문자열로 변환
                        usedPc = (g.Sum(x => x.usedPc ?? 0) / dayCount).ToString("F2") + "/" + g.Key.seatNumber,
                        averageRate = (g.Sum(x => x.averageRate ?? 0) / dayCount).ToString("F2") + "%",
                        pcPrice = (g.Sum(x => x.pcPrice ?? 0) / dayCount).ToString("F2") + "원",
                        foodPrice = (g.Sum(x => x.foodPrice ?? 0) / dayCount).ToString("F2") + "원",
                        totalPrice = (g.Sum(x => x.totalPrice ?? 0) / dayCount).ToString("F2") + "원",
                        // pricePercent는 날짜마다 동일하다고 가정하여 그룹의 첫번째 값 사용
                        pricePercent = g.First().pricePercent
                    })
                    .ToList();

                return aggregatedReturnValues;

            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }
    }
}
