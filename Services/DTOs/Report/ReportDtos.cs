using System.Text.Json.Serialization;

namespace EnterpriseMS.Services.DTOs.Report;

public class TemplateInfoDto
{
    [JsonPropertyName("id")]         public string Id { get; set; } = "";
    [JsonPropertyName("name")]       public string Name { get; set; } = "";
    [JsonPropertyName("fileName")]   public string FileName { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("createdAt")]  public string CreatedAt { get; set; } = "";
    [JsonPropertyName("contextSource")] public string? ContextSource { get; set; }
    [JsonPropertyName("category")]   public string? Category { get; set; }
    [JsonPropertyName("fields")]     public List<TemplateFieldDto> Fields { get; set; } = new();
}

public class TemplateFieldDto
{
    [JsonPropertyName("name")]        public string Name { get; set; } = "";
    [JsonPropertyName("label")]       public string Label { get; set; } = "";
    [JsonPropertyName("required")]    public bool Required { get; set; }
    [JsonPropertyName("type")]        public string Type { get; set; } = "text";
    [JsonPropertyName("source")]      public string Source { get; set; } = "manual";
    [JsonPropertyName("binding")]     public string? Binding { get; set; }
    [JsonPropertyName("configKey")]   public string? ConfigKey { get; set; }
    [JsonPropertyName("defaultValue")] public string? DefaultValue { get; set; }
    [JsonPropertyName("helpText")]    public string? HelpText { get; set; }
    [JsonPropertyName("options")]     public List<OptionItemDto>? Options { get; set; }
}

public class OptionItemDto
{
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
}

public class TemplatePlaceholderDto
{
    [JsonPropertyName("name")]            public string Name { get; set; } = "";
    [JsonPropertyName("paragraphIndex")]  public int ParagraphIndex { get; set; }
    [JsonPropertyName("context")]         public string Context { get; set; } = "";
    [JsonPropertyName("surroundingText")] public string SurroundingText { get; set; } = "";
}

public class ConfigureTemplateRequest
{
    /// <summary>编辑已有模板时传入其 Id；为空表示新建。</summary>
    public string TemplateId { get; set; } = "";
    public string TemplateName { get; set; } = "";
    public string TemplateDescription { get; set; } = "";
    /// <summary>模板分类（如 报告/合同/证书），用于列表筛选。</summary>
    public string Category { get; set; } = "";
    public List<ReplacementItem> Replacements { get; set; } = new();
    /// <summary>原始段落模板文件（步骤3保存时重新上传，或复用步骤1的）</summary>
    public byte[]? TemplateBytes { get; set; }
}

public class DeleteTemplateRequest
{
    public string TemplateId { get; set; } = "";
}

