using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Entities.Bid;
using EnterpriseMS.Infrastructure.Data.Seeds;
using Serilog;

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
    public DbSet<SysConfig> SysConfigs { get; set; }
    // HR
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeContract> Contracts { get; set; }
    public DbSet<EmployeeCertificate> Certificates { get; set; }
    public DbSet<EmployeeEducation> Educations { get; set; }
    public DbSet<EmployeeWorkExp> WorkExperiences { get; set; }
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
    // 投标管理
    public DbSet<BidProject> BidProjects { get; set; }
    public DbSet<BidRequirement> BidRequirements { get; set; }
    public DbSet<BidDocument> BidDocuments { get; set; }
    public DbSet<BidTemplate> BidTemplates { get; set; }
    // 通知中心
    public DbSet<SysNotification> Notifications { get; set; }
    public DbSet<SysNotificationRead> NotificationReads { get; set; }

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
        mb.Entity<EmployeeEducation>().HasOne(e => e.Employee).WithMany(emp => emp.EducationList)
            .HasForeignKey(e => e.EmployeeId);
        mb.Entity<EmployeeWorkExp>().HasOne(w => w.Employee).WithMany(emp => emp.WorkExperiences)
            .HasForeignKey(w => w.EmployeeId);
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

        // 投标管理
        mb.Entity<BidProject>().HasOne(e => e.Project).WithMany()
            .HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<BidRequirement>().HasOne(e => e.BidProject).WithMany(b => b.Requirements)
            .HasForeignKey(e => e.BidProjectId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<BidDocument>().HasOne(e => e.BidProject).WithMany(b => b.Documents)
            .HasForeignKey(e => e.BidProjectId).OnDelete(DeleteBehavior.Cascade);

        // 通知已读标记：同一用户对同一条通知只应有一条记录
        mb.Entity<SysNotificationRead>().HasIndex(r => new { r.UserId, r.NotificationId }).IsUnique();

        // 外键索引：为频繁查询的外键字段添加索引，提升查询性能
        mb.Entity<EmployeeContract>().HasIndex(c => c.EmployeeId);
        mb.Entity<EmployeeCertificate>().HasIndex(c => c.EmployeeId);
        mb.Entity<ProjectMember>().HasIndex(m => m.ProjectId);
        mb.Entity<ProjectMember>().HasIndex(m => m.EmployeeId);
        mb.Entity<ProjectMilestone>().HasIndex(m => m.ProjectId);
        mb.Entity<ProjectAcceptance>().HasIndex(a => a.ProjectId);
        mb.Entity<ProjectOperLog>().HasIndex(l => l.ProjectId);
        mb.Entity<ProjectContract>().HasIndex(c => c.ProjectId);
        mb.Entity<ProjectInvoice>().HasIndex(i => i.ProjectId);
        mb.Entity<ProjectFile>().HasIndex(f => f.ProjectId);
        mb.Entity<BidRequirement>().HasIndex(r => r.BidProjectId);
        mb.Entity<BidDocument>().HasIndex(d => d.BidProjectId);
        mb.Entity<BudgetSection>().HasIndex(s => s.TaskId);
        mb.Entity<ReviewOpinion>().HasIndex(o => o.TaskId);
        mb.Entity<InfoArticle>().HasIndex(a => a.CategoryId);
        mb.Entity<KbFile>().HasIndex(f => f.CategoryId);

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
                    entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }

    private static void SeedData(ModelBuilder mb)
    {
        SystemSeeds.Seed(mb);
        MenuSeeds.Seed(mb);
        DictSeeds.Seed(mb);
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
    /// <summary>供 DebugController 调用的公共包装</summary>
    public async Task SeedTablePublicAsync<T>() where T : class => await SeedTableAsync<T>();

    private async Task SeedTableAsync<T>() where T : class
    {
        // EF Core 9：HasData 种子配置仅存在于 design-time 模型（read-optimized 运行时模型不存储），
        // 故读取种子数据须走 IDesignTimeModel，否则会抛
        // "The requested configuration is not stored in the read-optimized model" 异常。
        var designTimeModel = this.GetService<IDesignTimeModel>().Model;
        var entityType = designTimeModel.FindEntityType(typeof(T));
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
                catch (Exception ex)
                {
                    Log.Warning(ex, "种子字段转换失败：{Entity}.{Property}", typeof(T).Name, pi?.Name);
                }
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
