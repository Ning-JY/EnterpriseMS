using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.Bid;

[Table("bid_project")]
public class BidProject : BaseEntity
{
    [Column("project_id")]     public long      ProjectId   { get; set; }
    [Column("project_name")]   public string    ProjectName { get; set; } = "";
    [Column("project_code")]   public string    ProjectCode { get; set; } = "";
    [Column("tenderer")]       public string?   Tenderer    { get; set; }
    [Column("budget")]         public decimal?  Budget      { get; set; }
    [Column("deadline")]       public DateTime? Deadline    { get; set; }
    [Column("status")]         public int       Status      { get; set; } = 0;
    [Column("remark")]         public string?   Remark      { get; set; }

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
