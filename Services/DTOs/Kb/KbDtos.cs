using System.ComponentModel.DataAnnotations;

namespace EnterpriseMS.Services.DTOs.Kb;

public class KbQueryDto
{
    public long? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
}

public class KbFileDto
{
    public long Id { get; set; }
    public string FileName { get; set; } = "";
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string OriginalName { get; set; } = "";
    public long FileSize { get; set; }
    public string FileSizeText { get; set; } = "";
    public string FileExt { get; set; } = "";
    public string ExtIcon => FileExt.ToLower() switch
    {
        "pdf" => "fa-file-pdf",
        "doc" or "docx" => "fa-file-word",
        "xls" or "xlsx" => "fa-file-excel",
        "ppt" or "pptx" => "fa-file-powerpoint",
        "jpg" or "jpeg" or "png" or "gif" or "bmp" => "fa-file-image",
        "zip" or "rar" or "7z" => "fa-file-archive",
        "mp4" or "avi" or "mov" => "fa-file-video",
        "mp3" or "wav" => "fa-file-audio",
        "txt" or "md" => "fa-file-alt",
        _ => "fa-file"
    };
    public string? Description { get; set; }
    public string? Version { get; set; }
    public int DownloadCount { get; set; }
    public bool IsPinned { get; set; }
    public int Status { get; set; }
    public bool CanPreview { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class KbUploadDto
{
    [Required(ErrorMessage = "请选择文件")]
    public IFormFile File { get; set; } = null!;
    public long? CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
}

public class KbUpdateDto
{
    public long Id { get; set; }
    public string? FileName { get; set; }
    public long? CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public int? Status { get; set; }
}

public class KbCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public int FileCount { get; set; }
}
