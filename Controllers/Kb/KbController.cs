using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Kb;

[Authorize, Route("kb")]
public class KbController : BaseAuthController
{
    private readonly IUnitOfWork    _uow;
    private readonly IOperLogService _logSvc;
    private static readonly string[] AllowedExts =
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
          ".txt", ".png", ".jpg", ".jpeg", ".zip", ".rar" };
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    public KbController(IUnitOfWork uow, IOperLogService logSvc, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _logSvc = logSvc; }

    // ── 文件浏览（所有登录用户）──────────────────────────────
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Index(long? categoryId, string? keyword, int page = 1, int size = 20)
    {
        var categories = await _uow.KbCategories.Query()
            .Where(c => c.Status == 1).OrderBy(c => c.Sort).ToListAsync();

        var q = _uow.KbFiles.Query()
            .Include(f => f.Category)
            .Where(f => f.Status == 1);

        if (categoryId.HasValue) q = q.Where(f => f.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(f => f.FileName.Contains(keyword) ||
                             (f.Description != null && f.Description.Contains(keyword)));

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(f => f.IsPinned)
                           .ThenByDescending(f => f.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();

        ViewBag.Categories  = categories;
        ViewBag.CategoryId  = categoryId;
        ViewBag.Keyword     = keyword;
        ViewBag.Total       = total;
        ViewBag.Page        = page;
        ViewBag.Size        = size;
        return View(list);
    }

    // ── 管理页（需要权限）───────────────────────────────────
    [HttpGet("manage")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Manage(long? categoryId, string? keyword, int page = 1, int size = 20)
    {
        var categories = await _uow.KbCategories.Query()
            .Where(c => c.Status == 1).OrderBy(c => c.Sort).ToListAsync();

        var q = _uow.KbFiles.Query()
            .Include(f => f.Category)
            .AsQueryable();

        if (categoryId.HasValue) q = q.Where(f => f.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(f => f.FileName.Contains(keyword));

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(f => f.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.CategoryId = categoryId;
        ViewBag.Keyword    = keyword;
        ViewBag.Total      = total;
        ViewBag.Page       = page;
        ViewBag.Size       = size;
        return View(list);
    }

    // ── 上传文件 ────────────────────────────────────────────
    [HttpPost("upload"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:upload")]
    public async Task<IActionResult> Upload(
        IFormFile file, [FromForm] long categoryId,
        [FromForm] string? displayName, [FromForm] string? description,
        [FromForm] string? version, [FromForm] bool isPinned = false)
    {
        if (file == null || file.Length == 0)
            return Json(ApiResult<object>.Fail("请选择文件"));
        if (file.Length > MaxFileSize)
            return Json(ApiResult<object>.Fail("文件大小不能超过50MB"));

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExts.Contains(ext))
            return Json(ApiResult<object>.Fail($"不支持的文件类型 {ext}，支持：{string.Join(" ", AllowedExts)}"));

        var category = await _uow.KbCategories.GetByIdAsync(categoryId);
        if (category == null) return Json(ApiResult<object>.Fail("分类不存在"));

        var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kb",
                                   categoryId.ToString());
        Directory.CreateDirectory(saveDir);
        var saveName = $"{DateTime.Now:yyyyMMdd}_{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(saveDir, saveName);

        using (var fs = new FileStream(savePath, FileMode.Create))
            await file.CopyToAsync(fs);

        var kbFile = new KbFile
        {
            CategoryId   = categoryId,
            FileName     = string.IsNullOrWhiteSpace(displayName)
                           ? Path.GetFileNameWithoutExtension(file.FileName) : displayName,
            OriginalName = file.FileName,
            FilePath     = savePath,
            FileSize     = file.Length,
            FileExt      = ext.TrimStart('.'),
            Description  = description,
            Version      = version,
            IsPinned     = isPinned,
            Status       = 1,
            CreatedBy    = User.GetRealName(),
        };
        await _uow.KbFiles.AddAsync(kbFile);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("上传知识库文件", $"[{category.Name}] {kbFile.FileName}", "INSERT", kbFile.Id);
        return Json(ApiResult<object>.Ok(new { kbFile.Id }, "上传成功"));
    }

    // ── 下载文件（计数 + 返回文件）──────────────────────────
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在或已删除");

        // 更新下载次数（不影响主流程）
        f.DownloadCount++;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();

        var bytes   = await global::System.IO.File.ReadAllBytesAsync(f.FilePath);
        var mime    = GetMimeType(f.FileExt ?? "bin");
        var dlName  = Uri.EscapeDataString(f.OriginalName);
        Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{dlName}";
        return File(bytes, mime);
    }

    // ── 预览（PDF/图片内嵌，其他跳下载）───────────────────
    [HttpGet("preview/{id}")]
    public async Task<IActionResult> Preview(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在");

        var previewExts = new[] { "pdf", "jpg", "jpeg", "png" };
        if (!previewExts.Contains(f.FileExt?.ToLower()))
            return RedirectToAction("Download", new { id });

        var bytes = await global::System.IO.File.ReadAllBytesAsync(f.FilePath);
        var mime  = GetMimeType(f.FileExt ?? "bin");
        // 内嵌预览不设 Content-Disposition: attachment
        return File(bytes, mime);
    }

    // ── 置顶切换 ────────────────────────────────────────────
    [HttpPost("pin/{id}")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> TogglePin(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return Json(ApiResult<object>.Fail("文件不存在"));
        f.IsPinned   = !f.IsPinned;
        f.UpdatedBy  = User.GetRealName();
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(f.IsPinned ? "已置顶" : "已取消置顶"));
    }

    // ── 删除文件 ────────────────────────────────────────────
    [HttpPost("delete/{id}")]
    [HasPermission("kb:file:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return Json(ApiResult<object>.Fail("文件不存在"));
        // 软删除（物理文件保留，管理员可恢复）
        _uow.KbFiles.SoftDelete(f);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("删除知识库文件", f.FileName, "DELETE", id);
        return Json(ApiResult<object>.Ok("已删除"));
    }

    private static string GetMimeType(string ext) => ext.ToLower() switch
    {
        "pdf"  => "application/pdf",
        "doc"  => "application/msword",
        "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"  => "application/vnd.ms-excel",
        "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ppt"  => "application/vnd.ms-powerpoint",
        "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "txt"  => "text/plain",
        "jpg" or "jpeg" => "image/jpeg",
        "png"  => "image/png",
        "zip"  => "application/zip",
        "rar"  => "application/x-rar-compressed",
        _      => "application/octet-stream",
    };
}
