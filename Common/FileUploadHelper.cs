namespace EnterpriseMS.Common;

/// <summary>
/// 上传校验结果。用于区分“无文件”与“被拒绝”，便于 Controller 返回精准提示。
/// </summary>
public enum UploadCheck
{
    Ok,
    Empty,
    TooLarge,
    ExtensionNotAllowed
}

/// <summary>
/// 文件上传公共工具类，供 Project / Contract / Certificate 等模块复用。
/// 统一封装扩展名白名单 + 大小上限，消除各 Controller 手写 FileStream 导致的不一致。
/// </summary>
public static class FileUploadHelper
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    /// <summary>
    /// 统一扩展名白名单（与 ProjectService.AllowedFileExts 对齐），覆盖常见业务文档 / 图片 / 压缩 / CAD 格式。
    /// </summary>
    public static readonly HashSet<string> DefaultAllowedExts = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf","doc","docx","xls","xlsx","ppt","pptx",
        "jpg","jpeg","png","gif","bmp","tiff",
        "zip","rar","7z","txt","csv","dwg","dxf"
    };

    /// <summary>
    /// 预校验上传文件：空文件 / 超大小 / 扩展名不在白名单均会被拦截。
    /// allowedExts 为 null 时使用 DefaultAllowedExts。
    /// </summary>
    public static UploadCheck CheckUpload(IFormFile? file, IEnumerable<string>? allowedExts = null)
    {
        if (file == null || file.Length == 0) return UploadCheck.Empty;
        if (file.Length > MaxFileSize) return UploadCheck.TooLarge;

        var ext = Path.GetExtension(file.FileName);
        var set = allowedExts != null
            ? new HashSet<string>(allowedExts, StringComparer.OrdinalIgnoreCase)
            : DefaultAllowedExts;
        return set.Contains(ext) ? UploadCheck.Ok : UploadCheck.ExtensionNotAllowed;
    }

    /// <summary>
    /// 保存上传文件到指定子目录（wwwroot/uploads/{folder}/），返回 (物理路径, 原始文件名)。
    /// 校验未通过（空 / 超大 / 扩展名不允许）时返回 null。
    /// </summary>
    public static async Task<(string path, string name)?> SaveUploadFile(IFormFile? file, string folder,
        IEnumerable<string>? allowedExts = null)
    {
        if (CheckUpload(file, allowedExts) != UploadCheck.Ok) return null;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);

        var ext  = Path.GetExtension(file!.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);

        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);

        return (path, file.FileName);
    }
}
