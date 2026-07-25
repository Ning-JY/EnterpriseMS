namespace EnterpriseMS.Services.DTOs.Report;

public class TemplateInfoDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Description { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public List<TemplateFieldDto> Fields { get; set; } = new();
}

public class TemplateFieldDto
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Required { get; set; }
    public string Type { get; set; } = "text";
    public string Source { get; set; } = "manual";
    public string? Binding { get; set; }
    public string? ConfigKey { get; set; }
    public string? DefaultValue { get; set; }
    public string? HelpText { get; set; }
    public List<OptionItemDto>? Options { get; set; }
}

public class OptionItemDto
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
}

public class TemplatePlaceholderDto
{
    public string Name { get; set; } = "";
    public int ParagraphIndex { get; set; }
    public string Context { get; set; } = "";
    public string SurroundingText { get; set; } = "";
}

public class ConfigureTemplateRequest
{
    public string TemplateName { get; set; } = "";
    public string TemplateDescription { get; set; } = "";
    public List<ReplacementItem> Replacements { get; set; } = new();
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
}

public class ReportFillRequest
{
    public string TemplateId { get; set; } = "";
    public Dictionary<string, string> SupplementaryFields { get; set; } = new();
    public List<MappedExcelColumnDto> ExcelColumns { get; set; } = new();
    public List<Dictionary<string, string>> ExcelRows { get; set; } = new();
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
