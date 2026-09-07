namespace EnterpriseMS.Common;

/// <summary>
/// 上传校验结果。用于区分“无文件”与“被拒绝”，便于 Controller 返回精准提示。
/// 大小上限不再在此处判断 —— 统一由 UploadLimits.MaxUploadBytes（500MB）接管，
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
/// 统一封装扩展名校验（危险黑名单）+ 非 Web 根存储，消除各 Controller 手写 FileStream 导致的不一致与存储型 XSS。
/// </summary>
public static class FileUploadHelper
{
    /// <summary>扩展名被安全策略拦截时的统一提示文案（各模块复用，避免提示口径不一）</summary>
    public const string RejectExtMessage =
        "该文件类型被安全策略拦截（可执行文件 / 脚本 / 网页类），请压缩为 zip / rar 后再上传";

    /// <summary>
    /// 已知业务扩展名（文档 / 图片 / 压缩 / CAD 等常见类型）。
    /// 现在仅作「已知类型」语义，**不再作为放行门槛**：业务格式五花八门（如 .gcfx、.gbq、.skp 等专业软件产物），
    /// 一律放行，只拦截 DangerousExts。当调用方显式传入 allowedExts 时，才按白名单严格校验。
    /// </summary>
    public static readonly HashSet<string> DefaultAllowedExts = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf","doc","docx","xls","xlsx","ppt","pptx",
        "jpg","jpeg","png","gif","bmp","tiff",
        "zip","rar","7z","txt","csv","dwg","dxf"
    };

    /// <summary>
    /// 危险扩展名黑名单 —— 无论黑白名单模式一律拒绝。
    /// 包括可执行/脚本/网页类：可在浏览器渲染执行或被系统加载，存在存储型 XSS 与钓鱼分发风险。
    /// 其余所有扩展名（含各类专业软件私有格式）均允许上传。
    /// </summary>
    public static readonly HashSet<string> DangerousExts = new(StringComparer.OrdinalIgnoreCase)
    {
        // 可执行 / 安装包 / 系统高危
        "exe","bat","cmd","com","msi","scr","pif","cpl","dll","hta","lnk","scf","inf","reg",
        // 脚本
        "js","jse","mjs","cjs","vbs","vbe","ps1","psm1","psd1","sh","bash","jar","war",
        // 网页 / 富媒体（可被浏览器渲染执行脚本）
        "html","htm","xhtml","xht","svg","svgz","swf",
        // 服务端脚本
        "php","php3","php5","phtml","jsp","jspx","asp","aspx","ashx","asmx"
    };

    /// <summary>
    /// 扩展名是否允许上传：命中危险黑名单 → 拒绝；其余一律放行。
    /// </summary>
    public static bool IsAllowed(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).TrimStart('.');
        // 无扩展名的文件（如 README、Makefile）本身不可执行，放行
        if (string.IsNullOrEmpty(ext)) return true;
        return !DangerousExts.Contains(ext);
    }

    /// <summary>
    /// 预校验上传文件扩展名（大小由 UploadLimits.MaxUploadBytes 全局限制）。
    /// allowedExts 为 null 时使用「非黑名单即放行」；显式传入时按白名单严格校验（且同样受黑名单约束）。
    /// </summary>
    public static UploadCheck CheckUpload(IFormFile? file, IEnumerable<string>? allowedExts = null)
    {
        if (file == null || file.Length == 0) return UploadCheck.Empty;

        var ext = Path.GetExtension(file.FileName).TrimStart('.');
        // 危险类型优先拦截，任何模式下都不放行
        if (!string.IsNullOrEmpty(ext) && DangerousExts.Contains(ext))
            return UploadCheck.ExtensionNotAllowed;

        // 未指定白名单 → 非黑名单即放行（业务私有格式如 .gcfx 全部允许）
        if (allowedExts == null) return UploadCheck.Ok;

        var set = new HashSet<string>(allowedExts, StringComparer.OrdinalIgnoreCase);
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
