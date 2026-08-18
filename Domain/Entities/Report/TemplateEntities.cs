namespace EnterpriseMS.Domain.Entities.Report;

/// <summary>模板定义（通用模板可视化配置的核心存储，替代原 template-manifest.json）。</summary>
public class TemplateDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Description { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    /// <summary>主数据上下文来源（project/employee/projcontract/employeecontract），填充向导据此选择实例；纯手动/配置模板为 null。</summary>
    public string? ContextSource { get; set; }
    /// <summary>模板分类（报告/合同/证书…），用于列表筛选与归类。</summary>
    public string? Category { get; set; }
    public List<TemplateField> Fields { get; set; } = new();
}

/// <summary>模板字段（含取值来源声明：source + binding/configKey）。</summary>
public class TemplateField
{
    public int Id { get; set; }
    public string TemplateId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Required { get; set; }
    public string Type { get; set; } = "text";
    public string Source { get; set; } = "manual";
    public string? Binding { get; set; }
    public string? ConfigKey { get; set; }
    public string? DefaultValue { get; set; }
    public string? HelpText { get; set; }
    public int Sort { get; set; }
}
