using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;

namespace EnterpriseMS.Infrastructure.Repositories;

/// <summary>
/// 非 BaseEntity 仓储实现 —— 用于 SysUserRole / SysRoleMenu 等联合表。
/// </summary>
public class BasicRepository<T> : IBasicRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public BasicRepository(AppDbContext db) { _db = db; _set = db.Set<T>(); }

    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null
            ? await _set.AsNoTracking().ToListAsync()
            : await _set.AsNoTracking().Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public async Task AddRangeAsync(IEnumerable<T> entities) => await _set.AddRangeAsync(entities);
    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);
    public IQueryable<T> Query() => _set.AsQueryable();
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => _set.AnyAsync(predicate);
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? _set.CountAsync() : _set.CountAsync(predicate);
}
