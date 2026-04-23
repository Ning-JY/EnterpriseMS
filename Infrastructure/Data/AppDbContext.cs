using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Infrastructure.Data.Seeds;

namespace EnterpriseMS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 系统模块
    public DbSet<SysUser> SysUsers { get; set; }
    public DbSet<SysRole> SysRoles { get; set; }
    public DbSet<SysMenu> SysMenus { get; set; }
    public DbSet<SysDept> SysDepts { get; set; }
    public DbSet<SysPost> SysPosts { get; set; }
    public DbSet<SysUserRole> SysUserRoles { get; set; }
    public DbSet<SysRoleMenu> SysRoleMenus { get; set; }
    public DbSet<SysDictType> SysDictTypes { get; set; }
    public DbSet<SysDictData> SysDictDatas { get; set; }
    public DbSet<SysOperLog> SysOperLogs { get; set; }
    public DbSet<SysLoginLog> SysLoginLogs { get; set; }
    // HR
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeContract> Contracts { get; set; }
    public DbSet<EmployeeCertificate> Certificates { get; set; }
    // 项目
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjMembers { get; set; }
    public DbSet<ProjectMilestone> Milestones { get; set; }
    public DbSet<ProjectAcceptance> Acceptances { get; set; }
    public DbSet<ProjectOperLog> ProjLogs { get; set; }
    // 概预算
    public DbSet<BudgetTask> BudgetTasks { get; set; }
    public DbSet<BudgetSection> BudgetSections { get; set; }
    public DbSet<ReviewOpinion> ReviewOpinions { get; set; }
    // 项目扩展
    public DbSet<ProjectContract> ProjContracts { get; set; }
    public DbSet<ProjectInvoice> ProjInvoices { get; set; }
    public DbSet<ProjectFile> ProjFiles { get; set; }
    // 公开信息
    public DbSet<InfoArticle> InfoArticles { get; set; }
    public DbSet<InfoCategory> InfoCategories { get; set; }
    // 知识库
    public DbSet<KbFile> KbFiles { get; set; }
    public DbSet<KbCategory> KbCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // 复合主键
        mb.Entity<SysUserRole>().HasKey(e => new { e.UserId, e.RoleId });
        mb.Entity<SysRoleMenu>().HasKey(e => new { e.RoleId, e.MenuId });

        // 关系配置
        mb.Entity<SysUser>().HasOne(u => u.Dept).WithMany(d => d.Users)
            .HasForeignKey(u => u.DeptId).OnDelete(DeleteBehavior.SetNull);
        // SysUser -> Employee 一对一（可空唯一，NULL不参与唯一性检查）
        mb.Entity<SysUser>().HasIndex(u => u.EmployeeId).IsUnique();
        mb.Entity<SysUserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
        mb.Entity<SysUserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
        mb.Entity<SysRoleMenu>().HasOne(rm => rm.Role).WithMany(r => r.RoleMenus)
            .HasForeignKey(rm => rm.RoleId);
        mb.Entity<SysRoleMenu>().HasOne(rm => rm.Menu).WithMany(m => m.RoleMenus)
            .HasForeignKey(rm => rm.MenuId);
        mb.Entity<EmployeeContract>().HasOne(c => c.Employee).WithMany(e => e.Contracts)
            .HasForeignKey(c => c.EmployeeId);
        mb.Entity<EmployeeCertificate>().HasOne(c => c.Employee).WithMany(e => e.Certificates)
            .HasForeignKey(c => c.EmployeeId);
        mb.Entity<ProjectMember>().HasOne(m => m.Project).WithMany(p => p.Members)
            .HasForeignKey(m => m.ProjectId);
        mb.Entity<ProjectMember>().HasOne(m => m.Employee).WithMany()
            .HasForeignKey(m => m.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<ProjectMilestone>().HasOne(m => m.Project).WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId);
        mb.Entity<ProjectAcceptance>().HasOne(a => a.Project).WithMany(p => p.Acceptances)
            .HasForeignKey(a => a.ProjectId);
        mb.Entity<ProjectOperLog>().HasOne(l => l.Project).WithMany(p => p.OperLogs)
            .HasForeignKey(l => l.ProjectId);
        mb.Entity<BudgetSection>().HasOne(s => s.Task).WithMany(t => t.Sections)
            .HasForeignKey(s => s.TaskId);
        mb.Entity<ReviewOpinion>().HasOne(o => o.Task).WithMany(t => t.Opinions)
            .HasForeignKey(o => o.TaskId);
        mb.Entity<InfoArticle>().HasOne(a => a.Category).WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId);
        mb.Entity<KbFile>().HasOne(f => f.Category).WithMany(c => c.Files)
            .HasForeignKey(f => f.CategoryId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<ProjectContract>().HasOne(c => c.Project).WithMany(p => p.Contracts)
            .HasForeignKey(c => c.ProjectId);
        mb.Entity<ProjectInvoice>().HasOne(i => i.Project).WithMany(p => p.Invoices)
            .HasForeignKey(i => i.ProjectId);
        mb.Entity<ProjectFile>().HasOne(f => f.Project).WithMany(p => p.Files)
            .HasForeignKey(f => f.ProjectId);

        // 全局软删除过滤器
        foreach (var entityType in mb.Model.GetEntityTypes())
        {
            var prop = entityType.FindProperty("IsDeleted");
            if (prop == null) continue;
            var param = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Equal(
                Expression.Property(param, "IsDeleted"),
                Expression.Constant(false));
            mb.Entity(entityType.ClrType)
              .HasQueryFilter(Expression.Lambda(body, param));
        }

        SeedData(mb);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == 0)
                    entry.Entity.Id = SnowflakeId.Next();
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = DateTime.Now;
            }
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.Now;
        }
        return base.SaveChangesAsync(ct);
    }

    private static void SeedData(ModelBuilder mb)
    {
        SystemSeeds.Seed(mb);
        HrSeeds.Seed(mb);
        ProjectSeeds.Seed(mb);
        BudgetSeeds.Seed(mb);
        InfoSeeds.Seed(mb);
    }

    /* ── 原 SeedData 已拆分到 Seeds/ 目录下的独立种子类 ──
     * SystemSeeds  → 部门/岗位/角色/用户/字典/角色菜单
     * HrSeeds      → 员工/合同/证书
     * ProjectSeeds → 项目/成员/里程碑/验收
     * BudgetSeeds  → 概预算任务/分部
     * InfoSeeds    → 知识库分类/菜单
     */
    public async Task SeedAsync()
    {
        // 利用 EF Model 的 GetSeedData 读取 HasData 配置，批量插入
        // 每个实体类型独立处理，已存在则跳过（Upsert 语义）
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await Database.BeginTransactionAsync();
            try
            {
                // 按依赖顺序插入（先主表后子表）
                await SeedTableAsync<SysDept>();
                await SeedTableAsync<SysPost>();
                await SeedTableAsync<SysRole>();
                await SeedTableAsync<SysMenu>();
                await SeedTableAsync<SysUser>();
                await SeedTableAsync<SysUserRole>();
                await SeedTableAsync<SysRoleMenu>();
                await SeedTableAsync<SysDictType>();
                await SeedTableAsync<SysDictData>();
                await SeedTableAsync<KbCategory>();
                await SeedTableAsync<Employee>();
                await SeedTableAsync<EmployeeContract>();
                await SeedTableAsync<EmployeeCertificate>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Project.Project>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Project.ProjectMember>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Project.ProjectMilestone>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Project.ProjectAcceptance>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Budget.BudgetTask>();
                await SeedTableAsync<EnterpriseMS.Domain.Entities.Budget.BudgetSection>();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>供 DebugController 调用的公共包装</summary>
    public async Task SeedTablePublicAsync<T>() where T : class => await SeedTableAsync<T>();

    private async Task SeedTableAsync<T>() where T : class
    {
        var entityType = Model.FindEntityType(typeof(T));
        if (entityType == null) return;
        var seedData = entityType.GetSeedData().ToList();
        if (!seedData.Any()) return;

        var dbSet = Set<T>();
        var keyProps = entityType.FindPrimaryKey()?.Properties;
        if (keyProps == null) return;

        foreach (var seed in seedData)
        {
            // 构造实体实例
            var entity = Activator.CreateInstance<T>();
            foreach (var kv in seed)
            {
                var pi = typeof(T).GetProperty(kv.Key);
                if (pi == null || kv.Value == null) continue;
                try
                {
                    var targetType = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                    pi.SetValue(entity, Convert.ChangeType(kv.Value, targetType));
                }
                catch { /* 忽略转换失败的字段 */ }
            }

            // 检查是否已存在（按主键）
            var keyValues = keyProps.Select(k =>
            {
                var pi = typeof(T).GetProperty(k.Name);
                return pi?.GetValue(entity);
            }).ToArray();

            var existing = await dbSet.FindAsync(keyValues);
            if (existing == null)
                await dbSet.AddAsync(entity);
        }
        await SaveChangesAsync();
    }

}
