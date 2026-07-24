using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class OperLogService : IOperLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpCtx;

    public OperLogService(AppDbContext db, IHttpContextAccessor httpCtx)
    { _db = db; _httpCtx = httpCtx; }

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
            OperTime     = DateTime.Now,
        };
        log.Id = EnterpriseMS.Common.SnowflakeId.Next();
        await _db.SysOperLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<SysOperLog>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _db.SysOperLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(l => l.Title.Contains(keyword) ||
                             (l.OperName != null && l.OperName.Contains(keyword)));
        return await q.OrderByDescending(l => l.OperTime).ToPagedAsync(page, size);
    }
}
