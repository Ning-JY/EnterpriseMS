using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.System;

// ── 菜单管理 ──────────────────────────────────────────────────
[Authorize, Route("system/menu")]
public class MenuController : BaseAuthController
{
    private readonly IMenuService _menuSvc;
    public MenuController(IMenuService menuSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _menuSvc = menuSvc;
    }

    [HasPermission("sys:menu:list")]
    public async Task<IActionResult> Index()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return View(tree);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return ApiOk(tree);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:menu:add")]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto)
    {
        try
        {
            var id = await _menuSvc.CreateAsync(dto, User.Identity?.Name ?? "system");
            return ApiOk(new { id }, "创建成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpGet("detail/{id}")]
    [HasPermission("sys:menu:edit")]
    public async Task<IActionResult> Detail(long id)
    {
        var menu = await _menuSvc.GetByIdAsync(id);
        if (menu == null) return ApiFail("菜单不存在");
        return ApiOk(new { menu });
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:menu:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateMenuDto dto)
    {
        try
        {
            await _menuSvc.UpdateAsync(dto, User.Identity?.Name ?? "system");
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:menu:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _menuSvc.DeleteAsync(id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
