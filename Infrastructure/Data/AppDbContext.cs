using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Entities.Info;

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
        var dt = new DateTime(2026, 1, 1);

        // ── 部门 ──────────────────────────────────────────────
        mb.Entity<SysDept>().HasData(
            new SysDept { Id = 1, DeptName = "总公司", ParentId = 0, Ancestors = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 2, DeptName = "工程咨询事业部", ParentId = 1, Ancestors = "0,1", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 3, DeptName = "交通和土地利用事业部", ParentId = 1, Ancestors = "0,1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 4, DeptName = "城市设计事业部", ParentId = 1, Ancestors = "0,1", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 5, DeptName = "区域和产业经济事业部", ParentId = 1, Ancestors = "0,1", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 6, DeptName = "生产经营部", ParentId = 1, Ancestors = "0,1", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 岗位 ──────────────────────────────────────────────
        mb.Entity<SysPost>().HasData(
            new SysPost { Id = 1, PostName = "总经理", PostCode = "ceo", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 2, PostName = "副总经理", PostCode = "vceo", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 3, PostName = "项目负责人", PostCode = "pm", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 4, PostName = "技术负责人", PostCode = "tech", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 5, PostName = "商务负责人", PostCode = "business", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 6, PostName = "高级工程师", PostCode = "senior", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 7, PostName = "工程师", PostCode = "engineer", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 8, PostName = "助理工程师", PostCode = "assist", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 9, PostName = "行政专员", PostCode = "admin", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 角色 ──────────────────────────────────────────────
        mb.Entity<SysRole>().HasData(
            new SysRole { Id = 1, RoleName = "超级管理员", RoleCode = "superadmin", DataScope = 1, Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 2, RoleName = "管理员", RoleCode = "admin", DataScope = 1, Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 3, RoleName = "项目经理", RoleCode = "pm", DataScope = 3, Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "可查看本部门及子部门全部项目" },
            new SysRole { Id = 4, RoleName = "工程师", RoleCode = "engineer", DataScope = 4, Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "只能查看本人参与的项目" },
            new SysRole { Id = 5, RoleName = "财务", RoleCode = "finance", DataScope = 2, Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 6, RoleName = "只读", RoleCode = "readonly", DataScope = 1, Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "只有查看权限，无增删改" }
        );

        // ── 用户 ──────────────────────────────────────────────
        mb.Entity<SysUser>().HasData(
            new SysUser { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "超级管理员", DeptId = 1, PostId = 1, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 2, Username = "ningjinyuan", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "甯金元", DeptId = 2, PostId = 3, Status = 1, EmployeeId = 101, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 3, Username = "caolijun", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "曹丽君", DeptId = 2, PostId = 4, Status = 1, EmployeeId = 102, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 4, Username = "liurunze", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "刘润泽", DeptId = 3, PostId = 3, Status = 1, EmployeeId = 103, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 5, Username = "wangshuaiwei", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "王帅伟", DeptId = 4, PostId = 6, Status = 1, EmployeeId = 104, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 6, Username = "yangtong", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "杨通", DeptId = 2, PostId = 7, Status = 1, EmployeeId = 105, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 用户角色 ──────────────────────────────────────────
        mb.Entity<SysUserRole>().HasData(
            new SysUserRole { UserId = 1, RoleId = 1 },
            new SysUserRole { UserId = 2, RoleId = 3 },
            new SysUserRole { UserId = 3, RoleId = 4 },
            new SysUserRole { UserId = 4, RoleId = 3 },
            new SysUserRole { UserId = 5, RoleId = 4 },
            new SysUserRole { UserId = 6, RoleId = 4 }
        );

        // ── 员工档案 ──────────────────────────────────────────
        mb.Entity<Employee>().HasData(
            new Employee { Id = 101, EmpNo = "EMP20230001", RealName = "甯金元", Gender = 1, Phone = "13800000001", Email = "zhangsan@company.com", DeptId = 2, PostId = 3, Status = 1, EntryDate = new DateTime(2020, 3, 1), FormalDate = new DateTime(2020, 6, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 102, EmpNo = "EMP20230002", RealName = "曹丽君", Gender = 2, Phone = "13800000002", Email = "lisi@company.com", DeptId = 2, PostId = 4, Status = 1, EntryDate = new DateTime(2019, 7, 1), FormalDate = new DateTime(2019, 10, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 103, EmpNo = "EMP20230003", RealName = "刘润泽", Gender = 1, Phone = "13800000003", Email = "wangwu@company.com", DeptId = 3, PostId = 3, Status = 1, EntryDate = new DateTime(2021, 1, 1), FormalDate = new DateTime(2021, 4, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 104, EmpNo = "EMP20230004", RealName = "王帅伟", Gender = 1, Phone = "13800000004", Email = "zhaoliu@company.com", DeptId = 4, PostId = 6, Status = 1, EntryDate = new DateTime(2018, 5, 1), FormalDate = new DateTime(2018, 8, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 105, EmpNo = "EMP20230005", RealName = "杨通", Gender = 1, Phone = "13800000005", Email = "sunqi@company.com", DeptId = 2, PostId = 7, Status = 1, EntryDate = new DateTime(2022, 3, 1), FormalDate = new DateTime(2022, 6, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 106, EmpNo = "EMP20230006", RealName = "郭家松", Gender = 1, Phone = "13800000006", Email = "zhouba@company.com", DeptId = 3, PostId = 7, Status = 1, EntryDate = new DateTime(2021, 9, 1), FormalDate = new DateTime(2021, 12, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 107, EmpNo = "EMP20230007", RealName = "陈俊童", Gender = 1, Phone = "13800000007", Email = "wujiu@company.com", DeptId = 5, PostId = 4, Status = 1, EntryDate = new DateTime(2020, 6, 1), FormalDate = new DateTime(2020, 9, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 108, EmpNo = "EMP20230008", RealName = "舒影", Gender = 2, Phone = "13800000008", Email = "zhengshi@company.com", DeptId = 4, PostId = 3, Status = 1, EntryDate = new DateTime(2017, 4, 1), FormalDate = new DateTime(2017, 7, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 109, EmpNo = "EMP20230009", RealName = "肖玲", Gender = 2, Phone = "13800000009", Email = "chenxm@company.com", DeptId = 2, PostId = 8, Status = 0, EntryDate = new DateTime(2026, 1, 1), ProbationEndDate = new DateTime(2026, 4, 1), CreatedAt = dt, CreatedBy = "system" },
            new Employee { Id = 110, EmpNo = "EMP20230010", RealName = "魏利", Gender = 2, Phone = "13800000010", Email = "linxy@company.com", DeptId = 6, PostId = 9, Status = 1, EntryDate = new DateTime(2023, 5, 1), FormalDate = new DateTime(2023, 8, 1), CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 用户与员工绑定（一对一）──────────────────────────
        // 通过直接更新HasData里的EmployeeId实现，需配合SysUser
        // 注意：EF Core HasData不支持更新已存在记录，这里在User种子里直接写EmployeeId

        // ── 员工合同 ──────────────────────────────────────────
        mb.Entity<EmployeeContract>().HasData(
            new EmployeeContract { Id = 1001, EmployeeId = 101, ContractNo = "HT2020-001", ContractType = "固定期限", StartDate = new DateTime(2020, 6, 1), EndDate = new DateTime(2023, 5, 31), SignDate = new DateTime(2020, 6, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1002, EmployeeId = 101, ContractNo = "HT2023-001", ContractType = "固定期限", StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2026, 5, 31), SignDate = new DateTime(2023, 6, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1003, EmployeeId = 102, ContractNo = "HT2019-001", ContractType = "固定期限", StartDate = new DateTime(2019, 10, 1), EndDate = new DateTime(2022, 9, 30), SignDate = new DateTime(2019, 10, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1004, EmployeeId = 102, ContractNo = "HT2022-001", ContractType = "无固定期限", StartDate = new DateTime(2022, 10, 1), EndDate = new DateTime(2099, 12, 31), SignDate = new DateTime(2022, 10, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1005, EmployeeId = 103, ContractNo = "HT2021-001", ContractType = "固定期限", StartDate = new DateTime(2021, 4, 1), EndDate = new DateTime(2024, 3, 31), SignDate = new DateTime(2021, 4, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1006, EmployeeId = 103, ContractNo = "HT2024-001", ContractType = "固定期限", StartDate = new DateTime(2024, 4, 1), EndDate = new DateTime(2027, 3, 31), SignDate = new DateTime(2024, 4, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1007, EmployeeId = 104, ContractNo = "HT2018-001", ContractType = "无固定期限", StartDate = new DateTime(2018, 8, 1), EndDate = new DateTime(2099, 12, 31), SignDate = new DateTime(2018, 8, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1008, EmployeeId = 105, ContractNo = "HT2022-002", ContractType = "固定期限", StartDate = new DateTime(2022, 6, 1), EndDate = new DateTime(2025, 5, 31), SignDate = new DateTime(2022, 6, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1009, EmployeeId = 106, ContractNo = "HT2021-002", ContractType = "固定期限", StartDate = new DateTime(2021, 12, 1), EndDate = new DateTime(2024, 11, 30), SignDate = new DateTime(2021, 12, 1), Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeContract { Id = 1010, EmployeeId = 106, ContractNo = "HT2024-002", ContractType = "固定期限", StartDate = new DateTime(2024, 12, 1), EndDate = new DateTime(2027, 11, 30), SignDate = new DateTime(2024, 12, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 员工证书 ──────────────────────────────────────────
        mb.Entity<EmployeeCertificate>().HasData(
            new EmployeeCertificate { Id = 2001, EmployeeId = 101, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2019ABCD0001", IssueOrg = "住建部", IssueDate = new DateTime(2019, 11, 1), ExpireDate = new DateTime(2025, 10, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2002, EmployeeId = 102, CertName = "一级造价工程师", CertType = "造价工程师", CertNo = "2018ABCD0002", IssueOrg = "住建部", IssueDate = new DateTime(2018, 6, 1), ExpireDate = new DateTime(2026, 5, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2003, EmployeeId = 103, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2021ABCD0003", IssueOrg = "住建部", IssueDate = new DateTime(2021, 11, 1), ExpireDate = new DateTime(2025, 10, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2004, EmployeeId = 104, CertName = "二级建造师", CertType = "建造师", CertNo = "2016ABCD0004", IssueOrg = "四川省住建厅", IssueDate = new DateTime(2016, 9, 1), ExpireDate = null, Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2005, EmployeeId = 104, CertName = "注册建筑师", CertType = "注册建筑师", CertNo = "2020ABCD0005", IssueOrg = "住建部", IssueDate = new DateTime(2020, 3, 1), ExpireDate = new DateTime(2026, 2, 28), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2006, EmployeeId = 105, CertName = "助理工程师职称", CertType = "职称证书", CertNo = "2022ABCD0006", IssueOrg = "四川省人社厅", IssueDate = new DateTime(2022, 8, 1), ExpireDate = null, Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2007, EmployeeId = 107, CertName = "一级造价工程师", CertType = "造价工程师", CertNo = "2019ABCD0007", IssueOrg = "住建部", IssueDate = new DateTime(2019, 6, 1), ExpireDate = new DateTime(2026, 2, 28), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new EmployeeCertificate { Id = 2008, EmployeeId = 108, CertName = "注册城乡规划师", CertType = "注册规划师", CertNo = "2016ABCD0008", IssueOrg = "住建部", IssueDate = new DateTime(2016, 11, 1), ExpireDate = new DateTime(2026, 5, 31), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 字典类型 ──────────────────────────────────────────
        mb.Entity<SysDictType>().HasData(
            new SysDictType { Id = 1, DictName = "业务类型", DictType = "biz_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 2, DictName = "采购方式", DictType = "procurement_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 3, DictName = "合同类型", DictType = "contract_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 4, DictName = "证书类型", DictType = "cert_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 5, DictName = "里程碑类型", DictType = "milestone_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 6, DictName = "概预算任务类型", DictType = "budget_task_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 7, DictName = "项目进度状态", DictType = "proj_status", Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 字典数据 ──────────────────────────────────────────
        mb.Entity<SysDictData>().HasData(
            // 业务类型
            new SysDictData { Id = 101, DictType = "biz_type", DictLabel = "可行性研究报告", DictValue = "可行性研究报告", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 102, DictType = "biz_type", DictLabel = "节能评估报告", DictValue = "节能评估报告", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 103, DictType = "biz_type", DictLabel = "稳评报告", DictValue = "稳评报告", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 104, DictType = "biz_type", DictLabel = "概算编制", DictValue = "概算编制", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 105, DictType = "biz_type", DictLabel = "预算编制", DictValue = "预算编制", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 106, DictType = "biz_type", DictLabel = "结算编制", DictValue = "结算编制", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 107, DictType = "biz_type", DictLabel = "概算评审", DictValue = "概算评审", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 108, DictType = "biz_type", DictLabel = "预算评审", DictValue = "预算评审", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 109, DictType = "biz_type", DictLabel = "结算评审", DictValue = "结算评审", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 110, DictType = "biz_type", DictLabel = "控制性详细规划", DictValue = "控制性详细规划", Sort = 10, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 111, DictType = "biz_type", DictLabel = "专项规划", DictValue = "专项规划", Sort = 11, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 112, DictType = "biz_type", DictLabel = "城市更新规划", DictValue = "城市更新规划", Sort = 12, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 113, DictType = "biz_type", DictLabel = "施工图设计", DictValue = "施工图设计", Sort = 13, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 114, DictType = "biz_type", DictLabel = "战略咨询", DictValue = "战略咨询", Sort = 14, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 115, DictType = "biz_type", DictLabel = "施工阶段全过程管控", DictValue = "施工阶段全过程管控", Sort = 15, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 采购方式
            new SysDictData { Id = 201, DictType = "procurement_type", DictLabel = "竞争性磋商", DictValue = "竞争性磋商", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 202, DictType = "procurement_type", DictLabel = "询价", DictValue = "询价", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 203, DictType = "procurement_type", DictLabel = "公开招标", DictValue = "公开招标", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 204, DictType = "procurement_type", DictLabel = "邀请招标", DictValue = "邀请招标", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 205, DictType = "procurement_type", DictLabel = "公开招选", DictValue = "公开招选", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 206, DictType = "procurement_type", DictLabel = "框架协议采购", DictValue = "框架协议采购", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 207, DictType = "procurement_type", DictLabel = "单一来源", DictValue = "单一来源", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 合同类型
            new SysDictData { Id = 301, DictType = "contract_type", DictLabel = "固定期限", DictValue = "固定期限", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 302, DictType = "contract_type", DictLabel = "无固定期限", DictValue = "无固定期限", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 303, DictType = "contract_type", DictLabel = "劳务合同", DictValue = "劳务合同", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 304, DictType = "contract_type", DictLabel = "实习协议", DictValue = "实习协议", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 证书类型
            new SysDictData { Id = 401, DictType = "cert_type", DictLabel = "注册规划师", DictValue = "注册规划师", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 402, DictType = "cert_type", DictLabel = "造价工程师", DictValue = "造价工程师", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 403, DictType = "cert_type", DictLabel = "注册建筑师", DictValue = "注册建筑师", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 404, DictType = "cert_type", DictLabel = "注册工程师", DictValue = "注册工程师", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 405, DictType = "cert_type", DictLabel = "建造师", DictValue = "建造师", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 406, DictType = "cert_type", DictLabel = "职称证书", DictValue = "职称证书", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 407, DictType = "cert_type", DictLabel = "岗位证书", DictValue = "岗位证书", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 里程碑类型
            new SysDictData { Id = 501, DictType = "milestone_type", DictLabel = "资料收集", DictValue = "资料收集", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 502, DictType = "milestone_type", DictLabel = "现状调研", DictValue = "现状调研", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 503, DictType = "milestone_type", DictLabel = "方案设计", DictValue = "方案设计", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 504, DictType = "milestone_type", DictLabel = "内部评审", DictValue = "内部评审", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 505, DictType = "milestone_type", DictLabel = "专家评审", DictValue = "专家评审", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 506, DictType = "milestone_type", DictLabel = "报批上报", DictValue = "报批上报", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 507, DictType = "milestone_type", DictLabel = "成果交付", DictValue = "成果交付", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 508, DictType = "milestone_type", DictLabel = "回款", DictValue = "回款", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 概预算任务类型
            new SysDictData { Id = 601, DictType = "budget_task_type", DictLabel = "概算编制", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 602, DictType = "budget_task_type", DictLabel = "预算编制", DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 603, DictType = "budget_task_type", DictLabel = "结算编制", DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 604, DictType = "budget_task_type", DictLabel = "概算评审", DictValue = "3", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 605, DictType = "budget_task_type", DictLabel = "预算评审", DictValue = "4", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 606, DictType = "budget_task_type", DictLabel = "结算评审", DictValue = "5", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 项目进度状态
            new SysDictData { Id = 701, DictType = "proj_status", DictLabel = "前期商务", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 702, DictType = "proj_status", DictLabel = "预计启动", DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 703, DictType = "proj_status", DictLabel = "标书制作中", DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 704, DictType = "proj_status", DictLabel = "投标/磋商中", DictValue = "3", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 705, DictType = "proj_status", DictLabel = "已中标·签订合同中", DictValue = "4", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 706, DictType = "proj_status", DictLabel = "已签回合同", DictValue = "5", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 707, DictType = "proj_status", DictLabel = "执行中", DictValue = "6", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 708, DictType = "proj_status", DictLabel = "成果提交", DictValue = "7", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 709, DictType = "proj_status", DictLabel = "已完成", DictValue = "8", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 710, DictType = "proj_status", DictLabel = "已终止", DictValue = "9", Sort = 10, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 示例项目 ──────────────────────────────────────────
        mb.Entity<Project>().HasData(
            new Project
            {
                Id = 1001,
                ProjNo = "PRJ-2024-001",
                ProjName = "成都市某片区控制性详细规划",
                DeptId = 4,
                BizType = "控制性详细规划",
                OwnerName = "成都市规划和自然资源局",
                OwnerContact = "王处长",
                OwnerPhone = "028-12345678",
                ProcurementType = "竞争性磋商",
                ContractAmount = 98m,
                IsJointVenture = false,
                TechLeaderId = 108,
                BizLeaderId = 103,
                SignDate = new DateTime(2024, 3, 15),
                PlanEndDate = new DateTime(2024, 12, 31),
                ProgressStatus = 6,
                Remark = "重点项目，需配合规委会评审",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1002,
                ProjNo = "PRJ-2024-002",
                ProjName = "某住宅小区结算审核",
                DeptId = 2,
                BizType = "结算评审",
                OwnerName = "成都市某国有投资公司",
                OwnerContact = "张总",
                OwnerPhone = "13900000001",
                ProcurementType = "询价",
                ContractAmount = 45m,
                IsJointVenture = false,
                TechLeaderId = 102,
                BizLeaderId = 101,
                SignDate = new DateTime(2024, 5, 1),
                PlanEndDate = new DateTime(2024, 8, 31),
                ProgressStatus = 7,
                Remark = "送审金额约3200万元",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1003,
                ProjNo = "PRJ-2024-003",
                ProjName = "某市政道路工程可行性研究报告",
                DeptId = 3,
                BizType = "可行性研究报告",
                OwnerName = "成都市交通运输局",
                OwnerContact = "李科长",
                OwnerPhone = "028-87654321",
                ProcurementType = "公开招标",
                ContractAmount = 160m,
                IsJointVenture = true,
                OurRatio = 60m,
                TechLeaderId = 103,
                BizLeaderId = 107,
                SignDate = new DateTime(2024, 1, 20),
                PlanEndDate = new DateTime(2024, 10, 31),
                ProgressStatus = 6,
                Remark = "联合体项目，牵头方，我方占比60%",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1004,
                ProjNo = "PRJ-2023-018",
                ProjName = "某工业园区概念规划设计",
                DeptId = 5,
                BizType = "战略咨询",
                OwnerName = "成都高新区管委会",
                OwnerContact = "陈主任",
                OwnerPhone = "13800000099",
                ProcurementType = "单一来源",
                ContractAmount = 32m,
                IsJointVenture = false,
                TechLeaderId = 107,
                BizLeaderId = 104,
                SignDate = new DateTime(2023, 9, 1),
                PlanEndDate = new DateTime(2024, 3, 31),
                ActualEndDate = new DateTime(2024, 4, 15),
                ProgressStatus = 8,
                Remark = "已完成，已全额回款",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new Project
            {
                Id = 1005,
                ProjNo = "PRJ-2024-004",
                ProjName = "某新城片区节能评估报告",
                DeptId = 2,
                BizType = "节能评估报告",
                OwnerName = "成都天府新区建设局",
                OwnerContact = "刘工",
                OwnerPhone = "13700000001",
                ProcurementType = "竞争性磋商",
                ContractAmount = 28m,
                IsJointVenture = false,
                TechLeaderId = 105,
                BizLeaderId = 101,
                SignDate = new DateTime(2024, 6, 10),
                PlanEndDate = new DateTime(2024, 9, 30),
                ProgressStatus = 3,
                Remark = "投标截止2024-06-10",
                CreatedAt = dt,
                CreatedBy = "system"
            }
        );

        // ── 项目成员 ──────────────────────────────────────────
        mb.Entity<ProjectMember>().HasData(
            new ProjectMember { Id = 3001, ProjectId = 1001, EmployeeId = 108, Role = "项目负责人", DutyDesc = "总体技术把控，规划方案设计", Ratio = 40m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3002, ProjectId = 1001, EmployeeId = 104, Role = "参与人员", DutyDesc = "用地分析与指标测算", Ratio = 30m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3003, ProjectId = 1001, EmployeeId = 106, Role = "参与人员", DutyDesc = "现状调研与CAD制图", Ratio = 20m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3004, ProjectId = 1001, EmployeeId = 103, Role = "商务负责人", DutyDesc = "合同对接、开票及回款跟进", Ratio = 10m, JoinDate = new DateTime(2024, 3, 15), Status = 0, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMember { Id = 3005, ProjectId = 1002, EmployeeId = 102, Role = "项目负责人", DutyDesc = "结算审核技术负责", Ratio = 50m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3006, ProjectId = 1002, EmployeeId = 105, Role = "参与人员", DutyDesc = "工程量核算", Ratio = 30m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3007, ProjectId = 1002, EmployeeId = 101, Role = "商务负责人", DutyDesc = "商务对接", Ratio = 20m, JoinDate = new DateTime(2024, 5, 1), Status = 0, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMember { Id = 3008, ProjectId = 1003, EmployeeId = 103, Role = "项目负责人", DutyDesc = "可研报告编制", Ratio = 45m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3009, ProjectId = 1003, EmployeeId = 107, Role = "商务负责人", DutyDesc = "联合体协调及商务", Ratio = 35m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMember { Id = 3010, ProjectId = 1003, EmployeeId = 106, Role = "参与人员", DutyDesc = "交通量调查与分析", Ratio = 20m, JoinDate = new DateTime(2024, 1, 20), Status = 0, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 项目里程碑 ────────────────────────────────────────
        mb.Entity<ProjectMilestone>().HasData(
            new ProjectMilestone { Id = 4001, ProjectId = 1001, MilestoneName = "现状调研与基础资料收集", MilestoneType = "现状调研", PlanDate = new DateTime(2024, 4, 30), ActualDate = new DateTime(2024, 4, 25), OwnerId = 108, Status = 2, IsOverdue = false, Sort = 1, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4002, ProjectId = 1001, MilestoneName = "规划方案初稿", MilestoneType = "方案设计", PlanDate = new DateTime(2024, 7, 31), ActualDate = new DateTime(2024, 8, 5), OwnerId = 108, Status = 2, IsOverdue = true, Sort = 2, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4003, ProjectId = 1001, MilestoneName = "专家评审会", MilestoneType = "专家评审", PlanDate = new DateTime(2024, 10, 31), ActualDate = null, OwnerId = 108, Status = 1, IsOverdue = false, Sort = 3, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4004, ProjectId = 1001, MilestoneName = "成果正式交付", MilestoneType = "成果交付", PlanDate = new DateTime(2024, 12, 31), ActualDate = null, OwnerId = 103, AcceptAmount = 98m, Status = 0, IsOverdue = false, Sort = 4, CreatedAt = dt, CreatedBy = "system" },

            new ProjectMilestone { Id = 4005, ProjectId = 1002, MilestoneName = "资料收集与初步核查", MilestoneType = "资料收集", PlanDate = new DateTime(2024, 5, 20), ActualDate = new DateTime(2024, 5, 18), OwnerId = 102, Status = 2, IsOverdue = false, Sort = 1, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4006, ProjectId = 1002, MilestoneName = "结算审核报告初稿", MilestoneType = "方案设计", PlanDate = new DateTime(2024, 7, 15), ActualDate = new DateTime(2024, 7, 20), OwnerId = 102, Status = 2, IsOverdue = true, Sort = 2, CreatedAt = dt, CreatedBy = "system" },
            new ProjectMilestone { Id = 4007, ProjectId = 1002, MilestoneName = "审核报告正式提交", MilestoneType = "成果交付", PlanDate = new DateTime(2024, 8, 31), ActualDate = null, OwnerId = 101, AcceptAmount = 45m, Status = 1, IsOverdue = false, Sort = 3, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 验收记录 ──────────────────────────────────────────
        mb.Entity<ProjectAcceptance>().HasData(
            new ProjectAcceptance { Id = 5001, ProjectId = 1004, AcceptBatch = "第一批（预付款）", AcceptDate = new DateTime(2023, 10, 1), AcceptAmount = 9.6m, InvoiceNo = "INV2023100001", Remark = "预付款30%", CreatedAt = dt, CreatedBy = "system" },
            new ProjectAcceptance { Id = 5002, ProjectId = 1004, AcceptBatch = "第二批（中期款）", AcceptDate = new DateTime(2024, 1, 15), AcceptAmount = 16m, InvoiceNo = "INV2024010001", Remark = "完成中期成果50%", CreatedAt = dt, CreatedBy = "system" },
            new ProjectAcceptance { Id = 5003, ProjectId = 1004, AcceptBatch = "第三批（尾款）", AcceptDate = new DateTime(2024, 5, 20), AcceptAmount = 6.4m, InvoiceNo = "INV2024050001", Remark = "成果交付完成", CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 概预算示例任务 ────────────────────────────────────
        mb.Entity<BudgetTask>().HasData(
            new BudgetTask
            {
                Id = 6001,
                TaskNo = "YS-2024-001",
                TaskName = "某住宅小区（一期）工程结算审核",
                TaskType = 5,
                DeptId = 2,
                OwnerName = "成都某房产开发有限公司",
                OwnerContact = "赵总",
                OwnerPhone = "13900000002",
                ContractorName = "中建三局第二建设工程有限责任公司",
                BuildingScale = "住宅12栋，建筑面积约8.6万㎡",
                SubmitAmount = 6850m,
                ApprovedAmount = 6312m,
                ReductionRate = 7.85m,
                QuotaBasis = "四川省2015建设工程工程量清单计价定额",
                FeeStandard = "四川省2015费用定额",
                TechLeaderId = 102,
                BizLeaderId = 101,
                PlanDate = new DateTime(2024, 8, 31),
                Status = 4,
                Remark = "核减率7.85%，共出具评审意见42条",
                CreatedAt = dt,
                CreatedBy = "system"
            },
            new BudgetTask
            {
                Id = 6002,
                TaskNo = "YS-2024-002",
                TaskName = "某市政道路工程预算编制",
                TaskType = 1,
                DeptId = 3,
                OwnerName = "成都市交通运输局",
                OwnerContact = "李科长",
                OwnerPhone = "028-87654321",
                ContractorName = null,
                BuildingScale = "双向四车道，全长3.2km",
                SubmitAmount = 4200m,
                ApprovedAmount = null,
                ReductionRate = null,
                QuotaBasis = "四川省2020市政工程定额",
                FeeStandard = "四川省2020费用定额",
                TechLeaderId = 107,
                BizLeaderId = 103,
                PlanDate = new DateTime(2024, 10, 31),
                Status = 1,
                CreatedAt = dt,
                CreatedBy = "system"
            }
        );

        // ── 费用分部示例 ──────────────────────────────────────
        mb.Entity<BudgetSection>().HasData(
            new BudgetSection { Id = 7001, TaskId = 6001, SectionNo = 1, SectionName = "建筑工程", Category = "土建工程", ContractAmount = 3200m, SubmitAmount = 3100m, ApprovedAmount = 2890m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7002, TaskId = 6001, SectionNo = 2, SectionName = "安装工程", Category = "安装工程", ContractAmount = 1800m, SubmitAmount = 1750m, ApprovedAmount = 1650m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7003, TaskId = 6001, SectionNo = 3, SectionName = "室外市政配套", Category = "市政管道", ContractAmount = 1100m, SubmitAmount = 1000m, ApprovedAmount = 972m, Status = 2, CreatedAt = dt, CreatedBy = "system" },
            new BudgetSection { Id = 7004, TaskId = 6001, SectionNo = 4, SectionName = "绿化景观工程", Category = "绿化景观", ContractAmount = 800m, SubmitAmount = 0m, ApprovedAmount = 800m, Status = 2, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 知识库分类 ────────────────────────────────────────────
        mb.Entity<KbCategory>().HasData(
            new KbCategory { Id = 1, Name = "模板文件", Icon = "fa-file-word", Description = "常用工作模板，合同模板、报告模板等", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 2, Name = "公司通知", Icon = "fa-bullhorn", Description = "公司内部通知、公告", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 3, Name = "行业规范", Icon = "fa-book", Description = "工程咨询、规划、造价等行业标准规范", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 4, Name = "规章制度", Icon = "fa-gavel", Description = "公司规章制度、管理办法", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 5, Name = "培训资料", Icon = "fa-graduation-cap", Description = "内部培训讲义、学习材料", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 6, Name = "其他", Icon = "fa-folder-open", Description = "其他共享文件", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 菜单 ──────────────────────────────────────────────
        mb.Entity<SysMenu>().HasData(
            // ── 一级目录 ──────────────────────────────────────
            new SysMenu { Id = 1, MenuName = "系统管理", ParentId = 0, MenuType = "M", Icon = "fa-cogs", Path = "/system", Sort = 1, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 2, MenuName = "员工档案", ParentId = 0, MenuType = "M", Icon = "fa-users", Path = "/hr", Sort = 2, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 3, MenuName = "项目管理", ParentId = 0, MenuType = "M", Icon = "fa-project-diagram", Path = "/project", Sort = 3, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 4, MenuName = "概预算结算", ParentId = 0, MenuType = "M", Icon = "fa-file-invoice-dollar", Path = "/budget", Sort = 4, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 5, MenuName = "个人中心", ParentId = 0, MenuType = "M", Icon = "fa-user-circle", Path = "/profile", Sort = 5, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 6, MenuName = "知识库", ParentId = 0, MenuType = "M", Icon = "fa-database", Path = "/kb", Sort = 6, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 7, MenuName = "报表中心", ParentId = 0, MenuType = "M", Icon = "fa-chart-bar", Path = "/report", Sort = 7, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 8, MenuName = "造价小工具", ParentId = 0, MenuType = "M", Icon = "fa-calculator", Path = "/tool", Sort = 8, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // ── 系统管理子菜单 ────────────────────────────────
            new SysMenu { Id = 11, MenuName = "用户管理", ParentId = 1, MenuType = "C", Icon = "fa-user", Path = "/system/user", Sort = 1, Visible = 1, Status = 1, Perms = "sys:user:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 12, MenuName = "角色管理", ParentId = 1, MenuType = "C", Icon = "fa-user-tag", Path = "/system/role", Sort = 2, Visible = 1, Status = 1, Perms = "sys:role:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 13, MenuName = "部门管理", ParentId = 1, MenuType = "C", Icon = "fa-sitemap", Path = "/system/dept", Sort = 3, Visible = 1, Status = 1, Perms = "sys:dept:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 14, MenuName = "字典管理", ParentId = 1, MenuType = "C", Icon = "fa-book", Path = "/system/dict", Sort = 4, Visible = 1, Status = 1, Perms = "sys:dict:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 15, MenuName = "操作日志", ParentId = 1, MenuType = "C", Icon = "fa-history", Path = "/system/log", Sort = 5, Visible = 1, Status = 1, Perms = "sys:log:list", CreatedAt = dt, CreatedBy = "system" },
            // 菜单管理
            new SysMenu { Id = 16, MenuName = "菜单管理", ParentId = 1, MenuType = "C", Icon = "fa-th-list", Path = "/system/menu", Sort = 6, Visible = 1, Status = 1, Perms = "sys:menu:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 17, MenuName = "Debug工具", ParentId = 1, MenuType = "C", Icon = "fa-bug", Path = "/system/debug", Sort = 9, Visible = 1, Status = 1, Perms = "sys:debug:index", CreatedAt = dt, CreatedBy = "system" },

            // 用户管理按钮
            new SysMenu { Id = 111, MenuName = "新增", ParentId = 11, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:user:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 112, MenuName = "编辑", ParentId = 11, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:user:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 113, MenuName = "删除", ParentId = 11, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:user:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 114, MenuName = "重置密码", ParentId = 11, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:user:reset", CreatedAt = dt, CreatedBy = "system" },
            // 角色管理按钮
            new SysMenu { Id = 121, MenuName = "新增", ParentId = 12, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:role:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 122, MenuName = "编辑", ParentId = 12, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:role:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 123, MenuName = "删除", ParentId = 12, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:role:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 124, MenuName = "分配权限", ParentId = 12, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:role:perm", CreatedAt = dt, CreatedBy = "system" },
            // 部门管理按钮
            new SysMenu { Id = 131, MenuName = "新增", ParentId = 13, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:dept:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 132, MenuName = "编辑", ParentId = 13, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:dept:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 133, MenuName = "删除", ParentId = 13, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:dept:delete", CreatedAt = dt, CreatedBy = "system" },
            // 字典管理按钮
            new SysMenu { Id = 141, MenuName = "新增", ParentId = 14, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:dict:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 142, MenuName = "编辑", ParentId = 14, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:dict:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 143, MenuName = "删除", ParentId = 14, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:dict:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 144, MenuName = "分配权限", ParentId = 14, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:dict:perm", CreatedAt = dt, CreatedBy = "system" },
            // 操作日志管理按钮
            new SysMenu { Id = 151, MenuName = "新增", ParentId = 15, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:log:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 152, MenuName = "编辑", ParentId = 15, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:log:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 153, MenuName = "删除", ParentId = 15, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:log:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 154, MenuName = "分配权限", ParentId = 15, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:log:perm", CreatedAt = dt, CreatedBy = "system" },

            // 菜单管理按钮（sys:menu:add/edit/delete）
            new SysMenu { Id = 161, MenuName = "新增", ParentId = 16, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:menu:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 162, MenuName = "编辑", ParentId = 16, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:menu:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 163, MenuName = "删除", ParentId = 16, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:menu:delete", CreatedAt = dt, CreatedBy = "system" },

            // 字典管理按钮（只读查看，暂不设编辑权限按钮）
            // 操作日志（只读，无操作按钮）
            // 知识库子菜单
            new SysMenu { Id = 61, MenuName = "文件浏览", ParentId = 6, MenuType = "C", Icon = "fa-folder-open", Path = "/kb", Sort = 1, Visible = 1, Status = 1, Perms = "kb:file:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 62, MenuName = "文件管理", ParentId = 6, MenuType = "C", Icon = "fa-cog", Path = "/kb/manage", Sort = 2, Visible = 1, Status = 1, Perms = "kb:file:manage", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 621, MenuName = "上传", ParentId = 62, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "kb:file:upload", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 622, MenuName = "删除", ParentId = 62, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "kb:file:delete", CreatedAt = dt, CreatedBy = "system" },
            // 报表中心子菜单
            new SysMenu { Id = 71, MenuName = "回款报表", ParentId = 7, MenuType = "C", Icon = "fa-hand-holding-usd", Path = "/report/receipt", Sort = 1, Visible = 1, Status = 1, Perms = "report:receipt", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 72, MenuName = "产值报表", ParentId = 7, MenuType = "C", Icon = "fa-user-chart", Path = "/report/output", Sort = 2, Visible = 1, Status = 1, Perms = "report:output", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 81, MenuName = "报告生成", ParentId = 8, MenuType = "C", Icon = "fa-file-word", Path = "/tool/report", Sort = 1, Visible = 1, Status = 1, Perms = null, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 82, MenuName = "费用计算器", ParentId = 8, MenuType = "C", Icon = "fa-coins", Path = "/tool/calculator", Sort = 2, Visible = 1, Status = 1, Perms = null, CreatedAt = dt, CreatedBy = "system" },
            // 个人中心子菜单
            new SysMenu { Id = 51, MenuName = "个人资料", ParentId = 5, MenuType = "C", Icon = "fa-id-card", Path = "/profile", Sort = 1, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 52, MenuName = "产值统计", ParentId = 5, MenuType = "C", Icon = "fa-chart-bar", Path = "/my-stats", Sort = 2, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 员工档案
            new SysMenu { Id = 21, MenuName = "员工信息", ParentId = 2, MenuType = "C", Icon = "fa-id-card", Path = "/hr/employee", Sort = 1, Visible = 1, Status = 1, Perms = "hr:employee:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 22, MenuName = "合同管理", ParentId = 2, MenuType = "C", Icon = "fa-file-contract", Path = "/hr/contract", Sort = 2, Visible = 1, Status = 1, Perms = "hr:contract:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 23, MenuName = "证书管理", ParentId = 2, MenuType = "C", Icon = "fa-certificate", Path = "/hr/cert", Sort = 3, Visible = 1, Status = 1, Perms = "hr:cert:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 211, MenuName = "新增", ParentId = 21, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "hr:employee:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 212, MenuName = "编辑", ParentId = 21, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "hr:employee:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 213, MenuName = "转正", ParentId = 21, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "hr:employee:formal", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 214, MenuName = "离职", ParentId = 21, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "hr:employee:leave", CreatedAt = dt, CreatedBy = "system" },
            // 项目管理
            new SysMenu { Id = 31, MenuName = "项目台账", ParentId = 3, MenuType = "C", Icon = "fa-clipboard-list", Path = "/project", Sort = 1, Visible = 1, Status = 1, Perms = "proj:project:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 311, MenuName = "新建项目", ParentId = 31, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "proj:project:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 312, MenuName = "编辑项目", ParentId = 31, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "proj:project:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 313, MenuName = "变更状态", ParentId = 31, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "proj:project:status", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 314, MenuName = "终止项目", ParentId = 31, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "proj:project:terminate", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 315, MenuName = "添加成员", ParentId = 31, MenuType = "F", Sort = 5, Visible = 0, Status = 1, Perms = "proj:member:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 316, MenuName = "编辑成员", ParentId = 31, MenuType = "F", Sort = 6, Visible = 0, Status = 1, Perms = "proj:member:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 317, MenuName = "新增节点", ParentId = 31, MenuType = "F", Sort = 7, Visible = 0, Status = 1, Perms = "proj:milestone:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 318, MenuName = "完成节点", ParentId = 31, MenuType = "F", Sort = 8, Visible = 0, Status = 1, Perms = "proj:milestone:done", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 319, MenuName = "录入验收", ParentId = 31, MenuType = "F", Sort = 9, Visible = 0, Status = 1, Perms = "proj:acceptance:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 320, MenuName = "批量导入", ParentId = 31, MenuType = "F", Sort = 10, Visible = 0, Status = 1, Perms = "proj:project:import", CreatedAt = dt, CreatedBy = "system" },
            // 概预算结算
            new SysMenu { Id = 41, MenuName = "任务台账", ParentId = 4, MenuType = "C", Icon = "fa-calculator", Path = "/budget", Sort = 1, Visible = 1, Status = 1, Perms = "budget:task:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 411, MenuName = "新增任务", ParentId = 41, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "budget:task:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 412, MenuName = "编辑任务", ParentId = 41, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "budget:task:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 413, MenuName = "提交内审", ParentId = 41, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "budget:task:submit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 414, MenuName = "录入意见", ParentId = 41, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "budget:opinion:add", CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 超管拥有全部权限 ──────────────────────────────────
        var allMenuIds = new long[]
        {
            1,2,3,4,5,6,7,8,
            11,12,13,14,15,16,17,
            111,112,113,114,
            121,122,123,124,
            131,132,133,
            141,142,143,144,
            151,152,153,154,
            161,162,163,
            51,52,
            61,62,621,622,
            71,72,
            81,82,
            21,22,23,
            211,212,213,214,
            31, 311,312,313,314,315,316,317,318,319,320,
            41, 411,412,413,414,
        };
        mb.Entity<SysRoleMenu>().HasData(
            allMenuIds.Select(mid => new SysRoleMenu { RoleId = 1, MenuId = mid }).ToArray()
        );

        // 项目经理角色菜单（员工档案只读 + 项目管理全部）
        var pmMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            2, 21, 22, 23,          // 员工档案查看
            3, 31, 311,312,313,315,316,317,318,319,  // 项目管理全部
            4, 41, 413, 414,        // 概预算查看+录入意见
        };
        mb.Entity<SysRoleMenu>().HasData(
            pmMenuIds.Select(mid => new SysRoleMenu { RoleId = 3, MenuId = mid }).ToArray()
        );

        // 工程师角色菜单（项目查看 + 自己参与的操作）
        var engMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            3, 31, 317, 318, 319,   // 项目台账+完成节点+录入验收
            4, 41,                  // 概预算查看
        };
        mb.Entity<SysRoleMenu>().HasData(
            engMenuIds.Select(mid => new SysRoleMenu { RoleId = 4, MenuId = mid }).ToArray()
        );
    }
    /// <summary>
    /// EnsureCreated 模式下手动执行种子数据。
    /// Migration 模式下不需要调用此方法（HasData 已自动处理）。
    /// </summary>
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
