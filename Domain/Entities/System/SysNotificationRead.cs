using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.System;

[Table("sys_notification_read")]

/// <summary>
/// 通知已读标记（按用户维度）。
/// 通知内容本身可全局展示（RecipientType=all），但每个登录用户的"已读"状态互相独立，
/// 由本表记录，避免在通知主表上用单一 IsRead 字段导致"一人已读、全员已读"的问题。
/// 标记已读 = 写入一行；"标为未读" = 软删除本行（全局软删除过滤器自动过滤）。
/// </summary>
public class SysNotificationRead : BaseEntity
{
    /// <summary>用户 Id（对应 sys_user.Id）</summary>
    public long UserId { get; set; }

    /// <summary>通知 Id（对应 sys_notification.Id）</summary>
    public long NotificationId { get; set; }

    public DateTime ReadAt { get; set; }
}
