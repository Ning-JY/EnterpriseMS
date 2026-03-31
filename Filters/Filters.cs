using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Filters;

// ── HasPermission 特性 + 过滤器 ──────────────────────────────
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class HasPermissionAttribute : TypeFilterAttribute
{
    public string Permission { get; }
    public HasPermissionAttribute(string permission) : base(typeof(PermissionFilter))
    {
        Permission = permission;
        Arguments  = new object[] { permission };
    }
}

public class PermissionFilter : IAsyncActionFilter
{
    private readonly IPermissionService _permSvc;
    private readonly string _permission;

    public PermissionFilter(IPermissionService permSvc, string permission)
    { _permSvc = permSvc; _permission = permission; }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var user = ctx.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            HandleUnauthorized(ctx);
            return;
        }
        // superadmin 直接放行
        if (user.IsInRole("superadmin")) { await next(); return; }

        var userId = user.GetUserId();
        if (userId == 0) { HandleUnauthorized(ctx); return; }

        var has = await _permSvc.HasPermAsync(userId, _permission);
        if (!has) { HandleForbidden(ctx); return; }

        await next();
    }

    private static void HandleUnauthorized(ActionExecutingContext ctx)
    {
        if (IsAjax(ctx))
            ctx.Result = new JsonResult(ApiResult<object>.Unauthorized());
        else
            ctx.Result = new RedirectToActionResult("Login", "Account",
                new { returnUrl = ctx.HttpContext.Request.Path });
    }

    private static void HandleForbidden(ActionExecutingContext ctx)
    {
        if (IsAjax(ctx))
            ctx.Result = new JsonResult(ApiResult<object>.Forbidden());
        else
            ctx.Result = new RedirectToActionResult("Forbidden", "Home", null);
    }

    private static bool IsAjax(ActionExecutingContext ctx)
        => ctx.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
}

// ── 操作日志过滤器 ────────────────────────────────────────────
[AttributeUsage(AttributeTargets.Method)]
public class OperLogAttribute : Attribute
{
    public string Title        { get; set; }
    public string BusinessType { get; set; }
    public OperLogAttribute(string title, string businessType = "")
    { Title = title; BusinessType = businessType; }
}

public class OperationLogFilter : IAsyncActionFilter
{
    private readonly IOperLogService _logSvc;
    public OperationLogFilter(IOperLogService logSvc) => _logSvc = logSvc;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var attr = ctx.ActionDescriptor.EndpointMetadata
                      .OfType<OperLogAttribute>().FirstOrDefault();
        var result = await next();
        if (attr == null || result.Exception != null) return;
        try
        {
            await _logSvc.LogAsync(attr.Title, null, attr.BusinessType);
        }
        catch { /* 日志失败不影响主流程 */ }
    }
}

// ── 全局异常过滤器 ────────────────────────────────────────────
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        => _logger = logger;

    public Task OnExceptionAsync(ExceptionContext ctx)
    {
        var ex = ctx.Exception;
        _logger.LogError(ex, "未处理异常: {Message}", ex.Message);

        var isAjax = ctx.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                  || ctx.HttpContext.Request.ContentType?.Contains("application/json") == true;

        if (ex is BusinessException || ex is NotFoundException)
        {
            ctx.Result = isAjax
                ? new JsonResult(ApiResult<object>.Fail(ex.Message))
                : new RedirectToActionResult("Error", "Home",
                    new { message = ex.Message });
            ctx.ExceptionHandled = true;
            return Task.CompletedTask;
        }

        ctx.Result = isAjax
            ? new JsonResult(ApiResult<object>.Fail("服务器内部错误，请稍后重试", 500))
            : new RedirectToActionResult("Error", "Home", null);
        ctx.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}
