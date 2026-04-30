using System.ComponentModel.DataAnnotations;

namespace EnterpriseMS.Services.DTOs.Kb;

public class KbFileDto
{
    public long    Id            { get; set; }
    public long    CategoryId    { get; set; }
    public string  CategoryName  { get; set; } = "";
    public string  FileName      { get; set; } = "";
    public string  OriginalName  { get; set; } = "";
    public long    FileSize      { get; set; }
    public string? FileExt       { get; set; }
    public string? Description   { get; set; }
    public string? Version       { get; set; }
    public int     DownloadCount { get; set; }
    public bool    IsPinned      { get; set; }
    public int     Status        { get; set; }
    public string  CreatedBy     { get; set; } = "";
    public DateTime CreatedAt    { get; set; }

    public string FileSizeText => FileSize < 1024 ? $"{FileSize}B"
        : FileSize < 1048576 ? $"{FileSize / 1024.0:N1}KB" : $"{FileSize / 1048576.0:N1}MB";

    public string ExtIcon => (FileExt ?? "").ToLower() switch
    {
        "pdf"                   => "fa-file-pdf text-danger",
        "doc" or "docx"         => "fa-file-word text-primary",
        "xls" or "xlsx"         => "fa-file-excel text-success",
        "ppt" or "pptx"         => "fa-file-powerpoint text-warning",
        "jpg" or "jpeg" or "png"=> "fa-file-image text-info",
        "zip" or "rar"          => "fa-file-archive text-secondary",
        _                       => "fa-file text-muted"
    };

    public bool CanPreview => new[] { "pdf", "jpg", "jpeg", "png" }
        .Contains((FileExt ?? "").ToLower());
}

public class KbQueryDto
{
    public long?  CategoryId { get; set; }
    public string? Keyword   { get; set; }
    public int?   Status     { get; set; }
    public int    Page       { get; set; } = 1;
    public int    Size       { get; set; } = 20;
}

public class KbUploadDto
{
    [Required(ErrorMessage = "请选择文件")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "请选择分类")]
    public long CategoryId { get; set; }

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Version { get; set; }

    public bool IsPinned { get; set; }
}

public class KbUpdateDto
{
    [Required]
    public long Id { get; set; }

    [Required(ErrorMessage = "文件名称不能为空")]
    [MaxLength(200)]
    public string FileName { get; set; } = "";

    [Required(ErrorMessage = "请选择分类")]
    public long CategoryId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Version { get; set; }

    public bool IsPinned { get; set; }
    public int Status { get; set; } = 1;
}

public class KbCategoryDto
{
    public long   Id          { get; set; }
    public string Name        { get; set; } = "";
    public string? Icon       { get; set; }
    public string? Description{ get; set; }
    public int    Sort        { get; set; }
    public int    Status      { get; set; }
    public int    FileCount   { get; set; }
}
