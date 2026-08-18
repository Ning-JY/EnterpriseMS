using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>系统配置数据源：字段键即 ConfigKey，ResolveAsync 返回 ConfigKey -> ConfigValue。</summary>
public class ConfigDataSource : ITemplateDataSource
{
    private readonly AppDbContext _db;
    public ConfigDataSource(AppDbContext db) => _db = db;

    public string SourceId => "config";
    public string DisplayName => "系统配置";

    public Dictionary<string, string> GetFieldSchema() =>
        _db.SysConfigs.ToDictionary(c => c.ConfigKey,
            c => string.IsNullOrEmpty(c.ConfigValue) ? c.ConfigKey : $"{c.ConfigKey}（{c.ConfigValue}）");

    public Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Task.FromResult(new List<DataSourceInstance>());

    public async Task<Dictionary<string, string>> ResolveAsync(string? instanceId) =>
        await _db.SysConfigs.ToDictionaryAsync(c => c.ConfigKey, c => c.ConfigValue ?? "");
}
