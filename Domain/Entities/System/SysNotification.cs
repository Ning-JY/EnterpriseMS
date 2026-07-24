using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.System;

[Table("sys_notification")]

/// <summary>
/// 站内通知 / 提醒中心实体。
/// 已读状态持久化（IsRead / ReadAt），可见性按 RequiredPerm + RecipientType 过滤，
/// 便于任意业务通过 PushAsync 主动推送（如审批、公告等），也可由 SyncExpiry 自动聚合到期提醒。
/// </summary>
public class SysNotification : BaseEntity
{
    /// <summary>类型：cert_expiry / contract_expiry / custom 等，用于区分图标与来源</summary>
    public string Type { get; set; } = "";

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Link { get; set; }

    /// <summary>级别：info / warning / danger / success</summary>
    public string Level { get; set; } = "info";

    /// <summary>可见所需权限码；null 表示所有登录用户可见</summary>
    public string? RequiredPerm { get; set; }

    /// <summary>受众类型：all / role / user（当前统一 all，预留按角色/用户定向推送）</summary>
    public string RecipientType { get; set; } = "all";

    public long? RecipientId { get; set; }

    /// <summary>幂等键（如 cert_expiry:{Id}），SyncExpiry 用于更新而非重复插入</summary>
    public string? SourceKey { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
