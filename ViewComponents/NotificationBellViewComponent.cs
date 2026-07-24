using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.ViewComponents;

/// <summary>
/// 页眉通知铃铛。聚合当前登录用户可见的通知（证件/合同到期提醒等），
/// 渲染未读徽标与下拉列表。匿名用户返回空模型（铃铛不渲染内容）。
/// </summary>
public class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationService _notifSvc;

    public NotificationBellViewComponent(INotificationService notifSvc)
    {
        _notifSvc = notifSvc;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var summary = new NotificationSummary();
        if (User.Identity?.IsAuthenticated == true && User is System.Security.Claims.ClaimsPrincipal cp)
        {
            var userId = cp.GetUserId();
            if (userId != 0)
                summary = await _notifSvc.GetForUserAsync(userId, 20);
        }
        return View(summary);
    }
}
