using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EnterpriseMS.Common;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.System;

/// <summary>
/// Debug 工具 Controller - 仅超级管理员可用
/// 提供种子数据重新写入、数据库状态查看等开发/运维辅助功能。
/// 所有 DbContext 持久化已下沉到 ISystemSeedService，本控制器仅做参数校验与路由分发。
/// </summary>
[Authorize]
[Route("system/debug")]
public class DebugController : BaseAuthController
{
    private readonly ISystemSeedService _seedSvc;
    private readonly IPermissionService _permSvc;

    public DebugController(ISystemSeedService seedSvc, IPermissionService permSvc): base(permSvc)
    { _seedSvc = seedSvc; _permSvc = permSvc; }

    // ── 只允许 superadmin 访问的统一检查 ──────────────────────
    private bool IsSuperAdmin()
    {
        if (User.IsInRole("superadmin")) return true;
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out var userId))
            return _seedSvc.IsSuperAdmin(userId);
        return false;
    }

    private IActionResult Forbidden() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Forbidden", "Home")
            : RedirectToAction("Login", "Account");

    // ── 主页面 ───────────────────────────────────────────────
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!IsSuperAdmin()) return Forbidden();

        var stats = await _seedSvc.GetStatsAsync();
        ViewBag.Stats   = stats.Stats;
        ViewBag.Pending = stats.Pending;
        ViewBag.Applied = stats.Applied;
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

        var r = await _seedSvc.SeedAllAsync();
        return Json(ApiResult<object>.Ok(new
        {
            TotalAdded = r.TotalAdded,
            Details    = r.Details,
            Errors     = r.Errors,
        }, r.Errors.Any()
            ? $"种子数据写入完成，新增 {r.TotalAdded} 条，{r.Errors.Count} 个表出错"
            : $"种子数据写入完成，共新增 {r.TotalAdded} 条"));
    }

    // ── 只写入菜单和权限（常用：补新菜单不重建数据库）──────────
    [HttpPost("seed-menu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedMenu()
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限"));

        var results = await _seedSvc.SeedMenuAndDictsAsync();
        return Json(ApiResult<object>.Ok(results, "菜单/权限/字典写入完成"));
    }

    // ── 清空权限缓存（所有用户）──────────────────────────────
    [HttpPost("clear-cache")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        if (!IsSuperAdmin())
            return Json(ApiResult<object>.Fail("无权限"));

        var count = await _seedSvc.ClearAllUserCacheAsync();
        return Json(ApiResult<object>.Ok($"已清除 {count} 个用户的权限缓存"));
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

        var (pending, error) = await _seedSvc.MigrateAsync();
        if (error != null)
            return Json(ApiResult<object>.Fail($"迁移失败：{error}"));
        if (!pending.Any())
            return Json(ApiResult<object>.Ok("无待执行的迁移，数据库已是最新版本"));
        return Json(ApiResult<object>.Ok(pending, $"迁移完成，共执行 {pending.Count} 个迁移"));
    }
}
