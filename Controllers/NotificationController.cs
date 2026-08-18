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
[Route("notification")]
public class NotificationController : BaseAuthController
{
    private readonly INotificationService _notifSvc;

    public NotificationController(IPermissionService permSvc, INotificationService notifSvc)
        : base(permSvc)
    {
        _notifSvc = notifSvc;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Account", new { returnUrl = "/notification" });

        var summary = await _notifSvc.GetForUserAsync(userId, 500);
        ViewBag.UnreadCount = summary.UnreadCount;
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(int page = 1, int size = 20)
    {
        var userId = User.GetUserId();
        if (userId == 0) return ApiFail("未授权", 401);

        var summary = await _notifSvc.GetForUserAsync(userId, 500);
        var items = summary.Items;
        var total = items.Count;
        var pagedItems = items.Skip((page - 1) * size).Take(size)
            .Select(i => new
            {
                i.Id, i.Title, i.Content, Link = i.Link ?? "", i.Level, i.IsRead, i.CreatedAt
            }).Cast<object>().ToList();

        var paged = new PagedResult<object> { Items = pagedItems, Total = total, Page = page, PageSize = size };
        return ApiOk(paged);
    }

    [HttpPost("mark-read/{id}")]
    public async Task<IActionResult> MarkRead(long id)
    {
        var userId = User.GetUserId();
        if (userId == 0) return ApiFail("未授权", 401);

        await _notifSvc.MarkReadAsync(id, userId);
        return ApiOk<object>(null!, "已标记已读");
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.GetUserId();
        if (userId == 0) return ApiFail("未授权", 401);

        await _notifSvc.MarkAllReadAsync(userId);
        return ApiOk<object>(null!, "已全部标记为已读");
    }
}
