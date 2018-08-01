using Forex.Models;
using Forex.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Forex.Services
{
    public static class DbService
    {
        private static ForexDbContext DbContext { get; set; }

        public static void Initialize()
        {
            DbContext = ForexDbContext.GetInstance();
        }

        public async static Task<DateTime?> GetNewestTimeAsync()
        {
            var item = await DbContext.RateItems
                .OrderByDescending(o => o.Time)
                .FirstOrDefaultAsync();

            return item?.Time;
        }

        public async static Task<PagedResult<RateItem>> GetDailyDetailsAsync(DateTime? date = null, int page = 1)
        {
            var from = (date ?? DateTime.Now).Date;
            var to = from.AddDays(1);

            return await DbContext.RateItems
                .AsNoTracking()
                .Where(o => o.Time >= from && o.Time < to)
                .OrderByDescending(o => o.Time)
                .ToPagedResultAsync(page);
        }

        public async static Task<List<RateSummary>> GetDailySummaryAsync(int maxDays)
        {
            return await DbContext.RateSummaries
                .AsNoTracking()
                .OrderByDescending(o => o.Date)
                .Take(maxDays)
                .ToListAsync();
        }

        public async static Task SyncRateItemsAsync(List<RateItem> items)
        {
            DbContext.RateItems.AddRange(items);
            await DbContext.SaveChangesAsync();

            var affectedDates = items.Select(o => o.Time.Date).Distinct().ToArray();
            foreach (var date in affectedDates)
            {
                var dateEnd = date.AddDays(1).AddMilliseconds(-1);

                var allDayItems = await DbContext.RateItems
                    .AsNoTracking()
                    .Where(o => o.Time >= date && o.Time < dateEnd)
                    .ToArrayAsync();

                var daySummary = await DbContext.RateSummaries.FirstOrDefaultAsync(o => o.Date == date);
                if (daySummary == null)
                {
                    daySummary = new RateSummary
                    {
                        CurrencyId = items.First().CurrencyId,
                        Date = date
                    };
                    DbContext.RateSummaries.Add(daySummary);
                }

                daySummary.MinRate = allDayItems.Min(o => o.Rate);
                daySummary.MaxRate = allDayItems.Max(o => o.Rate);

                await DbContext.SaveChangesAsync();
            }
        }

        public static void Cleanup()
        {
            if (DbContext != null)
            {
                DbContext.Dispose();
            }
        }
    }
}
