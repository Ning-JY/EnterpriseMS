using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IOperLogService
{
    Task LogAsync(string title, string? content = null,
        string? businessType = null, long? businessId = null);

    /// <summary>分页查询操作日志（供 LogController 列表使用，避免 Controller 直连 DbContext）</summary>
    Task<PagedResult<SysOperLog>> GetPagedAsync(string? keyword, int page, int size);
}
