using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.DashBoard;
using Microsoft.EntityFrameworkCore;


namespace IpManager.Repository.DashBoard
{
    public partial class DashBoardRepository : IDashBoardRepository
    {
        private readonly ILoggerService LoggerService;
        private readonly IDbContextFactory<IpanalyzeContext> _dbContextFactory;

        public DashBoardRepository(IDbContextFactory<IpanalyzeContext> dbContextFactory,
            ILoggerService _loggerservice)
        {
            this._dbContextFactory = dbContextFactory;
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
                await using var context = _dbContextFactory.CreateDbContext();
                
                var pcroomtb = await context.PcroomTbs.Where(m => m.DelYn == false).ToListAsync();

                // 먼저 조건에 맞는 데이터를 메모리로 로드
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn == false &&
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
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트
                
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
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트
                
                // 모든 시간 문자열 목록 ("HH:mm" 형식)
                var allTimes = await context.TimeTbs
               .OrderBy(t => t.Time)
               .ToListAsync();
              

                // 기본 쿼리
                var query = from p in context.PinglogTbs
                            join pc in context.PcroomTbs on p.PcroomtbId equals pc.Pid
                            join Country in context.CountryTbs on pc.CountrytbId equals Country.Pid 
                            join City in context.CityTbs on pc.CitytbId equals City.Pid
                            join Town in context.TownTbs on pc.TowntbId equals Town.Pid
                            join t in context.TimeTbs on p.TimetbId equals t.Pid
                            where p.DelYn != true &&
                                  pc.DelYn != true &&
                                  Country.DelYn != true &&
                                  City.DelYn != true &&
                                  Town.DelYn != true &&
                                  p.CreateDt.HasValue &&
                                  p.CreateDt.Value.Date == targetDate.Date
                            select new
                            {
                                PcroomId = p.PcroomtbId,
                                CountryId = Country.Pid,
                                CountryName = Country.Name,
                                CityId = City.Pid,
                                CityName = City.Name,
                                TownId = Town.Pid,
                                TownName = Town.Name,
                                PcroomName = pc.Name,
                                TimeString = t.Time.HasValue ? t.Time.Value.ToString("HH:mm") : "",
                                p.UsedPc,
                                // 가정: PC방 테이블에 국가, 구, 도시 정보가 있을 경우
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
                    .GroupBy(x => new { x.PcroomId, x.PcroomName,
                        x.CountryId,
                        x.CountryName,
                        x.CityId,
                        x.CityName,
                        x.TownId,
                        x.TownName
                    })
                    .Select(g => new
                    {
                        PcroomId = g.Key.PcroomId,
                        PcroomName = g.Key.PcroomName,
                        CountryId = g.Key.CountryId,
                        CountryName = g.Key.CountryName,
                        CityId = g.Key.CityId,
                        CityName = g.Key.CityName,
                        TownId = g.Key.TownId,
                        TownName = g.Key.TownName,
                        TimeMap = g
                        .GroupBy(y => y.TimeString)
                        .ToDictionary(
                           tg => tg.Key,
                           tg => tg.Sum(y => y.UsedPc)
                        )
                    })
                    .ToList();

                // 모든 시간대를 기준으로 없는 시간은 0으로 채워 DTO 매핑
                var result = groupedData.Select(pc => new PcroomTimeDataDto
                {
                    pcRoomId = pc.PcroomId,
                    pcRoomName = pc.PcroomName,
                    countryId = pc.CountryId,
                    countryName = pc.CountryName,
                    cityId = pc.CityId,
                    cityName = pc.CityName,
                    townId = pc.TownId,
                    townName = pc.TownName,
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
                await using var ctx = _dbContextFactory.CreateDbContext();
                startDate = startDate.Date;
                endDate = endDate.Date;

                // 1) PC방 + 지역(FK) 메타정보 한 번에 로드
                var pcList = await ctx.PcroomTbs
                    .AsNoTracking()
                    .Where(r => !r.DelYn.Value
                                && (string.IsNullOrEmpty(pcName) || r.Name.Contains(pcName))
                                && (!countrytbid.HasValue || r.CountrytbId == countrytbid)
                                && (!citytbid.HasValue || r.CitytbId == citytbid)
                                && (!towntbid.HasValue || r.TowntbId == towntbid))
                    .Join(ctx.CountryTbs.Where(co => co.DelYn != true), pc => pc.CountrytbId, co => co.Pid,
                          (pc, co) => new { pc, co })
                    .Join(ctx.CityTbs.Where(ci => ci.DelYn != true), tmp => tmp.pc.CitytbId, ci => ci.Pid,
                          (tmp, ci) => new { tmp.pc, tmp.co, ci })
                    .Join(ctx.TownTbs.Where(ti => ti.DelYn != true), tmp => tmp.pc.TowntbId, to => to.Pid,
                          (tmp, to) => new
                          {
                              Pcroom = tmp.pc,
                              CountryId = tmp.co.Pid,
                              CountryName = tmp.co.Name,
                              CityId = tmp.ci.Pid,
                              CityName = tmp.ci.Name,
                              TownId = to.Pid,
                              TownName = to.Name
                          })
                    .Select(x => new
                    {
                        x.Pcroom.Pid,
                        x.Pcroom.Name,
                        x.Pcroom.Seatnumber,
                        x.Pcroom.PricePercent,
                        x.CountryId,
                        x.CountryName,
                        x.CityId,
                        x.CityName,
                        x.TownId,
                        x.TownName
                    })
                    .ToListAsync();

                // 2) 로그를 DB에서 “날짜·PC방별”로 집계
                var stats = await ctx.PinglogTbs
                    .AsNoTracking()
                    .Where(p => !p.DelYn.Value
                                && p.CreateDt.HasValue
                                && p.CreateDt.Value.Date >= startDate
                                && p.CreateDt.Value.Date <= endDate)
                    .GroupBy(p => new {
                        Date = p.CreateDt.Value.Date,
                        RoomId = p.PcroomtbId
                    })
                    .Select(g => new {
                        g.Key.Date,
                        g.Key.RoomId,
                        SumUsedPc = g.Sum(x => x.UsedPc),
                        SumPrice = g.Sum(x => x.Price)
                    })
                    .ToListAsync();

                // 3) 전체 날짜 리스트 생성
                var allDates = Enumerable
                    .Range(0, (endDate - startDate).Days + 1)
                    .Select(d => startDate.AddDays(d))
                    .ToList();

                // 4) 날짜별·PC방별 → PeriodList
                var periodLists = allDates
                    .Select(date =>
                    {
                        var items = from pc in pcList
                                    join s in stats
                                      on (pc.Pid, date) equals (s.RoomId, s.Date)
                                      into gj
                                    from s in gj.DefaultIfEmpty()
                                    select new PeriodAnayzeList
                                    {
                                        countryId = pc.CountryId,
                                        countryName = pc.CountryName,
                                        cityId = pc.CityId,
                                        cityName = pc.CityName,
                                        townId = pc.TownId,
                                        townName = pc.TownName,
                                        pcName = pc.Name,
                                        usedPc = (s?.SumUsedPc ?? 0) / 48.0,
                                        seatNumber = pc.Seatnumber,
                                        averageRate = pc.Seatnumber == 0
                                                      ? 0
                                                      : ((s?.SumUsedPc ?? 0) / 48.0) / pc.Seatnumber * 100,
                                        pcPrice = s?.SumPrice ?? 0,
                                        foodPrice = Math.Round((s?.SumPrice ?? 0) * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                        totalPrice = (s?.SumPrice ?? 0)
                                                      + Math.Round((s?.SumPrice ?? 0) * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                        pricePercent = pc.PricePercent + "%"
                                    };

                        return new PeriodList
                        {
                            AnalyzeDT = date.ToString("yyyy-MM-dd"),
                            AnalyzeList = items.ToList()
                        };
                    })
                    .Where(pl => pl.AnalyzeList.Any())
                    .ToList();

                // 5) 최종 평균(ReturnValue) — 여기서도 지역 정보 유지
                int dayCount = periodLists.Count;
                var returnValues = periodLists
                    .SelectMany(pl => pl.AnalyzeList)
                    .GroupBy(x => new {
                        x.countryId,
                        x.countryName,
                        x.cityId,
                        x.cityName,
                        x.townId,
                        x.townName,
                        x.pcName,
                        x.seatNumber,
                        x.pricePercent
                    })
                    .Select(g => new ReturnValue
                    {
                        countryId = g.Key.countryId,
                        countryName = g.Key.countryName,
                        cityId = g.Key.cityId,
                        cityName = g.Key.cityName,
                        townId = g.Key.townId,
                        townName = g.Key.townName,
                        pcName = g.Key.pcName,
                        usedPc = $"{(g.Sum(x => x.usedPc ?? 0) / dayCount):F2}/{g.Key.seatNumber}",
                        averageRate = $"{(g.Sum(x => x.averageRate ?? 0) / dayCount):F2}%",
                        pcPrice = $"{g.Sum(x => x.pcPrice ?? 0):F2}원",
                        foodPrice = $"{g.Sum(x => x.foodPrice ?? 0):F2}원",
                        totalPrice = $"{g.Sum(x => x.totalPrice ?? 0):F2}원",
                        pricePercent = g.Key.pricePercent
                    })
                    .ToList();

                return returnValues;

                /*
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트
                
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
                                             join Country in context.CountryTbs on room.CountrytbId equals Country.Pid
                                             join City in context.CityTbs on room.CitytbId equals City.Pid
                                             join Town in context.TownTbs on room.TowntbId equals Town.Pid
                                             where room.DelYn != true &&
                                             Country.DelYn != true &&
                                             City.DelYn != true &&
                                             Town.DelYn != true 
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
          
                */
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
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트

                // 해당 월의 첫 번째 날 (00:00:00)
                DateTime StartDate = new DateTime(TargetDate.Year, TargetDate.Month, 1);

                // 해당 월의 마지막 날 계산 (마지막 날의 23시 59분 59초)
                int lastDay = DateTime.DaysInMonth(TargetDate.Year, TargetDate.Month);
                DateTime EndDate = new DateTime(TargetDate.Year, TargetDate.Month, lastDay, 23, 59, 59);


                // 1) PC방 + 지역(FK) 메타정보 한 번에 로드
                var pcList = await context.PcroomTbs
                    .AsNoTracking()
                    .Where(r => !r.DelYn.Value
                                && (string.IsNullOrEmpty(pcName) || r.Name.Contains(pcName))
                                && (!countrytbid.HasValue || r.CountrytbId == countrytbid)
                                && (!citytbid.HasValue || r.CitytbId == citytbid)
                                && (!towntbid.HasValue || r.TowntbId == towntbid))
                    .Join(context.CountryTbs.Where(co => co.DelYn != true), pc => pc.CountrytbId, co => co.Pid,
                          (pc, co) => new { pc, co })
                    .Join(context.CityTbs.Where(ci => ci.DelYn != true), tmp => tmp.pc.CitytbId, ci => ci.Pid,
                          (tmp, ci) => new { tmp.pc, tmp.co, ci })
                    .Join(context.TownTbs.Where(ti => ti.DelYn != true), tmp => tmp.pc.TowntbId, to => to.Pid,
                          (tmp, to) => new
                          {
                              Pcroom = tmp.pc,
                              CountryId = tmp.co.Pid,
                              CountryName = tmp.co.Name,
                              CityId = tmp.ci.Pid,
                              CityName = tmp.ci.Name,
                              TownId = to.Pid,
                              TownName = to.Name
                          })
                    .Select(x => new
                    {
                        x.Pcroom.Pid,
                        x.Pcroom.Name,
                        x.Pcroom.Seatnumber,
                        x.Pcroom.PricePercent,
                        x.CountryId,
                        x.CountryName,
                        x.CityId,
                        x.CityName,
                        x.TownId,
                        x.TownName
                    })
                    .ToListAsync();


                // 2) 로그를 DB에서 “날짜·PC방별”로 집계
                var stats = await context.PinglogTbs
                    .AsNoTracking()
                    .Where(p => !p.DelYn.Value
                                && p.CreateDt.HasValue
                                && p.CreateDt.Value.Date >= StartDate
                                && p.CreateDt.Value.Date <= EndDate)
                    .GroupBy(p => new {
                        Date = p.CreateDt.Value.Date,
                        RoomId = p.PcroomtbId
                    })
                    .Select(g => new {
                        g.Key.Date,
                        g.Key.RoomId,
                        SumUsedPc = g.Sum(x => x.UsedPc),
                        SumPrice = g.Sum(x => x.Price)
                    })
                    .ToListAsync();

                // 3) 전체 날짜 리스트 생성
                var allDates = Enumerable
                    .Range(0, (EndDate - StartDate).Days + 1)
                    .Select(d => StartDate.AddDays(d))
                    .ToList();

                // 4) 날짜별·PC방별 → PeriodList
                var periodLists = allDates
                    .Select(date =>
                    {
                        var items = from pc in pcList
                                    join s in stats
                                      on (pc.Pid, date) equals (s.RoomId, s.Date)
                                      into gj
                                    from s in gj.DefaultIfEmpty()
                                    select new PeriodAnayzeList
                                    {
                                        countryId = pc.CountryId,
                                        countryName = pc.CountryName,
                                        cityId = pc.CityId,
                                        cityName = pc.CityName,
                                        townId = pc.TownId,
                                        townName = pc.TownName,
                                        pcName = pc.Name,
                                        usedPc = (s?.SumUsedPc ?? 0) / 48.0,
                                        seatNumber = pc.Seatnumber,
                                        averageRate = pc.Seatnumber == 0
                                                      ? 0
                                                      : ((s?.SumUsedPc ?? 0) / 48.0) / pc.Seatnumber * 100,
                                        pcPrice = s?.SumPrice ?? 0,
                                        foodPrice = Math.Round((s?.SumPrice ?? 0) * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                        totalPrice = (s?.SumPrice ?? 0)
                                                      + Math.Round((s?.SumPrice ?? 0) * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                        pricePercent = pc.PricePercent + "%"
                                    };

                        return new PeriodList
                        {
                            AnalyzeDT = date.ToString("yyyy-MM-dd"),
                            AnalyzeList = items.ToList()
                        };
                    })
                    .Where(pl => pl.AnalyzeList.Any())
                    .ToList();

                // 5) 최종 평균(ReturnValue) — 여기서도 지역 정보 유지
                int dayCount = periodLists.Count;
                var returnValues = periodLists
                    .SelectMany(pl => pl.AnalyzeList)
                    .GroupBy(x => new {
                        x.countryId,
                        x.countryName,
                        x.cityId,
                        x.cityName,
                        x.townId,
                        x.townName,
                        x.pcName,
                        x.seatNumber,
                        x.pricePercent
                    })
                    .Select(g => new ReturnValue
                    {
                        countryId = g.Key.countryId,
                        countryName = g.Key.countryName,
                        cityId = g.Key.cityId,
                        cityName = g.Key.cityName,
                        townId = g.Key.townId,
                        townName = g.Key.townName,
                        pcName = g.Key.pcName,
                        usedPc = $"{(g.Sum(x => x.usedPc ?? 0) / dayCount):F2}/{g.Key.seatNumber}",
                        averageRate = $"{(g.Sum(x => x.averageRate ?? 0) / dayCount):F2}%",
                        pcPrice = $"{g.Sum(x => x.pcPrice ?? 0):F2}원",
                        foodPrice = $"{g.Sum(x => x.foodPrice ?? 0):F2}원",
                        totalPrice = $"{g.Sum(x => x.totalPrice ?? 0):F2}원",
                        pricePercent = g.Key.pricePercent
                    })
                    .ToList();

                return returnValues;

                /*
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트
                
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
                */
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
                await using var ctx = _dbContextFactory.CreateDbContext();
                var date = TargetDate.Date;

                // 1) PC방 + 지역(FK) 메타정보 로드 (DelYn 필터 포함)
                var pcList = await ctx.PcroomTbs
                    .AsNoTracking()
                    .Where(r => !r.DelYn.Value
                                && (string.IsNullOrEmpty(pcName) || r.Name.Contains(pcName))
                                && (!countrytbid.HasValue || r.CountrytbId == countrytbid)
                                && (!citytbid.HasValue || r.CitytbId == citytbid)
                                && (!towntbid.HasValue || r.TowntbId == towntbid))
                    .Join(ctx.CountryTbs.Where(co => !co.DelYn.Value),
                          pc => pc.CountrytbId, co => co.Pid,
                          (pc, co) => new { pc, co })
                    .Join(ctx.CityTbs.Where(ci => !ci.DelYn.Value),
                          tmp => tmp.pc.CitytbId, ci => ci.Pid,
                          (tmp, ci) => new { tmp.pc, tmp.co, ci })
                    .Join(ctx.TownTbs.Where(to => !to.DelYn.Value),
                          tmp => tmp.pc.TowntbId, to => to.Pid,
                          (tmp, to) => new {
                              tmp.pc.Pid,
                              tmp.pc.Name,
                              tmp.pc.Seatnumber,
                              tmp.pc.PricePercent,
                              CountryId = tmp.co.Pid,
                              CountryName = tmp.co.Name,
                              CityId = tmp.ci.Pid,
                              CityName = tmp.ci.Name,
                              TownId = to.Pid,
                              TownName = to.Name
                          })
                    .ToListAsync();

                // 2) 하루치 로그를 PC방별로 집계
                var stats = await ctx.PinglogTbs
                    .AsNoTracking()
                    .Where(p => !p.DelYn.Value
                                && p.CreateDt.HasValue
                                && p.CreateDt.Value.Date == date)
                    .GroupBy(p => p.PcroomtbId)
                    .Select(g => new {
                        RoomId = g.Key,
                        SumUsedPc = g.Sum(x => x.UsedPc),
                        SumPrice = g.Sum(x => x.Price)
                    })
                    .ToListAsync();

                // 3) 메모리에서 조인 후 PeriodAnayzeList 생성
                var dailyList = from pc in pcList
                                join s in stats on pc.Pid equals s.RoomId into gj
                                from s in gj.DefaultIfEmpty()
                                select new PeriodAnayzeList
                                {
                                    countryId = pc.CountryId,
                                    countryName = pc.CountryName,
                                    cityId = pc.CityId,
                                    cityName = pc.CityName,
                                    townId = pc.TownId,
                                    townName = pc.TownName,
                                    pcName = pc.Name,
                                    usedPc = (s?.SumUsedPc ?? 0) / 48.0,
                                    seatNumber = pc.Seatnumber,
                                    averageRate = pc.Seatnumber == 0
                                                  ? 0
                                                  : ((s?.SumUsedPc ?? 0) / 48.0) / pc.Seatnumber * 100,
                                    pcPrice = s?.SumPrice ?? 0,
                                    foodPrice = Math.Round((s?.SumPrice ?? 0)
                                                     * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                    totalPrice = (s?.SumPrice ?? 0)
                                                  + Math.Round((s?.SumPrice ?? 0)
                                                     * ((100.0 - pc.PricePercent) / pc.PricePercent)),
                                    pricePercent = pc.PricePercent + "%"
                                };

                // 4) 하루치 ReturnValue 생성 (dayCount=1 이므로 평균=실제값)
                var returnValues = dailyList
                    .GroupBy(x => new {
                        x.countryId,
                        x.countryName,
                        x.cityId,
                        x.cityName,
                        x.townId,
                        x.townName,
                        x.pcName,
                        x.seatNumber,
                        x.pricePercent
                    })
                    .Select(g => new ReturnValue
                    {
                        countryId = g.Key.countryId,
                        countryName = g.Key.countryName,
                        cityId = g.Key.cityId,
                        cityName = g.Key.cityName,
                        townId = g.Key.townId,
                        townName = g.Key.townName,
                        pcName = g.Key.pcName,
                        usedPc = $"{g.Sum(x => x.usedPc ?? 0):F2}/{g.Key.seatNumber}",
                        averageRate = $"{g.Sum(x => x.averageRate ?? 0):F2}%",
                        pcPrice = $"{g.Sum(x => x.pcPrice ?? 0):F2}원",
                        foodPrice = $"{g.Sum(x => x.foodPrice ?? 0):F2}원",
                        totalPrice = $"{g.Sum(x => x.totalPrice ?? 0):F2}원",
                        pricePercent = g.Key.pricePercent
                    })
                    .ToList();

                return returnValues;

                /*
                await using var context = _dbContextFactory.CreateDbContext(); // ✅ 핵심 변경 포인트
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
                */
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        public async Task<List<XlsxDTO>?> GetXlsxDataList(DateTime startDate, DateTime endDate, List<int> pcId, string? pcName, int? countrytbId, int? towntbId, int? citytbid)
        {
            try
            {
                startDate = startDate.Date;
                endDate = endDate.Date;

                await using var ctx = _dbContextFactory.CreateDbContext();

                // 1) PC방 + 지역 메타정보 로드 (pcId 필터 추가)
                var pcList = await ctx.PcroomTbs
                    .AsNoTracking()
                    .Where(r => !r.DelYn.Value
                                // pcId가 null이 아니면, 리스트에 포함된 Pid만 남김
                                && (pcId == null || pcId.Contains(r.Pid))
                                && (string.IsNullOrEmpty(pcName) || r.Name.Contains(pcName))
                                && (!countrytbId.HasValue || r.CountrytbId == countrytbId)
                                && (!citytbid.HasValue || r.CitytbId == citytbid)
                                && (!towntbId.HasValue || r.TowntbId == towntbId))
                    .Join(ctx.CountryTbs.Where(co => co.DelYn != true),
                          pc => pc.CountrytbId, co => co.Pid,
                          (pc, co) => new { pc, co })
                    .Join(ctx.CityTbs.Where(ci => ci.DelYn != true),
                          tmp => tmp.pc.CitytbId, ci => ci.Pid,
                          (tmp, ci) => new { tmp.pc, tmp.co, ci })
                    .Join(ctx.TownTbs.Where(ti => ti.DelYn != true),
                          tmp => tmp.pc.TowntbId, to => to.Pid,
                          (tmp, to) => new
                          {
                              PcId = tmp.pc.Pid,
                              PcName = tmp.pc.Name,
                              SeatNumber = tmp.pc.Seatnumber,
                              PricePercent = tmp.pc.PricePercent,
                              CountryId = tmp.co.Pid,
                              CountryName = tmp.co.Name,
                              CityId = tmp.ci.Pid,
                              CityName = tmp.ci.Name,
                              TownId = to.Pid,
                              TownName = to.Name
                          })
                    .ToListAsync();

                // 2) 로그를 날짜·PC방별로 집계
                var stats = await ctx.PinglogTbs
                    .AsNoTracking()
                    .Where(p => !p.DelYn.Value
                                && p.CreateDt.HasValue
                                && p.CreateDt.Value.Date >= startDate
                                && p.CreateDt.Value.Date <= endDate)
                    .GroupBy(p => new {
                        Date = p.CreateDt.Value.Date,
                        RoomId = p.PcroomtbId
                    })
                    .Select(g => new {
                        g.Key.Date,
                        g.Key.RoomId,
                        SumUsedPc = g.Sum(x => x.UsedPc),
                        SumPrice = g.Sum(x => x.Price)
                    })
                    .ToListAsync();

                // 3) 전체 날짜 리스트 생성
                var allDates = Enumerable
                    .Range(0, (endDate - startDate).Days + 1)
                    .Select(d => startDate.AddDays(d))
                    .ToList();

                // 4) 날짜×PC방별로 flat 레코드 생성 (PcId 포함)
                var flat = allDates
                    .SelectMany(date => pcList.Select(pc =>
                    {
                        var stat = stats.FirstOrDefault(s => s.RoomId == pc.PcId && s.Date == date);
                        if (stat == null)
                            return null;

                        double usedPc = stat.SumUsedPc / 48.0;
                        double avgRate = pc.SeatNumber == 0
                            ? 0
                            : usedPc / pc.SeatNumber * 100.0;
                        double pcPrice = stat.SumPrice;
                        double foodPrice = Math.Round(pcPrice * ((100.0 - pc.PricePercent) / pc.PricePercent));
                        double totalPrice = pcPrice + foodPrice;

                        var data = new analyzeData
                        {
                            analyzeDT = date.ToString("yyyy-MM-dd"),
                            countryId = pc.CountryId,
                            countryName = pc.CountryName,
                            cityId = pc.CityId,
                            cityName = pc.CityName,
                            townId = pc.TownId,
                            townName = pc.TownName,
                            usedPc = usedPc,
                            averageRate = avgRate,
                            pcPrice = pcPrice,
                            foodPrice = foodPrice,
                            totalPrice = totalPrice,
                            seatNumber = pc.SeatNumber,
                            pricePercent = pc.PricePercent + "%"
                        };

                        return new { pc.PcId, pc.PcName, Data = data };
                    }))
                    .Where(x => x != null)
                    .ToList()!;

                // 5) PC방별로 그룹핑하여 XlsxDTO 생성 (pcId 채워줌)
                var result = flat
                    .GroupBy(x => new { x.PcId, x.PcName })
                    .Select(g => new XlsxDTO
                    {
                        pcId = g.Key.PcId,
                        pcName = g.Key.PcName,
                        datas = g.Select(x => x.Data).ToList()
                    })
                    .ToList();

                return result;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }
    }
}
