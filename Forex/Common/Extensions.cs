using Forex.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Forex
{
    public static class Extensions
    {
        #region PagedResult

        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageIndex, int pageSize = Configs.PAGE_SIZE)
        {
            return new PagedResult<T>
            {
                Items = source.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),
                Pager = new Pager
                {
                    CurrentPage = pageIndex,
                    ItemCount = source.Count(),
                    PageSize = pageSize
                }
            };
        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize = Configs.PAGE_SIZE)
        {
            return new PagedResult<T>
            {
                Items = await source.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(),
                Pager = new Pager
                {
                    CurrentPage = pageIndex,
                    ItemCount = await source.CountAsync(),
                    PageSize = pageSize
                }
            };
        }

        #endregion
    }
}
