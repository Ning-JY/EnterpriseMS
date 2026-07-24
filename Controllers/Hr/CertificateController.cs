using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.Hr;

// ── 证书管理 ──────────────────────────────────────────────────
[Authorize, Route("hr/cert")]
public class CertificateController : BaseAuthController
{
    private readonly ICertificateService    _svc;
    private readonly IDictService           _dictSvc;
    private readonly IEmployeeQueryService  _empQrySvc;

    public CertificateController(ICertificateService svc, IDictService dictSvc,
        IEmployeeQueryService empQrySvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _svc = svc; _dictSvc = dictSvc; _empQrySvc = empQrySvc;
    }

    [HasPermission("hr:cert:list")]
    public async Task<IActionResult> Index(string? keyword, int? status, int page = 1, int size = 15)
    {
        var (items, total, warnCount) = await _svc.GetPagedAsync(keyword, status, page, size);
        ViewBag.WarnCount = warnCount;
        ViewBag.CertTypes = await _dictSvc.GetDataByTypeAsync("cert_type");
        ViewBag.Employees = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Keyword = keyword; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Total = total; ViewBag.Size = size;
        return View(items);
    }

    [HttpPost("create-with-file")]
    [HasPermission("hr:cert:add")]
    public async Task<IActionResult> CreateWithFile(
        [FromForm] long EmployeeId, [FromForm] string CertName,
        [FromForm] string CertType, [FromForm] string? CertNo,
        [FromForm] string? IssueOrg, [FromForm] DateTime? IssueDate,
        [FromForm] DateTime? ExpireDate, IFormFile? file)
    {
        try
        {
            await _svc.CreateWithFileAsync(EmployeeId, CertName, CertType,
                CertNo, IssueOrg, IssueDate, ExpireDate, file, User.GetRealName());
            return ApiOk("证书已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("upload/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> Upload(long id, IFormFile file)
    {
        try
        {
            var result = await _svc.UploadAsync(id, file, User.GetRealName());
            return ApiOk(new { filePath = result.Value.path, fileName = result.Value.name }, "证书附件上传成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(long id)
    {
        var info = await _svc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path)) return NotFound();
        var bytes = await global::System.IO.File.ReadAllBytesAsync(info.Value.Path);
        return File(bytes, "application/octet-stream", info.Value.FileName);
    }

    [HttpPost("file/delete/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> DeleteFile(long id)
    {
        try { await _svc.DeleteFileAsync(id, User.GetRealName()); return ApiOk("附件已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _svc.DeleteAsync(id); return ApiOk("证书已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
