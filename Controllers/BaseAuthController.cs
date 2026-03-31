using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

/// <summary>
/// 所有需要显示侧边菜单的 Controller 的基类。
/// 在每个 Action 执行前自动从缓存/数据库加载当前用户的菜单树，
/// 注入到 ViewBag.MenuTree，确保任何页面侧边栏都能正常渲染。
/// </summary>
public abstract class BaseAuthController : Controller
{
    private readonly IPermissionService _permSvc;

    protected BaseAuthController(IPermissionService permSvc)
        => _permSvc = permSvc;

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 仅对 ViewResult 类型的 Action 注入菜单（跳过 JsonResult/文件下载等）
        var userId = User.GetUserId();
        if (userId > 0)
        {
            var menuTree = await _permSvc.GetUserMenuTreeAsync(userId);
            ViewBag.MenuTree = menuTree;
        }

        await next();
    }
}
