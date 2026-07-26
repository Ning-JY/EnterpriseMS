using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.System;

// ── 部门管理 ──────────────────────────────────────────────────
[Authorize, Route("system/dept")]
public class DeptController : BaseAuthController
{
    private readonly IDeptService _deptSvc;
    private readonly IOperLogService _logSvc;

    public DeptController(IDeptService deptSvc, IOperLogService logSvc,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _deptSvc = deptSvc; _logSvc = logSvc;
    }

    [HasPermission("sys:dept:list")]
    public async Task<IActionResult> Index()
    {
        var tree = await _deptSvc.GetTreeAsync();
        return View(tree);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _deptSvc.GetTreeAsync();
        return ApiOk(tree);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dept:add")]
    public async Task<IActionResult> Create([FromBody] CreateDeptDto dto)
    {
        try
        {
            var id = await _deptSvc.CreateAsync(dto, User.GetRealName());
            return ApiOk(new { id }, "创建成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dept:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateDeptDto dto)
    {
        try
        {
            await _deptSvc.UpdateAsync(dto, User.GetRealName());
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:dept:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _deptSvc.DeleteAsync(id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
