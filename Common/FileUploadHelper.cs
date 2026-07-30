namespace EnterpriseMS.Common;

/// <summary>
/// 上传校验结果。用于区分“无文件”与“被拒绝”，便于 Controller 返回精准提示。
/// 大小上限不再在此处判断 —— 统一由 Program.cs 的全局 500MB 限制接管，
/// 避免 Kestrel 缓冲期 DoS 且各 action 重复限制。
/// </summary>
public enum UploadCheck
{
    Ok,
    Empty,
    ExtensionNotAllowed
}

/// <summary>
/// 文件上传公共工具类，供 Project / Contract / Certificate / Kb 等模块复用。
/// 统一封装扩展名白名单 + 非 Web 根存储，消除各 Controller 手写 FileStream 导致的不一致与存储型 XSS。
/// </summary>
public static class FileUploadHelper
{
    /// <summary>
    /// 统一扩展名白名单（唯一来源）。覆盖常见业务文档 / 图片 / 压缩 / CAD 格式；
    /// 刻意排除 .html / .svg / .js / .exe 等可在浏览器渲染或执行的类型。
    /// </summary>
    public static readonly HashSet<string> DefaultAllowedExts = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf","doc","docx","xls","xlsx","ppt","pptx",
        "jpg","jpeg","png","gif","bmp","tiff",
        "zip","rar","7z","txt","csv","dwg","dxf"
    };

    /// <summary>
    /// 预校验上传文件扩展名（大小由全局 500MB 统一限制）。
    /// allowedExts 为 null 时使用 DefaultAllowedExts。
    /// </summary>
    public static UploadCheck CheckUpload(IFormFile? file, IEnumerable<string>? allowedExts = null)
    {
        if (file == null || file.Length == 0) return UploadCheck.Empty;

        var ext = Path.GetExtension(file.FileName).TrimStart('.');
        var set = allowedExts != null
            ? new HashSet<string>(allowedExts, StringComparer.OrdinalIgnoreCase)
            : DefaultAllowedExts;
        return set.Contains(ext) ? UploadCheck.Ok : UploadCheck.ExtensionNotAllowed;
    }

    /// <summary>
    /// 保存上传文件到「非 Web 根目录」（应用根/uploads/{folder}/），返回 (物理路径, 原始文件名)。
    /// 落点在 wwwroot 之外，静态文件中间件不会直接渲染这些文件，从根上杜绝存储型 XSS；
    /// 下载一律经由各 Controller 读取物理路径返回。
    /// </summary>
    public static async Task<(string path, string name)?> SaveUploadFile(IFormFile? file, string folder,
        IEnumerable<string>? allowedExts = null)
    {
        if (CheckUpload(file, allowedExts) != UploadCheck.Ok) return null;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", folder);
        Directory.CreateDirectory(dir);

        var extWithDot = Path.GetExtension(file!.FileName);
        var name = $"{Guid.NewGuid():N}{extWithDot}";
        var path = Path.Combine(dir, name);

        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);

        return (path, file.FileName);
    }
}
