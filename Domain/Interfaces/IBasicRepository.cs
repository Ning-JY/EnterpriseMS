using System.Linq.Expressions;

namespace EnterpriseMS.Domain.Interfaces;

/// <summary>
/// 非 BaseEntity 仓储接口 —— 用于 SysUserRole / SysRoleMenu 等联合表。
/// 这些表没有 Id/IsDeleted 等 BaseEntity 字段，无法使用 IRepository&lt;T&gt;。
/// </summary>
public interface IBasicRepository<T> where T : class
{
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void RemoveRange(IEnumerable<T> entities);
    IQueryable<T> Query();
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
