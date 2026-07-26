using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterpriseMS.Common;
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
    {
        _permSvc = permSvc;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // userId == 0 表示匿名用户，GetUserMenuTreeAsync 内部会返回公开菜单
        var userId = User.GetUserId();
        var menuTree = await _permSvc.GetUserMenuTreeAsync(userId);
        ViewBag.MenuTree = menuTree;

        await next();
    }

    // ── 统一 API 响应辅助：消除各 Controller 手写 Json(ApiResult ...) 的重复样板 ──
    // 说明：Controller 基类已有 Ok()，为避免命名冲突，这里统一用 ApiOk / ApiFail。
    // 行为与原 Json(ApiResult<object>.Ok/Fail(...)) 完全一致（data 承载业务数据，
    // message 默认“操作成功”），前端解析逻辑无需改动。

    /// <summary>成功：返回业务数据 data（message 默认“操作成功”）</summary>
    protected JsonResult ApiOk<T>(T data, string msg = "操作成功")
        => Json(ApiResult<T>.Ok(data, msg));

    /// <summary>失败：返回错误消息（code 默认 400）</summary>
    protected JsonResult ApiFail(string msg, int code = 400)
        => Json(ApiResult.Fail(msg, code));

    /// <summary>收集模型校验错误（消除各 Controller 重复的 GetErrors 私有方法）</summary>
    protected string GetErrors()
        => string.Join("；",
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
}
