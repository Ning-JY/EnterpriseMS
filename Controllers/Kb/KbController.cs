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
    private readonly IPermissionService _permSvc;
    public KbController(IUnitOfWork uow, IKbService kbSvc, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _kbSvc = kbSvc; _permSvc = permSvc; }

    // ── 文件浏览（所有登录用户）──────────────────────────────
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await _uow.KbCategories.Query()
            .Where(c => c.Status == 1).OrderBy(c => c.Sort).ToListAsync();
        return View();
    }

    // ── 管理页（需要权限）───────────────────────────────────
    [HttpGet("manage")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Manage()
    {
        var userId = User.GetUserId();
        ViewBag.Categories = await _uow.KbCategories.Query()
            .Where(c => c.Status == 1).OrderBy(c => c.Sort).ToListAsync();
        ViewBag.CanUpload = await _permSvc.HasPermAsync(userId, "kb:file:upload");
        ViewBag.CanDelete = await _permSvc.HasPermAsync(userId, "kb:file:delete");
        ViewBag.CanManage = await _permSvc.HasPermAsync(userId, "kb:file:manage");
        return View();
    }

    // ── AJAX 列表（公开浏览：仅启用文件）─────────────────────
    [HttpGet("list")]
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> List(long? categoryId, string? keyword, int page = 1, int size = 20)
        => ApiOk(await QueryKbAsync(categoryId, keyword, page, size, false));

    // ── AJAX 列表（后台管理：含禁用文件）─────────────────────
    [HttpGet("admin-list")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> AdminList(long? categoryId, string? keyword, int page = 1, int size = 20)
        => ApiOk(await QueryKbAsync(categoryId, keyword, page, size, true));

    /// <summary>共享查询：投影为扁平结构，规避 KbCategory.Files 导航循环引用。</summary>
    private async Task<PagedResult<object>> QueryKbAsync(long? categoryId, string? keyword, int page, int size, bool admin)
    {
        var q = _uow.KbFiles.Query().Include(f => f.Category).AsQueryable();
        if (!admin) q = q.Where(f => f.Status == 1);
        if (categoryId.HasValue) q = q.Where(f => f.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(f => f.FileName.Contains(keyword) ||
                             (f.Description != null && f.Description.Contains(keyword)));

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(f => f.IsPinned)
                           .ThenByDescending(f => f.CreatedAt)
                           .Skip((page - 1) * size).Take(size).ToListAsync();

        var items = list.Select(f => new
        {
            f.Id, f.FileName, f.OriginalName, f.Description,
            CategoryName = f.Category != null ? f.Category.Name : "-",
            f.FileSize, FileSizeText = FileSizeText(f.FileSize), f.FileExt,
            f.Version, f.IsPinned, f.Status, f.DownloadCount, f.CreatedBy, f.CreatedAt,
            CanPreview = new[] { "pdf", "jpg", "jpeg", "png" }.Contains((f.FileExt ?? "").ToLower())
        }).Cast<object>().ToList();

        return new PagedResult<object> { Items = items, Total = total, Page = page, PageSize = size };
    }

    private static string FileSizeText(long size)
        => size < 1024 ? $"{size}B"
         : size < 1048576 ? $"{size / 1024d:N1}KB"
         : $"{size / 1048576d:N1}MB";

    // ── 上传文件 ────────────────────────────────────────────
    [HttpPost("upload"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:upload")]
    public async Task<IActionResult> Upload(
        IFormFile file, [FromForm] long categoryId,
        [FromForm] string? displayName, [FromForm] string? description,
        [FromForm] string? version, [FromForm] bool isPinned = false)
    {
        if (file == null || file.Length == 0)
            return ApiFail("请选择文件");

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
        return ApiOk(new { id }, "上传成功");
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
        if (f == null) return ApiFail("文件不存在");
        await _kbSvc.TogglePinAsync(id, User.GetRealName());
        var newPinned = !f.IsPinned;
        return ApiOk(newPinned ? "已置顶" : "已取消置顶");
    }

    // ── 删除文件 ────────────────────────────────────────────
    [HttpPost("delete/{id}")]
    [HasPermission("kb:file:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return ApiFail("文件不存在");
        await _kbSvc.DeleteAsync(id, User.GetRealName());
        return ApiOk("已删除");
    }

}
