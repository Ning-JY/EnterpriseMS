using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Controllers;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Tool;

/// <summary>
/// 造价小工具模块 — 全部 Action 允许匿名访问
/// </summary>
[AllowAnonymous]
[Route("tool")]
public class ToolController : BaseAuthController
{
    public ToolController(IPermissionService permSvc) : base(permSvc) { }

    /// <summary>工程审核报告生成工具</summary>
    [HttpGet("report")]
    public IActionResult Report() => View();

    /// <summary>造价咨询费用计算器</summary>
    [HttpGet("calculator")]
    public IActionResult Calculator() => View();
}
