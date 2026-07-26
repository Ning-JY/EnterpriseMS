using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 通知中心服务实现。
/// - 铃铛/列表页共用 GetForUserAsync：按权限与受众过滤可见通知，并按用户独立计算已读状态。
/// - SyncExpiryAsync：幂等地把"证件60天内到期 / 合同30天内到期"聚合为全局通知行（已读状态按用户独立记录）。
/// - PushAsync：供任意业务主动推送通知（如审批、公告），可指定权限码与定向受众。
/// 可见性模型：RecipientType = all（全员可见）/ user（指定用户）/ role（指定角色）；
/// RequiredPerm 为空表示无门槛，否则需用户拥有该权限码才可见。
/// 已读状态统一由 SysNotificationRead 按用户记录，与通知主表解耦。
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork       _uow;
    private readonly IPermissionService _permSvc;
    private readonly IMemoryCache      _cache;

    public NotificationService(IUnitOfWork uow, IPermissionService permSvc, IMemoryCache cache)
    {
        _uow = uow;
        _permSvc = permSvc;
        _cache = cache;
    }

    // SyncExpiry 5 分钟缓存键，避免每次请求都重算写库
    private const string SyncCacheKey = "notif:sync:expiry";
    private static readonly TimeSpan SyncCacheTtl = TimeSpan.FromMinutes(5);

    public async Task<NotificationSummary> GetForUserAsync(long userId, int take = 20)
    {
        var visibleIds = await GetVisibleIdsAsync(userId);

        var list = await _uow.Notifications.Query()
            .Where(n => visibleIds.Contains(n.Id))
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

        var readSet = await GetReadSetAsync(userId);

        var items = list.Select(n => new NotificationItem
        {
            Id        = n.Id,
            Title     = n.Title,
            Content   = n.Content,
            Link      = n.Link,
            Level     = n.Level,
            CreatedAt = n.CreatedAt,
            IsRead    = readSet.Contains(n.Id),
        }).ToList();

        // 把最新公开公告作为 info 级提醒注入铃铛（不影响未读计数与已读状态）
        var announce = await GetAnnouncementItemsAsync();
        if (announce.Count > 0)
            items.InsertRange(0, announce);

        return new NotificationSummary
        {
            UnreadCount = visibleIds.Count(id => !readSet.Contains(id)),
            Items       = items,
        };
    }

    /// <summary>取最新公开公告，映射为 info 级通知项（Id 取负避免与真实通知主键冲突，始终视为已读）。</summary>
    private async Task<List<NotificationItem>> GetAnnouncementItemsAsync(int take = 5)
    {
        var list = await _uow.InfoArticles.Query()
            .Where(a => !a.IsDeleted && a.Status == 1 && a.IsPublic == 1)
            .OrderByDescending(a => a.IsTop)
            .ThenByDescending(a => a.PublishTime ?? a.CreatedAt)
            .Take(take).ToListAsync();
        return list.Select(a => new NotificationItem
        {
            Id        = -a.Id,
            Title     = a.Title,
            Content   = "公告 · " + (a.Category?.CategoryName ?? "资讯公告"),
            Link      = "/pub/Detail/" + a.Id,
            Level     = "info",
            CreatedAt = a.PublishTime ?? a.CreatedAt,
            IsRead    = true,
        }).ToList();
    }

    public async Task SyncExpiryAsync()
    {
        // 5 分钟内已同步过则跳过（降低写库频率）
        if (_cache.TryGetValue(SyncCacheKey, out _)) return;

        try
        {
            var today        = DateTime.UtcNow.Date;
            var certWarn     = today.AddDays(60);
            var contractWarn = today.AddDays(30);

            var certs = await _uow.Certificates.Query()
                .Include(c => c.Employee)
                .Where(c => c.Status == 0 && c.ExpireDate != null && c.ExpireDate <= certWarn)
                .ToListAsync();

            var contracts = await _uow.Contracts.Query()
                .Include(c => c.Employee)
                .Where(c => c.Status == 0 && c.EndDate <= contractWarn)
                .ToListAsync();

            var currentKeys = new List<string>();

            foreach (var c in certs)
            {
                var days = (c.ExpireDate!.Value - today).Days;
                var key  = $"cert_expiry:{c.Id}";
                currentKeys.Add(key);
                UpsertExpiry(key, "cert_expiry",
                    $"证书即将到期：{c.CertName}",
                    $"{c.Employee?.RealName ?? "未知员工"} 的「{c.CertName}」将于 {c.ExpireDate:yyyy-MM-dd} 到期（剩 {days} 天）",
                    "warning", "/hr/cert");
            }

            foreach (var c in contracts)
            {
                var days = (c.EndDate - today).Days;
                var key  = $"contract_expiry:{c.Id}";
                currentKeys.Add(key);
                UpsertExpiry(key, "contract_expiry",
                    $"合同即将到期：{c.ContractNo}",
                    $"{c.Employee?.RealName ?? "未知员工"} 的合同 {c.ContractNo} 将于 {c.EndDate:yyyy-MM-dd} 到期（剩 {days} 天）",
                    "danger", "/hr/contract");
            }

            // 清理已不再处于预警窗口的旧提醒（如证书已续期/合同已终止）
            var stale = await _uow.Notifications.Query()
                .Where(n => (n.Type == "cert_expiry" || n.Type == "contract_expiry")
                         && n.SourceKey != null && !currentKeys.Contains(n.SourceKey))
                .ToListAsync();
            foreach (var s in stale)
                _uow.Notifications.SoftDelete(s);

            await _uow.SaveChangesAsync();
        }
        finally
        {
            // 无论成功与否都写入缓存，避免异常时频繁重试写库
            _cache.Set(SyncCacheKey, true, SyncCacheTtl);
        }
    }

    public async Task MarkReadAsync(long id, long userId)
    {
        var already = await _uow.NotifReads.Query()
            .AnyAsync(r => r.UserId == userId && r.NotificationId == id);
        if (already) return;

        await _uow.NotifReads.AddAsync(new SysNotificationRead
        {
            UserId         = userId,
            NotificationId = id,
            ReadAt         = DateTime.UtcNow,
        });
        await _uow.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(long userId)
    {
        var visibleIds = await GetVisibleIdsAsync(userId);
        var readSet    = await GetReadSetAsync(userId);

        foreach (var nid in visibleIds)
        {
            if (readSet.Contains(nid)) continue;
            await _uow.NotifReads.AddAsync(new SysNotificationRead
            {
                UserId         = userId,
                NotificationId = nid,
                ReadAt         = DateTime.UtcNow,
            });
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<long> PushAsync(NotificationPushDto dto)
    {
        var n = new SysNotification
        {
            Type          = dto.Type,
            Title         = dto.Title,
            Content       = dto.Content,
            Link          = dto.Link,
            Level         = dto.Level,
            RequiredPerm  = dto.RequiredPerm,
            RecipientType = dto.RecipientType,
            RecipientId   = dto.RecipientId,
            SourceKey     = null,        // 主动推送不绑定幂等键
            CreatedBy     = dto.CreatedBy ?? "",
        };
        await _uow.Notifications.AddAsync(n);
        await _uow.SaveChangesAsync();
        return n.Id;
    }

    // ── 私有辅助 ──────────────────────────────────────────────

    /// <summary>计算当前用户可见的通知 Id 列表（权限 + 受众 + 全局未关闭）</summary>
    private async Task<List<long>> GetVisibleIdsAsync(long userId)
    {
        var perms   = await _permSvc.GetUserPermissionsAsync(userId);
        var roleIds = await _uow.UserRoles.Query()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        return await _uow.Notifications.Query()
            .Where(n => !n.IsRead) // 未被全局关闭
            .Where(n => n.RecipientType == "all"
                     || (n.RecipientType == "user" && n.RecipientId == userId)
                     || (n.RecipientType == "role" && n.RecipientId != null && roleIds.Contains(n.RecipientId.Value)))
            .Where(n => n.RequiredPerm == null || perms.Contains(n.RequiredPerm))
            .Select(n => n.Id)
            .ToListAsync();
    }

    /// <summary>获取当前用户已读的通知 Id 集合</summary>
    private async Task<HashSet<long>> GetReadSetAsync(long userId)
    {
        var ids = await _uow.NotifReads.Query()
            .Where(r => r.UserId == userId)
            .Select(r => r.NotificationId)
            .ToListAsync();
        return new HashSet<long>(ids);
    }

    /// <summary>按 SourceKey 幂等 upsert 一条全局到期提醒</summary>
    private void UpsertExpiry(string key, string type, string title, string content, string level, string link)
    {
        var existing = _uow.Notifications.Query().FirstOrDefault(n => n.SourceKey == key);
        if (existing == null)
        {
            _uow.Notifications.AddAsync(new SysNotification
            {
                Type          = type,
                Title         = title,
                Content       = content,
                Level         = level,
                Link          = link,
                RequiredPerm  = null,    // 全员可见
                RecipientType = "all",
                SourceKey     = key,
                CreatedBy     = "system",
            });
        }
        else
        {
            existing.Title   = title;
            existing.Content = content;
            existing.Link    = link;
            existing.Level   = level;
            existing.IsRead  = false;    // 重新进入"未处理"状态
            _uow.Notifications.Update(existing);
        }
    }
}
