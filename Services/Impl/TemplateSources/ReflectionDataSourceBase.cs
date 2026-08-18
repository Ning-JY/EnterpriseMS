using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>
/// 基于实体反射的通用数据源基类：Schema 的键即实体属性名，ResolveAsync 按属性名取字符串值。
/// 新增一个实体数据源只需继承本类并声明 Schema / GetInstance / ListInstancesAsync。
/// </summary>
public abstract class ReflectionDataSourceBase : ITemplateDataSource
{
    protected readonly AppDbContext Db;
    protected ReflectionDataSourceBase(AppDbContext db) => Db = db;

    public abstract string SourceId { get; }
    public abstract string DisplayName { get; }
    protected abstract Dictionary<string, string> Schema { get; }
    protected abstract object? GetInstance(long id);

    public Dictionary<string, string> GetFieldSchema() => Schema;

    public virtual Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Task.FromResult(new List<DataSourceInstance>());

    public Task<Dictionary<string, string>> ResolveAsync(string? instanceId)
    {
        var dict = new Dictionary<string, string>();
        if (!long.TryParse(instanceId, out var id))
            return Task.FromResult(dict);
        var inst = GetInstance(id);
        if (inst == null)
            return Task.FromResult(dict);

        foreach (var kv in Schema)
        {
            var prop = inst.GetType().GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance);
            var val = prop?.GetValue(inst);
            dict[kv.Key] = val == null
                ? ""
                : (val is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : val.ToString() ?? "");
        }
        return Task.FromResult(dict);
    }
}
