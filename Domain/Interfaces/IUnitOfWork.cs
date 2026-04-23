using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Entities.Info;

namespace EnterpriseMS.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // 系统
    IRepository<SysUser>     Users       { get; }
    IRepository<SysRole>     Roles       { get; }
    IRepository<SysMenu>     Menus       { get; }
    IRepository<SysDept>     Depts       { get; }
    IRepository<SysPost>     Posts       { get; }
    IRepository<SysDictType> DictTypes   { get; }
    IRepository<SysDictData> DictDatas   { get; }
    // HR
    IRepository<Employee>             Employees    { get; }
    IRepository<EmployeeContract>     Contracts    { get; }
    IRepository<EmployeeCertificate>  Certificates { get; }
    // 项目
    IRepository<Project>           Projects      { get; }
    IRepository<ProjectMember>     ProjMembers   { get; }
    IRepository<ProjectMilestone>  Milestones    { get; }
    IRepository<ProjectAcceptance> Acceptances   { get; }
    IRepository<ProjectOperLog>    ProjLogs      { get; }
    IRepository<ProjectContract>   ProjContracts { get; }
    IRepository<ProjectInvoice>    ProjInvoices  { get; }
    IRepository<ProjectFile>       ProjFiles     { get; }
    // 概预算
    IRepository<BudgetTask>    BudgetTasks    { get; }
    IRepository<BudgetSection> BudgetSections { get; }
    IRepository<ReviewOpinion> ReviewOpinions { get; }
    // 公开信息
    IRepository<InfoArticle>  InfoArticles   { get; }
    IRepository<InfoCategory> InfoCategories { get; }
    // 知识库
    IRepository<KbFile>     KbFiles      { get; }
    IRepository<KbCategory> KbCategories { get; }

    // 联合表（非 BaseEntity，使用 BasicRepository）
    IBasicRepository<SysUserRole> UserRoles { get; }
    IBasicRepository<SysRoleMenu> RoleMenus { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
