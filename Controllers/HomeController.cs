using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

[Authorize]
public class HomeController : BaseAuthController
{
    private readonly IProjectService _projSvc;
    private readonly IUserService    _userSvc;
    private readonly AppDbContext    _db;

    public HomeController(IPermissionService permSvc, IProjectService projSvc,
        IUserService userSvc, AppDbContext db)
        : base(permSvc)
    { _projSvc = projSvc; _userSvc = userSvc; _db = db; }

    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = await _projSvc.GetDashboardStatsAsync();
        return View();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await _userSvc.GetDetailAsync(User.GetUserId());
        if (user == null) return RedirectToAction("Login", "Account");
        return View(user);
    }

    [HttpPost("changepwd"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePwd(
        [FromBody] EnterpriseMS.Services.DTOs.User.ChangePasswordDto dto)
    {
        try
        {
            await _userSvc.ChangePasswordAsync(User.GetUserId(), dto.OldPassword, dto.NewPassword);
            return Json(ApiResult<object>.Ok("密码修改成功，请重新登录"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    /// <summary>个人产值统计</summary>
    [HttpGet("my-stats")]
    public async Task<IActionResult> MyStats()
    {
        var userInfo = await _userSvc.GetDetailAsync(User.GetUserId());
        ViewBag.EmployeeName = userInfo?.RealName ?? User.GetRealName();

        if (userInfo?.EmployeeId == null)
        {
            ViewBag.Error        = "您的账号未绑定员工档案，无法查看产值统计。请联系管理员在用户管理中绑定员工。";
            ViewBag.TotalContractValue = 0m;
            ViewBag.ProjectCount = 0;
            return View();   // 视图使用 ViewBag，不依赖 Model
        }

        var empId   = userInfo.EmployeeId.Value;
        var members = await _db.Set<EnterpriseMS.Domain.Entities.Project.ProjectMember>()
            .Include(m => m.Project).ThenInclude(p => p!.Dept)
            .Where(m => m.EmployeeId == empId && m.Status == 0 && !m.IsDeleted)
            .ToListAsync();

        var stats = members
            .Where(m => m.Project != null)
            .Select(m => new MyStatItem
            {
                ProjId          = m.ProjectId,
                ProjNo          = m.Project!.ProjNo,
                ProjName        = m.Project.ProjName,
                DeptName        = m.Project.Dept?.DeptName,
                Role            = m.Role,
                Ratio           = m.Ratio,
                ProgressStatus  = m.Project.ProgressStatus,
                ContractAmount  = m.Project.ActualContractAmount,
                MyContractValue = m.Project.ActualContractAmount * m.Ratio / 100,
            }).ToList();

        ViewBag.TotalContractValue = stats.Sum(s => s.MyContractValue);
        ViewBag.ProjectCount       = stats.Count;
        return View(stats);
    }

    [AllowAnonymous]
    public IActionResult Forbidden() => View();

    [AllowAnonymous]
    public IActionResult Error(string? message)
    {
        ViewBag.Message = message ?? "服务器内部错误";
        return View();
    }
}

/// <summary>个人产值统计条目（供 MyStats 视图使用）</summary>
public class MyStatItem
{
    public long    ProjId          { get; set; }
    public string  ProjNo          { get; set; } = "";
    public string  ProjName        { get; set; } = "";
    public string? DeptName        { get; set; }
    public string  Role            { get; set; } = "";
    public decimal Ratio           { get; set; }
    public int     ProgressStatus  { get; set; }
    public decimal ContractAmount  { get; set; }
    public decimal MyContractValue { get; set; }
}
