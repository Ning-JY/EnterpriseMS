using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Kb;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Kb;

[Authorize, Route("kb")]
public class KbController : BaseAuthController
{
    private readonly IKbService     _kbSvc;
    private readonly IOperLogService _logSvc;
    private readonly IUnitOfWork    _uow;

    public KbController(IKbService kbSvc, IOperLogService logSvc,
        IUnitOfWork uow, IPermissionService permSvc)
        : base(permSvc)
    { _kbSvc = kbSvc; _logSvc = logSvc; _uow = uow; }

    // ── 文件浏览 ────────────────────────────────────────────
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Index(KbQueryDto query)
    {
        var result = await _kbSvc.GetPagedAsync(query, adminView: false);
        ViewBag.Categories = await _kbSvc.GetCategoriesAsync();
        ViewBag.CategoryId = query.CategoryId;
        ViewBag.Keyword    = query.Keyword;
        return View(result);
    }

    // ── 管理页 ─────────────────────────────────────────────
    [HttpGet("manage")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Manage(KbQueryDto query)
    {
        var result = await _kbSvc.GetPagedAsync(query, adminView: true);
        ViewBag.Categories = await _kbSvc.GetCategoriesAsync();
        ViewBag.CategoryId = query.CategoryId;
        ViewBag.Keyword    = query.Keyword;
        ViewBag.Status     = query.Status;
        return View(result);
    }

    // ── 文件详情（JSON）────────────────────────────────────
    [HttpGet("detail/{id}")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Detail(long id)
    {
        var dto = await _kbSvc.GetDetailAsync(id);
        if (dto == null) return Json(ApiResult<object>.Fail("文件不存在"));
        return Json(ApiResult<object>.Ok(new {
            id = dto.Id, fileName = dto.FileName, categoryId = dto.CategoryId,
            categoryName = dto.CategoryName, originalName = dto.OriginalName,
            fileSize = dto.FileSize, fileExt = dto.FileExt,
            description = dto.Description, version = dto.Version,
            downloadCount = dto.DownloadCount, isPinned = dto.IsPinned,
            status = dto.Status, createdBy = dto.CreatedBy,
            createdAt = dto.CreatedAt.ToString("yyyy-MM-dd"),
        }));
    }

    // ── 上传文件 ────────────────────────────────────────────
    [HttpPost("upload"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:upload")]
    public async Task<IActionResult> Upload(KbUploadDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            var id = await _kbSvc.UploadAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("上传知识库文件", dto.File.FileName, "INSERT", id);
            return Json(ApiResult<object>.Ok(new { id }, "上传成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 更新文件信息 ────────────────────────────────────────
    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Update([FromBody] KbUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            await _kbSvc.UpdateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("修改知识库文件", $"文件ID：{dto.Id}", "UPDATE", dto.Id);
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 下载文件（流式返回，避免大文件 OOM）────────────────
    [HttpGet("download/{id}")]
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Download(long id)
    {
        var info = await _kbSvc.GetDownloadInfoAsync(id);
        if (info == null) return NotFound("文件不存在或已删除");

        var (path, name, mime, _) = info.Value;
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var dlName = Uri.EscapeDataString(name);
        Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{dlName}";
        return File(stream, mime);
    }

    // ── 预览（PDF/图片内嵌）────────────────────────────────
    [HttpGet("preview/{id}")]
    [HasPermission("kb:file:list")]
    public async Task<IActionResult> Preview(long id)
    {
        var dto = await _kbSvc.GetDetailAsync(id);
        if (dto == null) return NotFound("文件不存在");

        if (!dto.CanPreview)
            return RedirectToAction("Download", new { id });

        var info = await _kbSvc.GetDownloadInfoAsync(id);
        if (info == null) return NotFound("文件不存在");

        var (path, _, mime, _) = info.Value;
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, mime);
    }

    // ── 置顶切换 ────────────────────────────────────────────
    [HttpPost("pin/{id}"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> TogglePin(long id)
    {
        try
        {
            await _kbSvc.TogglePinAsync(id, User.GetRealName());
            await _logSvc.LogAsync("切换置顶", $"文件ID：{id}", "UPDATE", id);
            return Json(ApiResult<object>.Ok("操作成功"));
        }
        catch (NotFoundException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 删除文件 ────────────────────────────────────────────
    [HttpPost("delete/{id}"), ValidateAntiForgeryToken]
    [HasPermission("kb:file:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var dto = await _kbSvc.GetDetailAsync(id);
            await _kbSvc.DeleteAsync(id, User.GetRealName());
            await _logSvc.LogAsync("删除知识库文件", dto?.FileName, "DELETE", id);
            return Json(ApiResult<object>.Ok("已删除"));
        }
        catch (NotFoundException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 导出 Excel ─────────────────────────────────────────
    [HttpGet("export")]
    [HasPermission("kb:file:manage")]
    public async Task<IActionResult> Export(long? categoryId, string? keyword, int? status)
    {
        var query = new KbQueryDto
        {
            CategoryId = categoryId, Keyword = keyword,
            Status = status, Page = 1, Size = 99999
        };
        var result = await _kbSvc.GetPagedAsync(query, adminView: true);
        var rows = result.Items.Select((f, idx) => new
        {
            序号     = idx + 1,
            文件名称 = f.FileName,
            原始文件 = f.OriginalName,
            分类     = f.CategoryName,
            版本     = f.Version ?? "",
            大小     = f.FileSizeText,
            上传人   = f.CreatedBy,
            上传时间 = f.CreatedAt.ToString("yyyy-MM-dd"),
            下载次数 = f.DownloadCount,
            状态     = f.Status == 1 ? "启用" : "禁用",
            置顶     = f.IsPinned ? "是" : "否",
            说明     = f.Description ?? "",
        });
        var ms = new MemoryStream();
        await ms.SaveAsAsync(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"知识库文件_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private string GetErrors() => string.Join("；",
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
}
