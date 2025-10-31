using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;

namespace PegasusBackend.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PaginatedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            string? sortBy = null,
            string sortOrder = "asc",
            string? thenSortBy = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            try
            {
                if (!string.IsNullOrEmpty(sortBy))
                {
                    var orderString = $"{sortBy} {sortOrder}";
                    if (!string.IsNullOrEmpty(thenSortBy))
                        orderString += $", {thenSortBy} asc";

                    query = query.OrderBy(orderString);
                }
            }
            catch (ParseException)
            {
                var firstProp = typeof(T).GetProperties().FirstOrDefault()?.Name ?? "Id";
                query = query.OrderBy($"{firstProp} asc");
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                CurrentCount = items.Count
            };
        }
    }
}