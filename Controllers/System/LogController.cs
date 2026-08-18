using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.System;

// ── 操作日志 ──────────────────────────────────────────────────
[Authorize, Route("system/log")]
public class LogController : BaseAuthController
{
    private readonly IOperLogService _logSvc;
    public LogController(IOperLogService logSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _logSvc = logSvc;
    }

    [HasPermission("sys:log:list")]
    public async Task<IActionResult> Index(string? keyword, int page = 1, int size = 20)
    {
        var paged = await _logSvc.GetPagedAsync(keyword, page, size);
        ViewBag.Keyword = keyword;
        ViewBag.Page    = page;
        ViewBag.Size    = size;
        ViewBag.Total   = paged.Total;
        return View(paged.Items);
    }

    // 列表页 AJAX 数据源（前端 layui 表格调用）
    [HttpGet("list")]
    [HasPermission("sys:log:list")]
    public async Task<IActionResult> List(string? keyword, int page = 1, int size = 20)
        => ApiOk(await _logSvc.GetPagedAsync(keyword, page, size));
}
