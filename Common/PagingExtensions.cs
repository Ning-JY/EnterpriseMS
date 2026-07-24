using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Common;

/// <summary>
/// 统一分页扩展：消除各 Service / Controller 中手写 Count + Skip/Take 的重复样板。
/// 调用前请在 query 上先 OrderBy（EF Core 要求有序后才能 Skip/Take）。
/// </summary>
public static class PagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query, int page, int size)
    {
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<T> { Items = items, Total = total, Page = page, PageSize = size };
    }
}
