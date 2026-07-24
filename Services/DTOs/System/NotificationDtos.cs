namespace EnterpriseMS.Services.DTOs.System;

/// <summary>通知中心聚合结果（铃铛与列表页共用）</summary>
public class NotificationSummary
{
    public int UnreadCount { get; set; }
    public List<NotificationItem> Items { get; set; } = new();
}

/// <summary>单条通知视图模型</summary>
public class NotificationItem
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Link { get; set; }
    public string Level { get; set; } = "info";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>业务主动推送通知的入参</summary>
public class NotificationPushDto
{
    public string Type { get; set; } = "custom";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Link { get; set; }
    public string Level { get; set; } = "info";
    public string? RequiredPerm { get; set; }
    public string RecipientType { get; set; } = "all";
    public long? RecipientId { get; set; }
    public string? CreatedBy { get; set; }
}