public class ReplacementItem
{
    public string OldText { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string FieldLabel { get; set; } = "";
    /// <summary>字段是否必填（配置端收集，填充端用于校验）。</summary>
    public bool Required { get; set; } = true;
    /// <summary>字段输入类型：text/date/select/textarea/image，影响填充端渲染。</summary>
    public string Type { get; set; } = "text";
    /// <summary>替换方式：full=整段替换, inline=段落内文字替换, table-row=表格行替换（仅影响 docx 正文替换，不影响字段数据来源）</summary>
    public string Source { get; set; } = "full";
    /// <summary>字段数据来源：manual=手填, project=绑定项目字段, config=系统配置</summary>
    public string FieldSource { get; set; } = "manual";
    /// <summary>当 FieldSource=project 时，绑定的项目实体属性名（如 ProjName）</summary>
    public string? Binding { get; set; }
    /// <summary>当 FieldSource=config 时，对应的系统配置 ConfigKey</summary>
    public string? ConfigKey { get; set; }
    /// <summary>对于 inline 类型：OldText 中被替换文字的起始位置（字符索引）</summary>
    public int StartIndex { get; set; }
    /// <summary>对于 inline 类型：OldText 中被替换文字的结束位置（字符索引）</summary>
    public int EndIndex { get; set; }
    /// <summary>字段默认值（手动来源时预填）</summary>
    public string? DefaultValue { get; set; }
    /// <summary>字段填写提示</summary>
    public string? HelpText { get; set; }
}

public class ReportFillRequest
{
    public string TemplateId { get; set; } = "";
    /// <summary>数据上下文来源（project/employee/projcontract/employeecontract），与模板 ContextSource 对应。</summary>
    public string ContextSource { get; set; } = "";
    /// <summary>所选数据上下文实例 Id（如项目 Id），用于通用填充解析绑定字段。</summary>
    public string InstanceId { get; set; } = "";
    public Dictionary<string, object> SupplementaryFields { get; set; } = new();
    public List<MappedExcelColumnDto> ExcelColumns { get; set; } = new();
    public List<Dictionary<string, object>> ExcelRows { get; set; } = new();
    /// <summary>列表字段：字段名 → 行数据列表(每行一个字典)。模板中该字段所在表格行会被 MiniWord 克隆重复。</summary>
    public Dictionary<string, List<Dictionary<string, object>>> ListFields { get; set; } = new();
    /// <summary>图片字段：字段名 → 图片值。模板中 {{字段}} 处插入图片。</summary>
    public Dictionary<string, ImageFieldValue> ImageFields { get; set; } = new();
}

/// <summary>图片字段值：Base64 或服务器路径二选一。</summary>
public class ImageFieldValue
{
    public string? Base64 { get; set; }
    public string? Path { get; set; }
    public int Width { get; set; } = 200;
    public int Height { get; set; } = 200;
}

// ── 项目快捷生成报告：POST 请求体 ──
public class ReportGenerateFromProjectRequest
{
    public string TemplateId { get; set; } = "";
    public Dictionary<string, string> Fields { get; set; } = new();
}

public class MappedExcelColumnDto
{
    public string FieldName { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public int ColumnIndex { get; set; }
}

// ── 报表（回款 / 产值）结果 DTO：承载视图所需的全部数据 ──
public class ReceiptReportDto
{
    public List<EnterpriseMS.Domain.Entities.System.SysDept> Depts { get; set; } = new();
    public int? Year { get; set; }
    public long? DeptId { get; set; }
    public string? Keyword { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalPending { get; set; }
    public List<ReceiptByDeptDto> ByDept { get; set; } = new();
    public List<ReceiptByMonthDto> ByMonth { get; set; } = new();
    public List<EnterpriseMS.Domain.Entities.Project.ProjectInvoice> Invoices { get; set; } = new();
}

public class ReceiptByDeptDto
{
    public string DeptName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public int ReceivedCount { get; set; }
    public int TotalCount { get; set; }
}

public class ReceiptByMonthDto
{
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

public class OutputReportDto
{
    public List<EnterpriseMS.Domain.Entities.System.SysDept> Depts { get; set; } = new();
    public int? Year { get; set; }
    public long? DeptId { get; set; }
    public string? Keyword { get; set; }
    public decimal TotalContract { get; set; }
    public decimal TotalReceived { get; set; }
    public List<OutputByDeptDto> ByDept { get; set; } = new();
    public List<OutputEmployeeRowDto> Employees { get; set; } = new();
}

public class OutputByDeptDto
{
    public string DeptName { get; set; } = "";
    public int EmpCount { get; set; }
    public decimal ContractValue { get; set; }
    public decimal ReceivedValue { get; set; }
}

public class OutputEmployeeRowDto
{
    public long EmpId { get; set; }
    public string EmpName { get; set; } = "";
    public string DeptName { get; set; } = "";
    public int ProjectCount { get; set; }
    public decimal ContractValue { get; set; }
    public decimal ReceivedValue { get; set; }
    public List<EnterpriseMS.Domain.Entities.Project.ProjectMember> Projects { get; set; } = new();
}
