using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using PegasusBackend.Helpers;

public static class PaginationHelper
{
    public static async Task<PaginatedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        string? sortBy = null,
        string sortOrder = "asc")
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        if (!string.IsNullOrEmpty(sortBy))
            query = query.OrderBy($"{sortBy} {sortOrder}");

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
            PageSize = pageSize
        };
    }
}
