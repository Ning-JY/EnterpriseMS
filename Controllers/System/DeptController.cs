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
    public IActionResult Index() => View();

    [HttpGet("list")]
    [HasPermission("sys:dept:list")]
    public async Task<IActionResult> List(string? keyword, int page = 1, int size = 10)
        => ApiOk(await _deptSvc.GetPagedAsync(keyword, page, size));

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _deptSvc.GetTreeAsync();
        return ApiOk(tree);
    }

    // 新增 / 编辑表单（iframe 弹层）
    [HttpGet("form")]
    [HasPermission("sys:dept:list")]
    public async Task<IActionResult> Form(long? id, long? parentId)
    {
        DeptTreeDto? model = null;
        if (id.HasValue)
        {
            model = await _deptSvc.GetByIdAsync(id.Value);
            if (model == null) return NotFound();
        }

        // 上级部门下拉：把树拍平成带缩进的选项
        var tree = await _deptSvc.GetTreeAsync();
        var options = new List<(long Id, string Name, int Depth)>();
        void Flatten(DeptTreeDto n, int depth)
        {
            options.Add((n.Id, n.DeptName, depth));
            foreach (var c in n.Children) Flatten(c, depth + 1);
        }
        foreach (var n in tree) Flatten(n, 0);

        ViewBag.ParentId = parentId;
        ViewBag.DeptOptions = options;
        return View(model);
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
