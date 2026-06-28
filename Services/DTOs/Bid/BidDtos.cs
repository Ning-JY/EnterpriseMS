using EnterpriseMS.Domain.Enums;

namespace EnterpriseMS.Services.DTOs.Bid;

public class BidProjectDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public string ProjectCode { get; set; } = "";
    public string? Tenderer { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>解析阶段：0未解析 1解析中 2待人工确认 3已确认。前端据此渲染解析卡点UI。</summary>
    public int ParseStage { get; set; }
    public string? ParseStageName { get; set; }
    public string? FormatRuleJson { get; set; }
    public string? SourceFileName { get; set; }
    public DateTime? ElementsConfirmedAt { get; set; }
    public string? ElementsConfirmedBy { get; set; }

    public List<BidRequirementDto> Requirements { get; set; } = new();
    public List<BidDocumentDto> Documents { get; set; } = new();
}

public class BidProjectCreateDto
{
    public long ProjectId { get; set; }
    public string? Tenderer { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Remark { get; set; }
}

public class BidProjectUpdateDto
{
    public string? Tenderer { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public int? Status { get; set; }
    public string? Remark { get; set; }
}

public class BidRequirementDto
{
    public long Id { get; set; }
    public string Category { get; set; } = "";
    public string Content { get; set; } = "";
    public int? ScoreWeight { get; set; }
    public string? Description { get; set; }
    public bool IsVeto { get; set; }
    public string? SourceRef { get; set; }
    public bool NeedsReview { get; set; }
}

public class BidDocumentDto
{
    public long Id { get; set; }
    public string ChapterName { get; set; } = "";
    public int ChapterType { get; set; }
    public string? Content { get; set; }
    public int SortOrder { get; set; }
    public int Status { get; set; }
    public int? WordCount { get; set; }
}

public class BidAnalyzeRequest
{
    public long BidProjectId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class BidGenerateRequest
{
    public long BidProjectId { get; set; }
    public string ChapterName { get; set; } = "";
    public int ChapterType { get; set; } = 0;
    public int TargetWordCount { get; set; } = 2000;
    public string? CustomRequirements { get; set; }
}

public class BidGenerateFullRequest
{
    public long BidProjectId { get; set; }
    public List<string>? Chapters { get; set; }
    public int TargetWordCount { get; set; } = 2000;
}

public class BidReviewRequest
{
    public long BidProjectId { get; set; }
}

public class BidAssembleRequest
{
    public long BidProjectId { get; set; }
    public string Part { get; set; } = "all"; // all, technical, commercial
}

public class BidAssembleResult
{
    public string ProjectName { get; set; } = "";
    public string AssembleTime { get; set; } = "";
    public BidAssemblePart? TechnicalPart { get; set; }
    public BidAssemblePart? CommercialPart { get; set; }
    public BidAssemblePart? FullDocument { get; set; }
}

public class BidAssemblePart
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int WordCount { get; set; }
    public List<BidAssembleChapter> Chapters { get; set; } = new();
}

public class BidAssembleChapter
{
    public string Name { get; set; } = "";
    public string Content { get; set; } = "";
    public int WordCount { get; set; }
}

public class BidExportResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "";
    /// <summary>导出过程中的非阻断性提示（如预估页数超限、未识别到明确格式要求），文件仍会正常生成。</summary>
    public List<string> Warnings { get; set; } = new();
}

public class BidListQuery
{
    public long? ProjectId { get; set; }
    public int? Status { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
