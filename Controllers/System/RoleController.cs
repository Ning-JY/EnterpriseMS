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
        _roleSvc = roleSvc;
        _menuSvc = menuSvc;
        _logSvc = logSvc;
    }

    [HasPermission("sys:role:list")]
    public async Task<IActionResult> Index(string? keyword, int page = 1, int size = 10)
    {
        var result = await _roleSvc.GetPagedAsync(keyword, page, size);
        ViewBag.Keyword = keyword;
        return View(result);
    }

    // 列表页 AJAX 数据源（前端 layui 表格调用）
    [HttpGet("list")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> List(string? keyword, int page = 1, int size = 10)
        => ApiOk(await _roleSvc.GetPagedAsync(keyword, page, size));

    [HttpGet("detail/{id}")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var role = await _roleSvc.GetByIdAsync(id);
        if (role == null) return ApiFail("角色不存在");
        var menuIds = await _roleSvc.GetRoleMenuIdsAsync(id);
        return ApiOk(new { role, menuIds });
    }

    [HttpGet("menutree")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> MenuTree()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return ApiOk(tree);
    }

    // 新增 / 编辑表单（iframe 弹层）
    [HttpGet("form")]
    [HasPermission("sys:role:list")]
    public async Task<IActionResult> Form(long? id)
    {
        RoleListDto? model = null;
        if (id.HasValue)
        {
            model = await _roleSvc.GetByIdAsync(id.Value);
            if (model == null) return NotFound();
        }
        return View(model);
    }

    // 分配权限（iframe 弹层）
    [HttpGet("perm")]
    [HasPermission("sys:role:perm")]
    public async Task<IActionResult> Perm(long id)
    {
        var role = await _roleSvc.GetByIdAsync(id);
        if (role == null) return NotFound();
        var menuIds = await _roleSvc.GetRoleMenuIdsAsync(id);
        ViewBag.RoleId = role.Id;
        ViewBag.RoleName = role.RoleName;
        ViewBag.MenuIds = menuIds;
        // 菜单树预渲染（原前端 ems.get('/system/role/menutree') 打开时异步 → 改为服务端同步，避免弹窗偏下）
        ViewBag.MenuTree = await _menuSvc.GetTreeAsync();
        return View();
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:add")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid) return ApiFail(GetErrors());
        try
        {
            var id = await _roleSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新增角色", $"角色：{dto.RoleName}", "INSERT", id);
            return ApiOk("角色创建成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid) return ApiFail(GetErrors());
        try
        {
            await _roleSvc.UpdateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("修改角色", $"角色ID：{dto.Id}", "UPDATE", dto.Id);
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:role:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _roleSvc.DeleteAsync(id, User.GetRealName());
            await _logSvc.LogAsync("删除角色", $"角色ID：{id}", "DELETE", id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("assignmenus"), ValidateAntiForgeryToken]
    [HasPermission("sys:role:perm")]
    public async Task<IActionResult> AssignMenus(long roleId, [FromBody] List<long> menuIds)
    {
        await _roleSvc.AssignMenusAsync(roleId, menuIds);
        return ApiOk("权限分配成功");
    }

}
