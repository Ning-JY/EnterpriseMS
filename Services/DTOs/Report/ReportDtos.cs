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

public class MappedExcelColumnDto
{
    public string FieldName { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public int ColumnIndex { get; set; }
}
