using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>手动填写数据源：无绑定字段、无实例，仅作为字段来源选项之一。</summary>
public class ManualDataSource : ITemplateDataSource
{
    public string SourceId => "manual";
    public string DisplayName => "手动填写";
    public Dictionary<string, string> GetFieldSchema() => new();
    public Task<List<DataSourceInstance>> ListInstancesAsync() => Task.FromResult(new List<DataSourceInstance>());
    public Task<Dictionary<string, string>> ResolveAsync(string? instanceId) =>
        Task.FromResult(new Dictionary<string, string>());
}
