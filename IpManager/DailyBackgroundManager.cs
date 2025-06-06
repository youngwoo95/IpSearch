﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;

namespace IpManager
{
    public class DailyBackgroundManager : BackgroundService
    {
        private readonly IServiceScopeFactory ScopeFactory;
        private readonly ILoggerService LoggerService;

        public DailyBackgroundManager(IServiceScopeFactory _scopeFactory,
            ILoggerService _loggerservice)
        {
            this.LoggerService = _loggerservice;
            this.ScopeFactory = _scopeFactory;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("백그라운드 타이머 시작 / 간격 1일");

                while (!stoppingToken.IsCancellationRequested)
                {
                    // 다음 실행 시각 계산 (오늘 00:05가 지났으면 내일 00:05)
                    var now = DateTime.Now;
                    var next = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0);
                    if (now >= next)
                        next = next.AddDays(1);

                    var delay = next - now;
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (TaskCanceledException) { break; }

                    using (var scope = ScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IpanalyzeContext>();

                        // 전날을 구해야함.
                        DateTime today = DateTime.Today;
                        DateTime yesterday = today.AddDays(-1);
                       
                        var groupedData = await context.PinglogTbs
                         .Where(x => x.CreateDt >= yesterday && x.CreateDt < today)
                         .Join(
                             context.PcroomTbs.Where(p => p.PricePercent != 0),
                             log => log.PcroomtbId,
                             room => room.Pid,
                             (log, room) => new { log, room }
                         )
                         .GroupBy(x => x.log.PcroomtbId)
                         .Select(g => new
                         {
                             PcroomtbId = g.Key,
                             TowntbId = g.Max(x => x.room.TowntbId),   // ← 여기
                             TodayRate = g.Sum(x => x.log.PcRate) / 48.0,
                             TodaySales = g.Sum(x => x.log.Price),

                             // 그룹 내에서 PricePercent의 최대(혹은 최소)를 뽑아서 percent로 사용
                             Percent = g.Max(x => x.room.PricePercent),

                             TodayFoodSales = g.Sum(x => x.log.Price)
                                              * ((100.0 - g.Max(x => x.room.PricePercent))
                                                 / g.Max(x => x.room.PricePercent)),

                             TotalSales = g.Sum(x => x.log.Price)
                                              * (1.0 + ((100.0 - g.Max(x => x.room.PricePercent))
                                                         / g.Max(x => x.room.PricePercent)))
                         })
                         .ToListAsync();



                        // 가동률 1위
                        var topRateGroup = groupedData
                        .OrderByDescending(m => m.TodayRate)
                        .FirstOrDefault();

                        // 매출 1위
                        var topSales = groupedData
                        .OrderByDescending(m => m.TotalSales)
                        .FirstOrDefault();

                        if (topSales == null || topRateGroup == null)
                        {
                            LoggerService.FileErrorMessage("집계 결과가 없습니다.");
                            continue;
                        }

                        var toptown = await context.TownTbs.FirstOrDefaultAsync(m => m.Pid == topSales.TowntbId);
                        if (toptown == null)
                        {
                            LoggerService.FileErrorMessage($"TownTb[{topSales.TowntbId}] 미존재");
                            continue;
                        }

                        var analyzetb = new AnalyzeTb
                        {
                            TowntbId = toptown.Pid, // 매출 1위 동네 인덱스
                            TopSalesPcroomtbId = topSales.PcroomtbId, // 매출 1위 PC방 인덱스
                            TopOpratePcroomtbId = topRateGroup.PcroomtbId, // 가동률 1위 PC방 인덱스
                            CreateDt = DateTime.Now
                        };

                        await context.AnalyzeTbs.AddAsync(analyzetb);
                        await context.SaveChangesAsync().ConfigureAwait(false); // 저장
                        
                        LoggerService.FileLogMessage($"[DailyBackground] {DateTime.Now}: 전날 요약 저장 완료");
                    }
                }

            }catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
            }
        }
    }
}
