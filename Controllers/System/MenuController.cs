using System;
using System.Collections.Generic;
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
    public IActionResult Index() => View();

    [HttpGet("list")]
    [HasPermission("sys:menu:list")]
    public async Task<IActionResult> List(string? keyword)
        => ApiOk(await _menuSvc.GetFlatListAsync(keyword));

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return ApiOk(tree);
    }

    // ── 新增 / 编辑表单页（iframe 弹窗，isDialog 模式）─────────
    [HttpGet("form")]
    [HasPermission("sys:menu:list")]
    public async Task<IActionResult> Form(long? id, long? parentId)
    {
        MenuTreeDto? menu = null;
        if (id.HasValue && id.Value > 0)
            menu = await _menuSvc.GetByIdAsync(id.Value);

        // 父级菜单下拉：扁平化菜单树；编辑时排除自身及其子孙
        var tree = await _menuSvc.GetTreeAsync();
        var options = new List<(long Id, string Name, int Depth)>();
        void Flatten(MenuTreeDto node, int depth)
        {
            if (menu != null && node.Id == menu.Id) return; // 跳过自身及其子树
            options.Add((node.Id, node.MenuName, depth));
            foreach (var c in node.Children) Flatten(c, depth + 1);
        }
        foreach (var root in tree) Flatten(root, 0);

        ViewBag.MenuOptions = options;
        ViewBag.ParentId = parentId;
        return View("Form", menu);
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
