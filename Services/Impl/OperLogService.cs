using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class OperLogService : IOperLogService
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpCtx;

    public OperLogService(IUnitOfWork uow, IHttpContextAccessor httpCtx)
    { _uow = uow; _httpCtx = httpCtx; }

    public async Task LogAsync(string title, string? content = null,
        string? businessType = null, long? businessId = null)
    {
        var ctx  = _httpCtx.HttpContext;
        var log  = new SysOperLog
        {
            Title        = title,
            Content      = content,
            BusinessType = businessType,
            BusinessId   = businessId,
            OperName     = ctx?.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            OperUrl      = ctx?.Request.Path,
            OperIp       = ctx?.Connection.RemoteIpAddress?.ToString(),
            Status       = 1,
            OperTime     = DateTime.UtcNow,
        };
        log.Id = SnowflakeId.Next();
        await _uow.SysOperLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }

    public async Task<PagedResult<SysOperLog>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.SysOperLogs.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(l => l.Title.Contains(keyword) ||
                             (l.OperName != null && l.OperName.Contains(keyword)));
        return await q.OrderByDescending(l => l.OperTime).ToPagedAsync(page, size);
    }
}
