﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
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
                    item.PcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.TotalCount = analysis.Pcroom.Seatnumber; // 총대수
                    if (analysis.LatestGroup != null && analysis.LatestGroup.Items != null)
                    {
                        foreach (var temp in analysis.LatestGroup.Items)
                        {
                            item.Count = temp.UsedPc;
                        }
                    }
                    else
                    {
                        // 분석 결과가 없는것
                        item.Count = 0;
                    }

                    
                    item.Rate = ((float)item.Count / item.TotalCount) * 100;
                    item.ReturnRate = $"{item.Count}/{item.TotalCount} ({((double)item.Count / item.TotalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.Rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.BestName = best.PcRoomName; // 가장높은 매장명
                AnalysisData.AnalysisDate = DateTime.Now;
                AnalysisData.Datas = resultData;


                return AnalysisData;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 하루 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AnalysisDataDTO?> GetTodayDataAnalysis(DateTime TargetDate)
        {
            try
            {
                // 오늘날짜는 조회안됨.
                DateTime Today = DateTime.Today; // 오늘 날짜 (00:00:00 기준)

                // 1. 해당 날짜의 PinglogTbs 데이터를 가져옴
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value.Year == TargetDate.Year &&
                                m.CreateDt.Value.Month == TargetDate.Month &&
                                m.CreateDt.Value.Day == TargetDate.Day &&
                                m.CreateDt.Value.Date != Today)
                    .ToListAsync();

                // 2. 30분 단위로 그룹핑하면서 각 그룹별 UsedPc 합계 계산
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
                        PcroomId = g.Key.PcroomId,
                        GroupTime = g.Key.GroupTime,
                        SumUsedPc = g.Sum(x => x.UsedPc)  // 해당 그룹의 UsedPc 합계
                    })
                    .ToList();

                // 3. PC룸별로 모든 그룹의 UsedPc 합계를 다시 계산
                var groupedSumByRoom = groupedData
                    .GroupBy(x => x.PcroomId)
                    .Select(g => new {
                        PcroomId = g.Key,
                        TotalUsedPc = g.Sum(x => x.SumUsedPc)
                    })
                    .ToList();

                // 4. PC룸 정보와 왼쪽 조인 (해당 날짜의 로그가 없는 PC룸도 포함)
                var pcroomtb = await context.PcroomTbs
                    .Where(m => m.DelYn != true)
                    .ToListAsync();

                var finalResult = from room in pcroomtb
                                  join gs in groupedSumByRoom
                                      on room.Pid equals gs.PcroomId into gsJoin
                                  from gs in gsJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      Pcroom = room,
                                      TotalUsedPc = gs != null ? gs.TotalUsedPc : 0
                                  };

                List<ResultData> resultData = new List<ResultData>();
                foreach (var analysis in finalResult)
                {
                    var item = new ResultData();
                    item.PcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.TotalCount = analysis.Pcroom.Seatnumber; // 총대수
                    item.Count = analysis.TotalUsedPc; // 선택된 날짜의 사용PC대수
                    item.Rate = ((float)item.Count / item.TotalCount) * 100;
                    item.ReturnRate = $"{item.Count}/{item.TotalCount} ({((double)item.Count / item.TotalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.Rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.BestName = best.PcRoomName; // 가장높은 매장명
                AnalysisData.AnalysisDate = DateTime.Now;
                AnalysisData.Datas = resultData;


                return AnalysisData;

            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 주간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AnalysisDataDTO?> GetWeeksDataAnalysis(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                DateTime Today = DateTime.Today; // 오늘 날짜 (00:00:00 기준)

                // 1. 해당 날짜의 PinglogTbs 데이터를 가져옴
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value >= StartDate &&
                                m.CreateDt.Value <= EndDate && 
                                m.CreateDt.Value.Date != Today) // 시간 부분을 제거한 날짜 (년,월,일)을 반환하기때문에 Date만 비교를 하면됨.
                    .ToListAsync();

                // 2. 30분 단위로 그룹핑하면서 각 그룹별 UsedPc 합계 계산
                var groupedData = data
                    .GroupBy(m => new
                    {
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
                        PcroomId = g.Key.PcroomId,
                        GroupTime = g.Key.GroupTime,
                        SumUsedPc = g.Sum(x => x.UsedPc)  // 해당 그룹의 UsedPc 합계
                    })
                    .ToList();

                // 3. PC룸별로 모든 그룹의 UsedPc 합계를 다시 계산
                var groupedSumByRoom = groupedData
                    .GroupBy(x => x.PcroomId)
                    .Select(g => new
                    {
                        PcroomId = g.Key,
                        TotalUsedPc = g.Sum(x => x.SumUsedPc)
                    })
                    .ToList();

                // 4. PC룸 정보와 왼쪽 조인 (해당 날짜의 로그가 없는 PC룸도 포함)
                var pcroomtb = await context.PcroomTbs
                    .Where(m => m.DelYn != true)
                    .ToListAsync();

                var finalResult = from room in pcroomtb
                                  join gs in groupedSumByRoom
                                      on room.Pid equals gs.PcroomId into gsJoin
                                  from gs in gsJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      Pcroom = room,
                                      TotalUsedPc = gs != null ? gs.TotalUsedPc : 0
                                  };

                List<ResultData> resultData = new List<ResultData>();
                foreach (var analysis in finalResult)
                {
                    var item = new ResultData();
                    item.PcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.TotalCount = analysis.Pcroom.Seatnumber; // 총대수
                    item.Count = analysis.TotalUsedPc; // 선택된 날짜의 사용PC대수
                    item.Rate = ((float)item.Count / item.TotalCount) * 100;
                    item.ReturnRate = $"{item.Count}/{item.TotalCount} ({((double)item.Count / item.TotalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.Rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.BestName = best.PcRoomName; // 가장높은 매장명
                AnalysisData.AnalysisDate = DateTime.Now;
                AnalysisData.Datas = resultData;


                return AnalysisData;
            }
            catch(Exception ex)
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
        public async Task<AnalysisDataDTO?> GetMonthDataAnalysis(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                DateTime Today = DateTime.Today; // 오늘 날짜 (00:00:00 기준)

                // 1. 해당 날짜의 PinglogTbs 데이터를 가져옴
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value >= StartDate &&
                                m.CreateDt.Value <= EndDate &&
                                 m.CreateDt.Value.Date != Today)
                    .ToListAsync();

                // 2. 30분 단위로 그룹핑하면서 각 그룹별 UsedPc 합계 계산
                var groupedData = data
                    .GroupBy(m => new
                    {
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
                        PcroomId = g.Key.PcroomId,
                        GroupTime = g.Key.GroupTime,
                        SumUsedPc = g.Sum(x => x.UsedPc)  // 해당 그룹의 UsedPc 합계
                    })
                    .ToList();

                // 3. PC룸별로 모든 그룹의 UsedPc 합계를 다시 계산
                var groupedSumByRoom = groupedData
                    .GroupBy(x => x.PcroomId)
                    .Select(g => new
                    {
                        PcroomId = g.Key,
                        TotalUsedPc = g.Sum(x => x.SumUsedPc)
                    })
                    .ToList();

                // 4. PC룸 정보와 왼쪽 조인 (해당 날짜의 로그가 없는 PC룸도 포함)
                var pcroomtb = await context.PcroomTbs
                    .Where(m => m.DelYn != true)
                    .ToListAsync();

                var finalResult = from room in pcroomtb
                                  join gs in groupedSumByRoom
                                      on room.Pid equals gs.PcroomId into gsJoin
                                  from gs in gsJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      Pcroom = room,
                                      TotalUsedPc = gs != null ? gs.TotalUsedPc : 0
                                  };

                List<ResultData> resultData = new List<ResultData>();
                foreach (var analysis in finalResult)
                {
                    var item = new ResultData();
                    item.PcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.TotalCount = analysis.Pcroom.Seatnumber; // 총대수
                    item.Count = analysis.TotalUsedPc; // 선택된 날짜의 사용PC대수
                    item.Rate = ((float)item.Count / item.TotalCount) * 100;
                    item.ReturnRate = $"{item.Count}/{item.TotalCount} ({((double)item.Count / item.TotalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.Rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.BestName = best.PcRoomName; // 가장높은 매장명
                AnalysisData.AnalysisDate = DateTime.Now;
                AnalysisData.Datas = resultData;


                return AnalysisData;
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 년간 데이터 조회
        /// </summary>
        /// <param name="TargetDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AnalysisDataDTO?> GetYearDataAnalysis(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                DateTime Today = DateTime.Today; // 오늘 날짜 (00:00:00 기준)

                // 1. 해당 날짜의 PinglogTbs 데이터를 가져옴
                var data = await context.PinglogTbs
                    .Where(m => m.DelYn != true &&
                                m.CreateDt.Value >= StartDate &&
                                m.CreateDt.Value <= EndDate &&
                                 m.CreateDt.Value.Date != Today)
                    .ToListAsync();

                // 2. 30분 단위로 그룹핑하면서 각 그룹별 UsedPc 합계 계산
                var groupedData = data
                    .GroupBy(m => new
                    {
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
                        PcroomId = g.Key.PcroomId,
                        GroupTime = g.Key.GroupTime,
                        SumUsedPc = g.Sum(x => x.UsedPc)  // 해당 그룹의 UsedPc 합계
                    })
                    .ToList();

                // 3. PC룸별로 모든 그룹의 UsedPc 합계를 다시 계산
                var groupedSumByRoom = groupedData
                    .GroupBy(x => x.PcroomId)
                    .Select(g => new
                    {
                        PcroomId = g.Key,
                        TotalUsedPc = g.Sum(x => x.SumUsedPc)
                    })
                    .ToList();

                // 4. PC룸 정보와 왼쪽 조인 (해당 날짜의 로그가 없는 PC룸도 포함)
                var pcroomtb = await context.PcroomTbs
                    .Where(m => m.DelYn != true)
                    .ToListAsync();

                var finalResult = from room in pcroomtb
                                  join gs in groupedSumByRoom
                                      on room.Pid equals gs.PcroomId into gsJoin
                                  from gs in gsJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      Pcroom = room,
                                      TotalUsedPc = gs != null ? gs.TotalUsedPc : 0
                                  };

                List<ResultData> resultData = new List<ResultData>();
                foreach (var analysis in finalResult)
                {
                    var item = new ResultData();
                    item.PcRoomName = analysis.Pcroom.Name; // PC방 상호
                    item.TotalCount = analysis.Pcroom.Seatnumber; // 총대수
                    item.Count = analysis.TotalUsedPc; // 선택된 날짜의 사용PC대수
                    item.Rate = ((float)item.Count / item.TotalCount) * 100;
                    item.ReturnRate = $"{item.Count}/{item.TotalCount} ({((double)item.Count / item.TotalCount) * 100:F2}%)";

                    // 가동률 계산해야함.
                    resultData.Add(item);
                }

                var best = resultData.OrderByDescending(m => m.Rate).FirstOrDefault();
                AnalysisDataDTO AnalysisData = new AnalysisDataDTO();
                AnalysisData.BestName = best.PcRoomName; // 가장높은 매장명
                AnalysisData.AnalysisDate = DateTime.Now;
                AnalysisData.Datas = resultData;


                return AnalysisData;
            }
            catch (Exception ex)
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

        public async Task<List<PcroomTimeDataDto>> GetThisDayDataList()
        {
            try
            {
                // 대상 날짜를 하드코딩 (예: 2025년 3월 24일)
                var targetDate = new DateTime(2025, 3, 24);

                // 1. TimeTb에서 모든 시간(00:00 ~ 23:30)을 가져오기
                var allTimes = await context.TimeTbs
                    .OrderBy(t => t.Time)
                    .ToListAsync();

                // "HH:mm" 형식의 문자열 리스트 (예: "00:00", "00:30", ...)
                var allTimeStrings = allTimes
                    .Select(t => t.Time.HasValue ? t.Time.Value.ToString("HH:mm") : "")
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                // 2. PinglogTb, PcroomTb, TimeTb를 조인하여 대상 날짜의 데이터를 조회
                var pingLogs = await (
                    from p in context.PinglogTbs
                    join pc in context.PcroomTbs on p.PcroomtbId equals pc.Pid
                    join t in context.TimeTbs on p.TimetbId equals t.Pid
                    where p.DelYn != true
                          && p.CreateDt.HasValue
                          && p.CreateDt.Value.Date == targetDate.Date
                    select new
                    {
                        PcroomId = p.PcroomtbId,
                        PcroomName = pc.Name,
                        // 시간 문자열 ("HH:mm" 형식)
                        TimeString = t.Time.HasValue ? t.Time.Value.ToString("HH:mm") : "",
                        UsedPc = p.UsedPc
                    }
                ).ToListAsync();

                // 3. PC방별로 그룹화하고, 같은 시간대의 UsedPc를 합산하여 Dictionary로 구성
                var groupedData = pingLogs
                    .GroupBy(x => new { x.PcroomId, x.PcroomName })
                    .Select(g => new
                    {
                        PcroomId = g.Key.PcroomId,
                        PcroomName = g.Key.PcroomName,
                        // 각 시간대별 UsedPc 합산
                        TimeMap = g.GroupBy(x => x.TimeString)
                                   .ToDictionary(
                                       tg => tg.Key,
                                       tg => tg.Sum(x => x.UsedPc)
                                   )
                    })
                    .ToList();

                // 4. 모든 시간대를 기준으로, 없는 시간은 0으로 채워 DTO에 매핑
                var result = groupedData.Select(pc => new PcroomTimeDataDto
                {
                    PcroomId = pc.PcroomId,
                    PcroomName = pc.PcroomName,
                    AnalyList = allTimeStrings.Select(time => new ThisAnayzeList
                    {
                        Time = time,
                        Count = pc.TimeMap.ContainsKey(time) ? pc.TimeMap[time] : 0
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
    }
}
