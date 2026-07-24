using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>系统参数设置服务实现</summary>
public class ConfigService : IConfigService
{
    private readonly AppDbContext _db;
    public ConfigService(AppDbContext db) => _db = db;

    public async Task<List<SysConfig>> GetAllAsync()
        => await _db.SysConfigs.OrderBy(c => c.GroupName).ThenBy(c => c.Sort).ToListAsync();

    public async Task SaveAsync(List<SysConfigDto> configs)
    {
        foreach (var dto in configs)
        {
            var existing = await _db.SysConfigs.FirstOrDefaultAsync(c => c.ConfigKey == dto.ConfigKey);
            if (existing != null)
            {
                existing.ConfigValue = dto.ConfigValue;
            }
            else
            {
                _db.SysConfigs.Add(new SysConfig
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
        await _db.SaveChangesAsync();
    }
}
