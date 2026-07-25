using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Domain.Entities.Budget;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 系统种子 / 维护服务：承接原 DebugController 中直接写在 Controller 内的 DbContext 持久化逻辑。
/// 该服务本身合法注入 AppDbContext（种子/迁移本质就是 DbContext 级别操作），
/// 从而让 Controller 回归“仅做参数校验与路由分发”。
/// </summary>
public class SystemSeedService : ISystemSeedService
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _permSvc;

    public SystemSeedService(AppDbContext db, IPermissionService permSvc)
    { _db = db; _permSvc = permSvc; }

    public bool IsSuperAdmin(long userId)
        => _db.SysUserRoles.Any(ur => ur.UserId == userId && ur.RoleId == 1);

    // ── 数据库状态统计 ──────────────────────────────────────
    public async Task<SeedStatsDto> GetStatsAsync()
    {
        var stats = new Dictionary<string, int>
        {
            ["部门"]     = await _db.SysDepts.CountAsync(),
            ["岗位"]     = await _db.SysPosts.CountAsync(),
            ["角色"]     = await _db.SysRoles.CountAsync(),
            ["菜单"]     = await _db.SysMenus.CountAsync(),
            ["用户"]     = await _db.SysUsers.CountAsync(),
            ["用户角色"] = await _db.SysUserRoles.CountAsync(),
            ["角色菜单"] = await _db.SysRoleMenus.CountAsync(),
            ["字典类型"] = await _db.SysDictTypes.CountAsync(),
            ["字典数据"] = await _db.SysDictDatas.CountAsync(),
            ["知识库分类"]= await _db.KbCategories.CountAsync(),
            ["员工"]     = await _db.Employees.CountAsync(),
            ["项目"]     = await _db.Projects.CountAsync(),
        };

        var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
        var applied = (await _db.Database.GetAppliedMigrationsAsync()).ToList();

        return new SeedStatsDto { Stats = stats, Pending = pending, Applied = applied };
    }

    // ── 全量种子写入（幂等）──────────────────────────────────
    public async Task<SeedAllResult> SeedAllAsync()
    {
        var result = new List<string>();
        var errors = new List<string>();

        async Task<int> SeedOne<T>(string label) where T : class
        {
            try
            {
                var before = await _db.Set<T>().CountAsync();
                await _db.SeedTablePublicAsync<T>();
                var after  = await _db.Set<T>().CountAsync();
                var added  = after - before;
                result.Add($"{label}：新增 {added} 条（共 {after} 条）");
                return added;
            }
            catch (Exception ex)
            {
                errors.Add($"{label}：{ex.Message}");
                return 0;
            }
        }

        int total = 0;
        total += await SeedOne<SysDept>   ("部门");
        total += await SeedOne<SysPost>   ("岗位");
        total += await SeedOne<SysRole>   ("角色");
        total += await SeedOne<SysMenu>   ("菜单");
        total += await SeedOne<SysUser>   ("用户");
        total += await SeedOne<SysUserRole>("用户角色");
        total += await SeedOne<SysRoleMenu>("角色菜单");
        total += await SeedOne<SysDictType>("字典类型");
        total += await SeedOne<SysDictData>("字典数据");
        total += await SeedOne<KbCategory>  ("知识库分类");
        total += await SeedOne<Employee>      ("员工");
        total += await SeedOne<EmployeeContract>("劳动合同");
        total += await SeedOne<EmployeeCertificate>("员工证书");
        total += await SeedOne<Project>          ("项目");
        total += await SeedOne<ProjectMember>   ("项目成员");
        total += await SeedOne<ProjectMilestone>("工作节点");
        total += await SeedOne<ProjectAcceptance>("验收记录");
        total += await SeedOne<BudgetTask>   ("概预算任务");
        total += await SeedOne<BudgetSection>("概预算分部");

        // 二次补齐菜单 / 角色菜单 / 字典（与 SeedMenu 保持一致的幂等写入）
        await SeedOne<SysMenu>   ("菜单");
        await SeedOne<SysRoleMenu>("角色菜单");
        await SeedOne<SysDictType>("字典类型");
        await SeedOne<SysDictData>("字典数据");

        return new SeedAllResult { TotalAdded = total, Details = result, Errors = errors };
    }

    // ── 只写入菜单 / 权限 / 字典（常用：补新菜单不重建数据库）──
    // 数据来源已统一收敛到 Seeds/ 下的 HasData 种子文件（MenuSeeds / DictSeeds / SystemSeeds），
    // 此处仅触发与 SeedAllAsync 一致的幂等 upsert（按主键跳过已存在行），不再硬编码菜单/字典数据。
    public async Task<List<string>> SeedMenuAndDictsAsync()
    {
        var results = new List<string>();

        async Task SeedOne<T>(string label) where T : class
        {
            var before = await _db.Set<T>().CountAsync();
            await _db.SeedTablePublicAsync<T>();
            var after = await _db.Set<T>().CountAsync();
            results.Add($"{label}：现有 {after} 条（本次新增 {after - before} 条）");
        }

        await SeedOne<SysMenu>("菜单");
        await SeedOne<SysRoleMenu>("角色菜单");
        await SeedOne<SysDictType>("字典类型");
        await SeedOne<SysDictData>("字典数据");

        // ── 清除当前用户权限缓存，让菜单立即生效 ────────────────
        var userIds = await _db.SysUsers
            .Where(u => !u.IsDeleted && u.Status == 1)
            .Select(u => u.Id).ToListAsync();
        foreach (var uid in userIds)
            await _permSvc.ClearUserCacheAsync(uid);
        results.Add($"已刷新 {userIds.Count} 个用户的权限缓存");

        return results;
    }

    // ── 清空权限缓存（所有用户）──────────────────────────────
    public async Task<int> ClearAllUserCacheAsync()
    {
        var userIds = await _db.SysUsers
            .Where(u => !u.IsDeleted && u.Status == 1)
            .Select(u => u.Id).ToListAsync();

        foreach (var uid in userIds)
            await _permSvc.ClearUserCacheAsync(uid);

        return userIds.Count;
    }

    // ── 执行待执行的 Migration ────────────────────────────────
    public async Task<(List<string> Pending, string? Error)> MigrateAsync()
    {
        try
        {
            var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
            if (!pending.Any())
                return (pending, null);

            await _db.Database.MigrateAsync();
            return (pending, null);
        }
        catch (Exception ex)
        {
            return (new List<string>(), ex.Message);
        }
    }
}
