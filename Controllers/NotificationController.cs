using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

/// <summary>
/// 通知中心。铃铛展示在页眉（ViewComponent），本控制器提供"查看全部"列表页与标记已读接口。
/// 通知属于个人提醒，所有已登录用户均可访问（不附加额外权限码），匿名用户重定向登录。
/// </summary>
public class NotificationController : BaseAuthController
{
    private readonly INotificationService _notifSvc;

    public NotificationController(IPermissionService permSvc, INotificationService notifSvc)
        : base(permSvc)
    {
        _notifSvc = notifSvc;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Account", new { returnUrl = "/notifications" });

        var summary = await _notifSvc.GetForUserAsync(userId, 500);
        return View(summary);
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(long id)
    {
        var userId = User.GetUserId();
        if (userId == 0) return Json(ApiResult<object>.Unauthorized());

        await _notifSvc.MarkReadAsync(id, userId);
        return Json(ApiResult.Ok("已标记已读"));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.GetUserId();
        if (userId == 0) return Json(ApiResult<object>.Unauthorized());

        await _notifSvc.MarkAllReadAsync(userId);
        return Json(ApiResult.Ok("已全部标记为已读"));
    }
}
