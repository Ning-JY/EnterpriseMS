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
        var dt = new DateTime(2026, 1, 1);

        // ── 直接插入缺失菜单（幂等：主键已存在则跳过）──────────
        var menuSeeds = new List<EnterpriseMS.Domain.Entities.System.SysMenu>
    {
        // ── 在此处维护所有需要补充的菜单记录 ──

    };

        int menuAdded = 0, menuSkipped = 0;
        foreach (var m in menuSeeds)
        {
            if (!await _db.SysMenus.AnyAsync(x => x.Id == m.Id))
            {
                _db.SysMenus.Add(m);
                menuAdded++;
            }
            else menuSkipped++;
        }
        try { await _db.SaveChangesAsync(); results.Add($"菜单：新增 {menuAdded} 条，跳过 {menuSkipped} 条"); }
        catch (Exception ex) { results.Add($"菜单：失败 - {ex.Message}"); }

        // ── 直接插入缺失角色菜单绑定（超管 roleId=1）──────────
        var superAdminMenuIds = new long[]
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
        31,311,312,313,314,315,316,317,318,319,320,
        41,411,412,413,414,
        };
        int rmAdded = 0, rmSkipped = 0;
        foreach (var mid in superAdminMenuIds)
        {
            if (!await _db.SysRoleMenus.AnyAsync(x => x.RoleId == 1 && x.MenuId == mid))
            {
                _db.SysRoleMenus.Add(new EnterpriseMS.Domain.Entities.System.SysRoleMenu
                { RoleId = 1, MenuId = mid });
                rmAdded++;
            }
            else rmSkipped++;
        }
        try { await _db.SaveChangesAsync(); results.Add($"角色菜单：新增 {rmAdded} 条，跳过 {rmSkipped} 条"); }
        catch (Exception ex) { results.Add($"角色菜单：失败 - {ex.Message}"); }

        // ── 字典类型 / 字典数据：同样改为直接插入 ──────────────
        var dictTypeSeeds = new List<EnterpriseMS.Domain.Entities.System.SysDictType>
    {
        new() { Id=1,  DictName="业务类型",       DictType="biz_type",         Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=2,  DictName="采购方式",       DictType="procurement_type", Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=3,  DictName="合同类型",       DictType="contract_type",    Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=4,  DictName="证书类型",       DictType="cert_type",        Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=5,  DictName="里程碑类型",     DictType="milestone_type",   Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=6,  DictName="概预算任务类型", DictType="budget_task_type", Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=7,  DictName="项目进度状态",   DictType="proj_status",      Status=1, CreatedAt=dt, CreatedBy="system" },
    };
        int dtAdded = 0, dtSkipped = 0;
        foreach (var d in dictTypeSeeds)
        {
            if (!await _db.SysDictTypes.AnyAsync(x => x.Id == d.Id))
            { _db.SysDictTypes.Add(d); dtAdded++; }
            else dtSkipped++;
        }
        try { await _db.SaveChangesAsync(); results.Add($"字典类型：新增 {dtAdded} 条，跳过 {dtSkipped} 条"); }
        catch (Exception ex) { results.Add($"字典类型：失败 - {ex.Message}"); }

        // 字典数据（只示例补充几个关键条目，已有的会跳过）
        var dictDataSeeds = new List<EnterpriseMS.Domain.Entities.System.SysDictData>
    {
        new() { Id=101, DictType="biz_type", DictLabel="可行性研究报告", DictValue="可行性研究报告", Sort=1,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=102, DictType="biz_type", DictLabel="节能评估报告",   DictValue="节能评估报告",   Sort=2,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=103, DictType="biz_type", DictLabel="稳评报告",       DictValue="稳评报告",       Sort=3,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=104, DictType="biz_type", DictLabel="概算编制",       DictValue="概算编制",       Sort=4,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=105, DictType="biz_type", DictLabel="预算编制",       DictValue="预算编制",       Sort=5,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=106, DictType="biz_type", DictLabel="结算编制",       DictValue="结算编制",       Sort=6,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=107, DictType="biz_type", DictLabel="概算评审",       DictValue="概算评审",       Sort=7,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=108, DictType="biz_type", DictLabel="预算评审",       DictValue="预算评审",       Sort=8,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=109, DictType="biz_type", DictLabel="结算评审",       DictValue="结算评审",       Sort=9,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=110, DictType="biz_type", DictLabel="控制性详细规划", DictValue="控制性详细规划", Sort=10, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=111, DictType="biz_type", DictLabel="专项规划",       DictValue="专项规划",       Sort=11, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=112, DictType="biz_type", DictLabel="城市更新规划",   DictValue="城市更新规划",   Sort=12, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=113, DictType="biz_type", DictLabel="施工图设计",     DictValue="施工图设计",     Sort=13, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=114, DictType="biz_type", DictLabel="战略咨询",       DictValue="战略咨询",       Sort=14, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=115, DictType="biz_type", DictLabel="施工阶段全过程管控", DictValue="施工阶段全过程管控", Sort=15, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=201, DictType="procurement_type", DictLabel="竞争性磋商",   DictValue="竞争性磋商",   Sort=1, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=202, DictType="procurement_type", DictLabel="询价",         DictValue="询价",         Sort=2, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=203, DictType="procurement_type", DictLabel="公开招标",     DictValue="公开招标",     Sort=3, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=204, DictType="procurement_type", DictLabel="邀请招标",     DictValue="邀请招标",     Sort=4, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=205, DictType="procurement_type", DictLabel="公开招选",     DictValue="公开招选",     Sort=5, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=206, DictType="procurement_type", DictLabel="框架协议采购", DictValue="框架协议采购", Sort=6, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=207, DictType="procurement_type", DictLabel="单一来源",     DictValue="单一来源",     Sort=7, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=301, DictType="contract_type", DictLabel="固定期限",   DictValue="固定期限",   Sort=1, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=302, DictType="contract_type", DictLabel="无固定期限", DictValue="无固定期限", Sort=2, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=303, DictType="contract_type", DictLabel="劳务合同",   DictValue="劳务合同",   Sort=3, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=304, DictType="contract_type", DictLabel="实习协议",   DictValue="实习协议",   Sort=4, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=401, DictType="cert_type", DictLabel="注册规划师", DictValue="注册规划师", Sort=1, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=402, DictType="cert_type", DictLabel="造价工程师", DictValue="造价工程师", Sort=2, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=403, DictType="cert_type", DictLabel="注册建筑师", DictValue="注册建筑师", Sort=3, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=404, DictType="cert_type", DictLabel="注册工程师", DictValue="注册工程师", Sort=4, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=405, DictType="cert_type", DictLabel="建造师",     DictValue="建造师",     Sort=5, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=406, DictType="cert_type", DictLabel="职称证书",   DictValue="职称证书",   Sort=6, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=407, DictType="cert_type", DictLabel="岗位证书",   DictValue="岗位证书",   Sort=7, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=501, DictType="milestone_type", DictLabel="资料收集", DictValue="资料收集", Sort=1, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=502, DictType="milestone_type", DictLabel="现状调研", DictValue="现状调研", Sort=2, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=503, DictType="milestone_type", DictLabel="方案设计", DictValue="方案设计", Sort=3, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=504, DictType="milestone_type", DictLabel="内部评审", DictValue="内部评审", Sort=4, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=505, DictType="milestone_type", DictLabel="专家评审", DictValue="专家评审", Sort=5, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=506, DictType="milestone_type", DictLabel="报批上报", DictValue="报批上报", Sort=6, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=507, DictType="milestone_type", DictLabel="成果交付", DictValue="成果交付", Sort=7, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=508, DictType="milestone_type", DictLabel="回款",     DictValue="回款",     Sort=8, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=601, DictType="budget_task_type", DictLabel="概算编制", DictValue="0", Sort=1, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=602, DictType="budget_task_type", DictLabel="预算编制", DictValue="1", Sort=2, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=603, DictType="budget_task_type", DictLabel="结算编制", DictValue="2", Sort=3, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=604, DictType="budget_task_type", DictLabel="概算评审", DictValue="3", Sort=4, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=605, DictType="budget_task_type", DictLabel="预算评审", DictValue="4", Sort=5, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=606, DictType="budget_task_type", DictLabel="结算评审", DictValue="5", Sort=6, Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=701, DictType="proj_status", DictLabel="前期商务",          DictValue="0", Sort=1,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=702, DictType="proj_status", DictLabel="预计启动",          DictValue="1", Sort=2,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=703, DictType="proj_status", DictLabel="标书制作中",        DictValue="2", Sort=3,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=704, DictType="proj_status", DictLabel="投标/磋商中",       DictValue="3", Sort=4,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=705, DictType="proj_status", DictLabel="已中标·签订合同中", DictValue="4", Sort=5,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=706, DictType="proj_status", DictLabel="已签回合同",        DictValue="5", Sort=6,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=707, DictType="proj_status", DictLabel="执行中",            DictValue="6", Sort=7,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=708, DictType="proj_status", DictLabel="成果提交",          DictValue="7", Sort=8,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=709, DictType="proj_status", DictLabel="已完成",            DictValue="8", Sort=9,  Status=1, CreatedAt=dt, CreatedBy="system" },
        new() { Id=710, DictType="proj_status", DictLabel="已终止",            DictValue="9", Sort=10, Status=1, CreatedAt=dt, CreatedBy="system" },
    };
        int ddAdded = 0, ddSkipped = 0;
        foreach (var d in dictDataSeeds)
        {
            if (!await _db.SysDictDatas.AnyAsync(x => x.Id == d.Id))
            { _db.SysDictDatas.Add(d); ddAdded++; }
            else ddSkipped++;
        }
        try { await _db.SaveChangesAsync(); results.Add($"字典数据：新增 {ddAdded} 条，跳过 {ddSkipped} 条"); }
        catch (Exception ex) { results.Add($"字典数据：失败 - {ex.Message}"); }

        // ── 清除当前用户权限缓存，让菜单立即生效 ────────────────
        var userIds = await _db.SysUsers
            .Where(u => !u.IsDeleted && u.Status == 1)
            .Select(u => u.Id).ToListAsync();
        foreach (var uid in userIds)
            await _permSvc.ClearUserCacheAsync(uid);
        results.Add($"已刷新 {userIds.Count} 个用户的权限缓存");
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
