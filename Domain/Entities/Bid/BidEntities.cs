using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.Bid;

[Table("bid_project")]
public class BidProject : BaseEntity
{
    [Column("project_id")]     public long?     ProjectId   { get; set; }
    [Column("project_name")]   public string    ProjectName { get; set; } = "";
    [Column("project_code")]   public string    ProjectCode { get; set; } = "";
    [Column("tenderer")]       public string?   Tenderer    { get; set; }
    [Column("budget")]         public decimal?  Budget      { get; set; }
    [Column("deadline")]       public DateTime? Deadline    { get; set; }
    [Column("status")]         public int       Status      { get; set; } = 0;
    [Column("remark")]         public string?   Remark      { get; set; }

    /// <summary>解析阶段：0未解析 1解析中 2待确认 3已确认。与 Status(投标整体状态) 独立，专门驱动"招标文件解析"卡点。</summary>
    [Column("parse_stage")]        public int      ParseStage        { get; set; } = 0;
    /// <summary>格式要求：字体、页数限制、装订方式等，AI抽取后存为JSON，供排版导出模块读取。</summary>
    [Column("format_rule_json")]   public string?  FormatRuleJson    { get; set; }
    /// <summary>原始招标文件路径，便于复核时回看原文。</summary>
    [Column("source_file_path")]   public string?  SourceFilePath    { get; set; }
    [Column("source_file_name")]   public string?  SourceFileName    { get; set; }
    /// <summary>要素表人工确认时间/确认人，作为流程留痕，对应"人工确认要素表"卡点。</summary>
    [Column("elements_confirmed_at")] public DateTime? ElementsConfirmedAt { get; set; }
    [Column("elements_confirmed_by")] public string?   ElementsConfirmedBy { get; set; }

    public Entities.Project.Project? Project { get; set; }
    public ICollection<BidRequirement> Requirements { get; set; } = new List<BidRequirement>();
    public ICollection<BidDocument>    Documents    { get; set; } = new List<BidDocument>();
}

[Table("bid_requirement")]
public class BidRequirement : BaseEntity
{
    [Column("bid_project_id")] public long     BidProjectId { get; set; }
    [Column("category")]       public string   Category     { get; set; } = "";
    [Column("content")]        public string   Content      { get; set; } = "";
    [Column("score_weight")]   public int?     ScoreWeight  { get; set; }
    [Column("description")]    public string?  Description  { get; set; }

    /// <summary>是否为否决性条款（资格性审查中"不满足即否决"）。必须与其他评分/资质项分开展示，不能淹没在列表里。</summary>
    [Column("is_veto")]        public bool     IsVeto       { get; set; } = false;
    /// <summary>原文出处定位：PDF为页码（如 "p.14"），docx因无固定分页改为段落/章节定位（如 "投标人须知 §3.2"）。</summary>
    [Column("source_ref")]     public string?  SourceRef    { get; set; }
    /// <summary>AI抽取后未能在原文定位到出处，标记为待人工确认，不允许直接进入下游模块。</summary>
    [Column("needs_review")]   public bool     NeedsReview  { get; set; } = false;

    public BidProject? BidProject { get; set; }
}

[Table("bid_document")]
public class BidDocument : BaseEntity
{
    [Column("bid_project_id")] public long   BidProjectId { get; set; }
    [Column("chapter_name")]   public string ChapterName  { get; set; } = "";
    [Column("chapter_type")]   public int    ChapterType  { get; set; } = 0;
    [Column("content")]        public string? Content     { get; set; }
    [Column("sort_order")]     public int    SortOrder    { get; set; }
    [Column("status")]         public int    Status       { get; set; } = 0;
    [Column("word_count")]     public int?   WordCount    { get; set; }

    public BidProject? BidProject { get; set; }
}

[Table("bid_template")]
public class BidTemplate : BaseEntity
{
    [Column("name")]       public string  Name       { get; set; } = "";
    [Column("category")]   public string  Category   { get; set; } = "";
    [Column("industry")]   public string? Industry   { get; set; }
    [Column("content")]    public string  Content    { get; set; } = "";
    [Column("is_default")] public bool    IsDefault  { get; set; }
}
