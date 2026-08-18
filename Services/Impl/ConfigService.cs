using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
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

    public async Task<PagedResult<ConfigListDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.SysConfigs.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => c.ConfigKey.Contains(keyword) || c.ConfigValue.Contains(keyword)
                          || c.GroupName.Contains(keyword));
        var paged = await q.OrderBy(c => c.GroupName).ThenBy(c => c.Sort).ToPagedAsync(page, size);
        var items = paged.Items.Select(c => new ConfigListDto
        {
            Id = c.Id, GroupName = c.GroupName, ConfigKey = c.ConfigKey,
            ConfigValue = c.ConfigValue, ConfigType = c.ConfigType, Sort = c.Sort
        }).ToList();
        return new PagedResult<ConfigListDto>
        { Items = items, Total = paged.Total, Page = page, PageSize = size };
    }

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
