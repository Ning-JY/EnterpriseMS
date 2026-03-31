using System.Linq.Expressions;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);
    Task<PagedResult<T>> GetPagedAsync(int page, int size,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = true);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void SoftDelete(T entity);
    IQueryable<T> Query(bool noTracking = true);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
