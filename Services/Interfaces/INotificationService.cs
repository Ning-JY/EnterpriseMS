using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// 获取当前用户可见通知（按权限/受众过滤）。take 控制返回条数：
    /// 铃铛用 20，列表页用更大值（如 500）。
    /// </summary>
    Task<NotificationSummary> GetForUserAsync(long userId, int take = 20);

    /// <summary>幂等同步证件/合同到期提醒到通知表（带 5 分钟缓存，避免频繁写库）</summary>
    Task SyncExpiryAsync();

    Task MarkReadAsync(long id, long userId);
    Task MarkAllReadAsync(long userId);

    /// <summary>业务主动推送一条通知（可指定权限/受众），返回新记录 Id</summary>
    Task<long> PushAsync(NotificationPushDto dto);
}
