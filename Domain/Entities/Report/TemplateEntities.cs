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

/// <summary>模板使用留痕（用章申请/报告生成等）：谁、何时、用哪个模板、填了什么值、生成了什么文件。
/// Status 预留审批流扩展（done=已完成，pending=待审批…），当前阶段仅记录 done。</summary>
public class TemplateUsageRecord
{
    public long Id { get; set; }
    public string TemplateId { get; set; } = "";
    public string TemplateName { get; set; } = "";
    /// <summary>模板分类（用章申请/报告/请假…），与模板 Category 对应。</summary>
    public string Category { get; set; } = "";
    public long? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public long? ApplicantId { get; set; }
    public string? ApplicantName { get; set; }
    /// <summary>填充字段快照（JSON：字段名 → 值）。</summary>
    public string? FieldSnapshot { get; set; }
    public string? FileName { get; set; }
    /// <summary>状态：done=已完成（预留 pending=待审批等扩展）。</summary>
    public string Status { get; set; } = "done";
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}
