namespace EnterpriseMS.Services.AI.Models;

public class BidAnalysisResult
{
    public string ProjectName { get; set; } = "";
    public string ProjectCode { get; set; } = "";
    public string? Tenderer { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? Deadline { get; set; }
    public List<string> Qualifications { get; set; } = new();
    public List<string> TechnicalRequirements { get; set; } = new();
    public List<string> CommercialRequirements { get; set; } = new();
    public List<ScoringCriterion> ScoringCriteria { get; set; } = new();
    public List<string> BidDocuments { get; set; } = new();
    public List<string> SpecialNotes { get; set; } = new();
}

public class ScoringCriterion
{
    public string Item { get; set; } = "";
    public int MaxScore { get; set; }
    public string? Description { get; set; }
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
