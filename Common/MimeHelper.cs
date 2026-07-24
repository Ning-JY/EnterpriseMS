namespace EnterpriseMS.Common;

/// <summary>
/// MIME 类型映射 —— 单一来源。替换 Project / Kb 等 Controller 中的重复实现，
/// 覆盖 FileUploadHelper 白名单的全部扩展名，避免各处维护不一致的映射。
/// </summary>
public static class MimeHelper
{
    public static string GetMimeType(string? ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return "application/octet-stream";
        return ext.TrimStart('.').ToLowerInvariant() switch
        {
            "pdf"  => "application/pdf",
            "doc"  => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls"  => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ppt"  => "application/vnd.ms-powerpoint",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "txt"  => "text/plain",
            "csv"  => "text/csv",
            "jpg" or "jpeg" => "image/jpeg",
            "png"  => "image/png",
            "gif"  => "image/gif",
            "bmp"  => "image/bmp",
            "tiff" => "image/tiff",
            "zip"  => "application/zip",
            "rar"  => "application/x-rar-compressed",
            "7z"   => "application/x-7z-compressed",
            "dwg"  => "application/acad",
            "dxf"  => "application/dxf",
            _      => "application/octet-stream",
        };
    }
}
