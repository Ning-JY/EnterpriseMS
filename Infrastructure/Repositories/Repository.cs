using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Entities.Info;

namespace EnterpriseMS.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;
    public Repository(AppDbContext db) { _db = db; _set = db.Set<T>(); }

    public async Task<T?> GetByIdAsync(long id)
        => await _set.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null
            ? await _set.AsNoTracking().ToListAsync()
            : await _set.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<PagedResult<T>> GetPagedAsync(int page, int size,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = true)
    {
        var q = _set.AsNoTracking();
        if (predicate != null) q = q.Where(predicate);
        var total = await q.CountAsync();
        q = orderBy != null
            ? (descending ? q.OrderByDescending(orderBy) : q.OrderBy(orderBy))
            : q.OrderByDescending(e => e.CreatedAt);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<T> { Items = items, Total = total, Page = page, PageSize = size };
    }

    public async Task AddAsync(T entity)      => await _set.AddAsync(entity);
    public async Task AddRangeAsync(IEnumerable<T> entities) => await _set.AddRangeAsync(entities);
    public void Update(T entity)              => _set.Update(entity);
    public void SoftDelete(T entity)          { entity.IsDeleted = true; _set.Update(entity); }
    public IQueryable<T> Query(bool noTracking = true)
        => noTracking ? _set.AsNoTracking() : _set.AsQueryable();
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => _set.AnyAsync(predicate);
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? _set.CountAsync() : _set.CountAsync(predicate);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;

    // 字段缓存：同一请求内复用仓储实例，避免重复创建开销
    private IRepository<SysUser>?             _users;
    private IRepository<SysRole>?             _roles;
    private IRepository<SysMenu>?             _menus;
    private IRepository<SysDept>?             _depts;
    private IRepository<SysPost>?             _posts;
    private IRepository<SysDictType>?         _dictTypes;
    private IRepository<SysDictData>?         _dictDatas;
    private IRepository<Employee>?            _employees;
    private IRepository<EmployeeContract>?    _contracts;
    private IRepository<EmployeeCertificate>? _certs;
    private IRepository<Project>?             _projects;
    private IRepository<ProjectMember>?       _projMembers;
    private IRepository<ProjectMilestone>?    _milestones;
    private IRepository<ProjectAcceptance>?   _acceptances;
    private IRepository<ProjectOperLog>?      _projLogs;
    private IRepository<ProjectContract>?     _projContracts;
    private IRepository<ProjectInvoice>?      _projInvoices;
    private IRepository<ProjectFile>?         _projFiles;
    private IRepository<BudgetTask>?          _budgetTasks;
    private IRepository<BudgetSection>?       _budgetSections;
    private IRepository<ReviewOpinion>?       _reviewOpinions;
    private IRepository<InfoArticle>?         _infoArticles;
    private IRepository<InfoCategory>?        _infoCategories;
    private IRepository<KbFile>?              _kbFiles;
    private IRepository<KbCategory>?          _kbCategories;

    public UnitOfWork(AppDbContext db) => _db = db;

    public IRepository<SysUser>             Users          => _users          ??= new Repository<SysUser>(_db);
    public IRepository<SysRole>             Roles          => _roles          ??= new Repository<SysRole>(_db);
    public IRepository<SysMenu>             Menus          => _menus          ??= new Repository<SysMenu>(_db);
    public IRepository<SysDept>             Depts          => _depts          ??= new Repository<SysDept>(_db);
    public IRepository<SysPost>             Posts          => _posts          ??= new Repository<SysPost>(_db);
    public IRepository<SysDictType>         DictTypes      => _dictTypes      ??= new Repository<SysDictType>(_db);
    public IRepository<SysDictData>         DictDatas      => _dictDatas      ??= new Repository<SysDictData>(_db);
    public IRepository<Employee>            Employees      => _employees      ??= new Repository<Employee>(_db);
    public IRepository<EmployeeContract>    Contracts      => _contracts      ??= new Repository<EmployeeContract>(_db);
    public IRepository<EmployeeCertificate> Certificates   => _certs          ??= new Repository<EmployeeCertificate>(_db);
    public IRepository<Project>             Projects       => _projects       ??= new Repository<Project>(_db);
    public IRepository<ProjectMember>       ProjMembers    => _projMembers    ??= new Repository<ProjectMember>(_db);
    public IRepository<ProjectMilestone>    Milestones     => _milestones     ??= new Repository<ProjectMilestone>(_db);
    public IRepository<ProjectAcceptance>   Acceptances    => _acceptances    ??= new Repository<ProjectAcceptance>(_db);
    public IRepository<ProjectOperLog>      ProjLogs       => _projLogs       ??= new Repository<ProjectOperLog>(_db);
    public IRepository<ProjectContract>     ProjContracts  => _projContracts  ??= new Repository<ProjectContract>(_db);
    public IRepository<ProjectInvoice>      ProjInvoices   => _projInvoices   ??= new Repository<ProjectInvoice>(_db);
    public IRepository<ProjectFile>         ProjFiles      => _projFiles      ??= new Repository<ProjectFile>(_db);
    public IRepository<BudgetTask>          BudgetTasks    => _budgetTasks    ??= new Repository<BudgetTask>(_db);
    public IRepository<BudgetSection>       BudgetSections => _budgetSections ??= new Repository<BudgetSection>(_db);
    public IRepository<ReviewOpinion>       ReviewOpinions => _reviewOpinions ??= new Repository<ReviewOpinion>(_db);
    public IRepository<InfoArticle>         InfoArticles   => _infoArticles   ??= new Repository<InfoArticle>(_db);
    public IRepository<InfoCategory>        InfoCategories => _infoCategories ??= new Repository<InfoCategory>(_db);
    public IRepository<KbFile>              KbFiles        => _kbFiles        ??= new Repository<KbFile>(_db);
    public IRepository<KbCategory>          KbCategories   => _kbCategories   ??= new Repository<KbCategory>(_db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
    public async Task BeginTransactionAsync()
        => _tx = await _db.Database.BeginTransactionAsync();
    public async Task CommitAsync()
    { await _db.SaveChangesAsync(); if (_tx != null) await _tx.CommitAsync(); }
    public async Task RollbackAsync()
    { if (_tx != null) await _tx.RollbackAsync(); }
    public void Dispose() => _tx?.Dispose();
}
