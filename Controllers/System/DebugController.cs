using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.System;

/// <summary>
/// Debug 工具 Controller - 仅超级管理员可用
/// 提供种子数据重新写入、数据库状态查看等开发/运维辅助功能
/// </summary>
[Authorize]
[Route("system/debug")]
public class DebugController : BaseAuthController
{
    private readonly AppDbContext      _db;
    private readonly IPermissionService _permSvc;

    public DebugController(AppDbContext db, IPermissionService permSvc): base(permSvc)
    { _db = db; _permSvc = permSvc; }

    // ── 只允许 superadmin 访问的统一检查 ──────────────────────
    private bool IsSuperAdmin() => User.IsInRole("superadmin");

    private IActionResult Forbidden() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Forbidden", "Home")
            : RedirectToAction("Login", "Account");

    // ── 主页面 ───────────────────────────────────────────────
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!IsSuperAdmin()) return Forbidden();

        // 各表当前数据量
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

        // 待执行迁移
        var pending  = (await _db.Database.GetPendingMigrationsAsync()).ToList();
        var applied  = (await _db.Database.GetAppliedMigrationsAsync()).ToList();

        ViewBag.Stats   = stats;
        ViewBag.Pending = pending;
        ViewBag.Applied = applied;
        return View();
    }

    // ── 写入种子数据（幂等，已存在则跳过）──────────────────────
    [HttpPost("seed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Seed([FromForm] string confirm)
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限，仅超级管理员可操作"));
        if (confirm != "CONFIRM")
            return Json(ApiResult<object>.Fail("请输入确认字符 CONFIRM"));

        var result = new List<string>();
        var errors = new List<string>();

        // 逐表统计写入情况
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
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysDept>   ("部门");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysPost>   ("岗位");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysRole>   ("角色");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysMenu>   ("菜单");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysUser>   ("用户");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysUserRole>("用户角色");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysRoleMenu>("角色菜单");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysDictType>("字典类型");
        total += await SeedOne<EnterpriseMS.Domain.Entities.System.SysDictData>("字典数据");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Info.KbCategory>  ("知识库分类");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Hr.Employee>      ("员工");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Hr.EmployeeContract>   ("劳动合同");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Hr.EmployeeCertificate>("员工证书");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Project.Project>          ("项目");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Project.ProjectMember>   ("项目成员");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Project.ProjectMilestone>("工作节点");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Project.ProjectAcceptance>("验收记录");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Budget.BudgetTask>   ("概预算任务");
        total += await SeedOne<EnterpriseMS.Domain.Entities.Budget.BudgetSection>("概预算分部");

        return Json(ApiResult<object>.Ok(new
        {
            TotalAdded = total,
            Details    = result,
            Errors     = errors,
        }, errors.Any()
            ? $"种子数据写入完成，新增 {total} 条，{errors.Count} 个表出错"
            : $"种子数据写入完成，共新增 {total} 条"));
    }

    // ── 只写入菜单和权限（常用：补新菜单不重建数据库）──────────
    [HttpPost("seed-menu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedMenu()
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限"));

        var results = new List<string>();
        async Task SeedOne<T>(string label) where T : class
        {
            try
            {
                var before = await _db.Set<T>().CountAsync();
                await _db.SeedTablePublicAsync<T>();
                var after  = await _db.Set<T>().CountAsync();
                results.Add($"{label}：新增 {after - before} 条");
            }
            catch (Exception ex) { results.Add($"{label}：失败 - {ex.Message}"); }
        }

        await SeedOne<EnterpriseMS.Domain.Entities.System.SysMenu>   ("菜单");
        await SeedOne<EnterpriseMS.Domain.Entities.System.SysRoleMenu>("角色菜单");
        await SeedOne<EnterpriseMS.Domain.Entities.System.SysDictType>("字典类型");
        await SeedOne<EnterpriseMS.Domain.Entities.System.SysDictData>("字典数据");

        return Json(ApiResult<object>.Ok(results,
            $"菜单/权限/字典写入完成"));
    }

    // ── 清空权限缓存（所有用户）──────────────────────────────
    [HttpPost("clear-cache")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限"));

        var userIds = await _db.SysUsers
            .Where(u => !u.IsDeleted && u.Status == 1)
            .Select(u => u.Id).ToListAsync();

        foreach (var uid in userIds)
            await _permSvc.ClearUserCacheAsync(uid);

        return Json(ApiResult<object>.Ok($"已清除 {userIds.Count} 个用户的权限缓存"));
    }

    // ── 执行待执行的 Migration ────────────────────────────────
    [HttpPost("migrate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Migrate([FromForm] string confirm)
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限"));
        if (confirm != "CONFIRM")
            return Json(ApiResult<object>.Fail("请输入确认字符 CONFIRM"));

        try
        {
            var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
            if (!pending.Any())
                return Json(ApiResult<object>.Ok("无待执行的迁移，数据库已是最新版本"));

            await _db.Database.MigrateAsync();
            return Json(ApiResult<object>.Ok(pending, $"迁移完成，共执行 {pending.Count} 个迁移"));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail($"迁移失败：{ex.Message}"));
        }
    }
}
