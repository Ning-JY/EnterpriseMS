using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.System;

[Authorize, Route("system/role")]
public class RoleController : BaseAuthController
{
    private readonly IRoleService _roleSvc;
    private readonly IMenuService _menuSvc;
    private readonly IOperLogService _logSvc;

    public RoleController(IRoleService roleSvc, IMenuService menuSvc, IOperLogService logSvc,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _roleSvc = roleSvc; _menuSvc = menuSvc; _logSvc = logSvc;
    }

    [HasPermission("sys:role:list")]
    public async Task<IActionResult> Index(string? keyword, int page = 1, int size = 10)
    {
        var result = await _roleSvc.GetPagedAsync(keyword, page, size);
        ViewBag.Keyword = keyword;
        return View(result);
    }

    [HttpGet("detail/{id}")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var role = await _roleSvc.GetByIdAsync(id);
        if (role == null) return Json(ApiResult<object>.Fail("角色不存在"));
        var menuIds = await _roleSvc.GetRoleMenuIdsAsync(id);
        return Json(ApiResult<object>.Ok(new { role, menuIds }));
    }

    [HttpGet("menutree")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> MenuTree()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return Json(ApiResult<object>.Ok(tree));
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:add")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid) return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            var id = await _roleSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新增角色", $"角色：{dto.RoleName}", "INSERT", id);
            return Json(ApiResult<object>.Ok("角色创建成功"));
        }
        catch (BusinessException ex) { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid) return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            await _roleSvc.UpdateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("修改角色", $"角色ID：{dto.Id}", "UPDATE", dto.Id);
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:role:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _roleSvc.DeleteAsync(id, User.GetRealName());
            await _logSvc.LogAsync("删除角色", $"角色ID：{id}", "DELETE", id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("assignmenus"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:perm")]
    public async Task<IActionResult> AssignMenus(long roleId, [FromBody] List<long> menuIds)
    {
        await _roleSvc.AssignMenusAsync(roleId, menuIds);
        return Json(ApiResult<object>.Ok("权限分配成功"));
    }

    private string GetErrors() => string.Join("；",
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
}
