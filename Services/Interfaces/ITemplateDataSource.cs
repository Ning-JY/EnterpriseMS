using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseMS.Services.Interfaces;

/// <summary>通用模板数据源实例（填充向导下拉用）。</summary>
public class DataSourceInstance
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

/// <summary>
/// 通用模板数据源：任意实体 / 配置都可实现，经 DI 注册后由配置器与填充向导自动发现。
/// 配置器枚举所有实现得到「字段白名单」，填充向导按 source 派发 ResolveAsync 解析取值。
/// </summary>
public interface ITemplateDataSource
{
    /// <summary>数据源 id（与模板字段的 source 对应），如 project / employee / config。</summary>
    string SourceId { get; }

    /// <summary>展示名，如 "项目" / "员工" / "系统配置"。</summary>
    string DisplayName { get; }

    /// <summary>可绑定字段白名单：字段键 -> 中文标签（键须与实体属性名一致，用于反射取值）。</summary>
    Dictionary<string, string> GetFieldSchema();

    /// <summary>该数据源下的实例列表（填充向导下拉用）；无实例概念的数据源返回空列表。</summary>
    Task<List<DataSourceInstance>> ListInstancesAsync();

    /// <summary>
    /// 按实例 id 解析所有字段值（字段键 -> 值）；instanceId 为 null 时按数据源自身解析（如系统配置）。
    /// </summary>
    Task<Dictionary<string, string>> ResolveAsync(string? instanceId);
}
