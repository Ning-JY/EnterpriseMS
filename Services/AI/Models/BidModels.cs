namespace EnterpriseMS.Services.AI.Models;

public class BidAnalysisResult
{
    public string ProjectName { get; set; } = "";
    public string ProjectCode { get; set; } = "";
    public string? Tenderer { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }

    /// <summary>资格性条款，每条标注是否为否决项及原文出处。</summary>
    public List<QualificationItem> Qualifications { get; set; } = new();
    public List<RequirementItem> TechnicalRequirements { get; set; } = new();
    public List<RequirementItem> CommercialRequirements { get; set; } = new();
    public List<ScoringCriterion> ScoringCriteria { get; set; } = new();
    public List<string> BidDocuments { get; set; } = new();
    public List<string> SpecialNotes { get; set; } = new();

    /// <summary>格式要求（字体字号、页数限制、装订方式），供排版导出模块读取。</summary>
    public FormatRule? FormatRule { get; set; }

    /// <summary>AI抽取后未能在原文定位到出处的条目描述，前端展示为"待人工确认"清单，不阻塞流程但必须可见。</summary>
    public List<string> NeedsReview { get; set; } = new();
}

/// <summary>资格性审查条款。is_veto=true 表示"不满足即否决"，必须与普通资质要求分开展示。</summary>
public class QualificationItem
{
    public string Content { get; set; } = "";
    public bool IsVeto { get; set; } = false;
    /// <summary>原文出处：PDF用页码（如"p.14"），Word文档用章节/段落定位（如"投标人须知 §3.2"）。无法定位时留空，并应同时出现在 NeedsReview 中。</summary>
    public string? SourceRef { get; set; }
}

public class RequirementItem
{
    public string Content { get; set; } = "";
    public string? SourceRef { get; set; }
}

public class ScoringCriterion
{
    public string Item { get; set; } = "";
    public int MaxScore { get; set; }
    public string? Description { get; set; }
    public string? SourceRef { get; set; }
}

/// <summary>排版格式要求，来自投标人须知章节，供排版导出模块套用模板时使用。</summary>
public class FormatRule
{
    public string? Font { get; set; }
    public int? PageLimit { get; set; }
    public string? Binding { get; set; }
    public string? SourceRef { get; set; }
}

public class BidChapterRequest
{
    public string ChapterName { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string? ProjectType { get; set; }
    public string? ProjectDescription { get; set; }
    public string? Requirements { get; set; }
    public List<string> ScoringCriteria { get; set; } = new();
    public string? CompanyInfo { get; set; }
    public string? TemplateContent { get; set; }
    public string? ReferenceContent { get; set; }
    public int TargetWordCount { get; set; } = 2000;
    public string? CustomRequirements { get; set; }
}

public class BidReviewResult
{
    public int OverallScore { get; set; }
    public bool IsComplete { get; set; }
    public List<BidReviewIssue> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public List<string> MissingItems { get; set; } = new();
}

public class BidReviewIssue
{
    public string Chapter { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Suggestion { get; set; }
}
