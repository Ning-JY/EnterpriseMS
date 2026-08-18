using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        // 先检查是否有显式的 OperLogAttribute
        var attr = ctx.ActionDescriptor.EndpointMetadata
                      .OfType<OperLogAttribute>().FirstOrDefault();
        var result = await next();
        if (result.Exception != null) return;

        try
        {
            if (attr != null)
            {
                // 使用显式标记的日志
                await _logSvc.LogAsync(attr.Title, null, attr.BusinessType);
            }
            else
            {
                // 自动记录 POST 写操作（create/update/delete）
                var method = ctx.HttpContext.Request.Method;
                if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    var actionName = ctx.ActionDescriptor.RouteValues["action"]?.ToString() ?? "";
                    var controllerName = ctx.ActionDescriptor.RouteValues["controller"]?.ToString() ?? "";
                    var path = ctx.HttpContext.Request.Path.Value ?? "";

                    // 只记录实际的写操作，跳过 CSRF token 等无关请求
                    bool isWriteOp = actionName.Contains("Create", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Update", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Delete", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Save", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Formal", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Leave", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Complete", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Status", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Terminate", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Received", StringComparison.OrdinalIgnoreCase)
                                  || actionName.Contains("Import", StringComparison.OrdinalIgnoreCase);

                    if (isWriteOp)
                    {
                        var title = $"{controllerName}/{actionName}";
                        await _logSvc.LogAsync(title, null, method);
                    }
                }
            }
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

        // 防止错误页自身再次被重定向回错误页，形成浏览器侧 302 自引用死循环。
        // 已是错误页时直接返回 500，不再 RedirectToAction("Error")。
        var path = ctx.HttpContext.Request.Path.Value ?? "";
        if (path.Equals("/Home/Error", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/Home/Error/", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            ctx.ExceptionHandled = true;
            return Task.CompletedTask;
        }

        var isAjax = ctx.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                  || ctx.HttpContext.Request.ContentType?.Contains("application/json") == true;

        if (ex is BusinessException || ex is NotFoundException)
        {
            if (isAjax)
            {
                ctx.Result = new JsonResult(ApiResult<object>.Fail(ex.Message));
            }
            else
            {
                // 直接渲染错误视图（不走 RedirectToAction），避免 302 重定向；
                // 否则错误页自身再异常时会与 UseExceptionHandler 的 302 叠加成
                // /Home/Error → 302 → /Home/Error 的死循环。
                SetErrorView(ctx, ex, StatusCodes.Status400BadRequest);
            }
            ctx.ExceptionHandled = true;
            return Task.CompletedTask;
        }

        if (isAjax)
        {
            ctx.Result = new JsonResult(ApiResult<object>.Fail("服务器内部错误，请稍后重试", 500));
        }
        else
        {
            SetErrorView(ctx, ex, StatusCodes.Status500InternalServerError);
        }
        ctx.ExceptionHandled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 直接渲染独立的错误视图（~/Views/Home/Error.cshtml），不发起任何重定向。
    /// </summary>
    private static void SetErrorView(ExceptionContext ctx, Exception ex, int statusCode)
    {
        var vd = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            ["Message"] = ex.Message,
            // 真实异常类型 + 堆栈，便于 docker 部署排障时直接截图查看根因
            ["ErrorDetail"] = $"{ex.GetType().FullName}: {ex.Message}\n\n{ex.StackTrace}"
        };
        ctx.Result = new ViewResult
        {
            ViewName = "~/Views/Home/Error.cshtml",
            StatusCode = statusCode,
            ViewData = vd
        };
    }
}

// ── 统一模型校验过滤器 ─────────────────────────────────────
// 配合 FluentValidation（及 DataAnnotations）使用：当模型绑定后 ModelState 不合法时，
// 对 API / Ajax 请求统一返回 ApiResult.Fail（携带全部错误信息）；
// 对表单提交（返回 View）不拦截，交由视图层展示校验信息。
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ValidateModelAttribute : TypeFilterAttribute
{
    public ValidateModelAttribute() : base(typeof(ValidationFilter)) { }
}

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        if (ctx.ModelState.IsValid)
        {
            await next();
            return;
        }

        var isAjax = ctx.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                  || ctx.HttpContext.Request.ContentType?.Contains("application/json") == true;
        if (!isAjax)
        {
            // 表单提交：交还 action / 视图处理校验信息
            await next();
            return;
        }

        var errors = ctx.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        ctx.Result = new JsonResult(ApiResult<object>.Fail(
            errors.Count > 0 ? string.Join("；", errors) : "请求参数校验失败"));
    }
}
