using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>系统参数设置服务实现</summary>
public class ConfigService : IConfigService
{
    private readonly IUnitOfWork _uow;
    public ConfigService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SysConfig>> GetAllAsync()
        => await _uow.SysConfigs.Query().OrderBy(c => c.GroupName).ThenBy(c => c.Sort).ToListAsync();

    public async Task SaveAsync(List<SysConfigDto> configs)
    {
        foreach (var dto in configs)
        {
            var existing = await _uow.SysConfigs.Query().FirstOrDefaultAsync(c => c.ConfigKey == dto.ConfigKey);
            if (existing != null)
            {
                existing.ConfigValue = dto.ConfigValue;
            }
            else
            {
                await _uow.SysConfigs.AddAsync(new SysConfig
                {
                    Id = SnowflakeId.Next(),
                    ConfigKey = dto.ConfigKey,
                    ConfigValue = dto.ConfigValue,
                    ConfigType = dto.ConfigType ?? "text",
                    GroupName = dto.GroupName ?? "system",
                    Sort = dto.Sort
                });
            }
        }
        await _uow.SaveChangesAsync();
    }
}
