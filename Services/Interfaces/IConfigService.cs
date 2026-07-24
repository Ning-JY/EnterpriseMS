using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

/// <summary>系统参数设置服务（从 ConfigController 抽取，避免 Controller 直连 DbContext）</summary>
public interface IConfigService
{
    Task<List<SysConfig>> GetAllAsync();
    Task SaveAsync(List<SysConfigDto> configs);
}
