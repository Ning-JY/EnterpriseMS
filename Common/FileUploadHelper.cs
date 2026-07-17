namespace EnterpriseMS.Common;

/// <summary>
/// 文件上传公共工具类，供 ContractController、CertificateController 等复用。
/// </summary>
public static class FileUploadHelper
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    /// <summary>
    /// 保存上传文件到指定子目录（wwwroot/uploads/{folder}/），返回 (物理路径, 原始文件名)。
    /// 文件大小超过限制或文件为空时返回 null。
    /// </summary>
    public static async Task<(string path, string name)?> SaveUploadFile(IFormFile? file, string folder)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > MaxFileSize) return null;

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);

        var ext  = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);

        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);

        return (path, file.FileName);
    }
}
