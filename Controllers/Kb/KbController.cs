using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Kb;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Kb;

[Authorize, Route("kb")]
public class KbController : BaseAuthController
{
    private readonly IUnitOfWork    _uow;
    private readonly IKbService     _kbSvc;
    public KbController(IUnitOfWork uow, IKbService kbSvc, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _kbSvc = kbSvc; }

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

        // 扩展名白名单由 FileUploadHelper.DefaultAllowedExts 单一管控；大小由全局 500MB 限制；
        // 文件落非 Web 根目录，避免静态文件中间件直接渲染用户文件。
        var dto = new KbUploadDto
        {
            File = file,
            CategoryId = categoryId,
            DisplayName = displayName,
            Description = description,
            Version = version,
            IsPinned = isPinned,
        };
        var id = await _kbSvc.UploadAsync(dto, User.GetRealName());
        return Json(ApiResult<object>.Ok(new { id }, "上传成功"));
    }

    // ── 下载文件（计数 + 返回文件）──────────────────────────
    [HttpGet("download/{id}")]
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Download(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在或已删除");

        // 下载计数下沉到 Service（不影响主流程）
        await _kbSvc.IncrementDownloadCountAsync(id);

        return FileServingHelper.ServePhysicalFile(f.FilePath, f.OriginalName, f.FileExt);
    }

    // ── 预览（PDF/图片内嵌，其他跳下载）───────────────────
    [HttpGet("preview/{id}")]
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Preview(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在");

        var previewExts = new[] { "pdf", "jpg", "jpeg", "png" };
        if (!previewExts.Contains(f.FileExt?.ToLower()))
            return RedirectToAction("Download", new { id });

        return FileServingHelper.ServePhysicalFile(f.FilePath, f.OriginalName, f.FileExt, inline: true);
    }

    // ── 置顶切换 ────────────────────────────────────────────
    [HttpPost("pin/{id}")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> TogglePin(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return Json(ApiResult<object>.Fail("文件不存在"));
        await _kbSvc.TogglePinAsync(id, User.GetRealName());
        var newPinned = !f.IsPinned;
        return Json(ApiResult<object>.Ok(newPinned ? "已置顶" : "已取消置顶"));
    }

    // ── 删除文件 ────────────────────────────────────────────
    [HttpPost("delete/{id}")]
    [HasPermission("kb:file:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return Json(ApiResult<object>.Fail("文件不存在"));
        await _kbSvc.DeleteAsync(id, User.GetRealName());
        return Json(ApiResult<object>.Ok("已删除"));
    }

}
