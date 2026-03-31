using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.Info;

[Table("info_category")]
public class InfoCategory : BaseEntity
{
    [Column("category_name")] public string  CategoryName { get; set; } = "";
    [Column("parent_id")]     public long    ParentId     { get; set; }
    [Column("is_public")]     public int     IsPublic     { get; set; } = 0;
    [Column("sort")]          public int     Sort         { get; set; }
    [Column("status")]        public int     Status       { get; set; } = 1;
    public ICollection<InfoArticle> Articles { get; set; } = new List<InfoArticle>();
}

[Table("info_article")]
public class InfoArticle : BaseEntity
{
    [Column("category_id")]  public long     CategoryId  { get; set; }
    [Column("title")]        public string   Title       { get; set; } = "";
    [Column("content")]      public string   Content     { get; set; } = "";
    [Column("cover_image")]  public string?  CoverImage  { get; set; }
    [Column("is_public")]    public int      IsPublic    { get; set; } = 0;
    [Column("is_top")]       public int      IsTop       { get; set; } = 0;
    [Column("status")]       public int      Status      { get; set; } = 0;
    [Column("publish_time")] public DateTime? PublishTime { get; set; }
    [Column("view_count")]   public int      ViewCount   { get; set; }
    public InfoCategory? Category { get; set; }
}

/// <summary>
/// 知识库文件（模板/通知/规范/制度等，所有登录用户可见）
/// </summary>
[Table("kb_file")]
public class KbFile : BaseEntity
{
    [Column("category_id")]   public long    CategoryId  { get; set; }  // 分类ID
    [Column("file_name")]     public string  FileName    { get; set; } = ""; // 显示名称
    [Column("original_name")] public string  OriginalName{ get; set; } = ""; // 原始文件名
    [Column("file_path")]     public string  FilePath    { get; set; } = ""; // 服务器路径
    [Column("file_size")]     public long    FileSize    { get; set; }
    [Column("file_ext")]      public string? FileExt     { get; set; }  // pdf/docx/xlsx等
    [Column("description")]   public string? Description { get; set; }  // 说明
    [Column("version")]       public string? Version     { get; set; }  // 版本号
    [Column("download_count")]public int     DownloadCount{ get; set; } // 下载次数
    [Column("is_pinned")]     public bool    IsPinned    { get; set; }  // 置顶
    [Column("status")]        public int     Status      { get; set; } = 1; // 1启用0禁用
    public KbCategory? Category { get; set; }
}

/// <summary>
/// 知识库分类（模板文件/公司通知/行业规范/规章制度/其他）
/// </summary>
[Table("kb_category")]
public class KbCategory : BaseEntity
{
    [Column("name")]         public string  Name        { get; set; } = "";
    [Column("icon")]         public string? Icon        { get; set; } = "fa-folder";
    [Column("description")]  public string? Description { get; set; }
    [Column("sort")]         public int     Sort         { get; set; }
    [Column("status")]       public int     Status       { get; set; } = 1;
    public ICollection<KbFile> Files { get; set; } = new List<KbFile>();
}
