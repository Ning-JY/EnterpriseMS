using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Hr;
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
    public async Task<IActionResult> Index()
    {
        // 列表数据由 /hr/cert/list (AJAX) 提供，本页仅做容器。
        ViewBag.CertTypes = await _dictSvc.GetDataByTypeAsync(DictType.CertType);
        ViewBag.Employees = await _empQrySvc.GetAllOnJobAsync();
        return View();
    }

    [HttpGet("list")]
    [HasPermission("hr:cert:list")]
    public async Task<IActionResult> List(string? keyword, int? status, int page = 1, int size = 15)
    {
        var paged = await _svc.GetPagedAsync(keyword, status, page, size);
        return ApiOk(paged);
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
    [HasPermission("hr:cert:list")]
    public async Task<IActionResult> Download(long id)
    {
        var info = await _svc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path))
        {
            return NotFound();
        }

        return FileServingHelper.ServePhysicalFile(info.Value.Path, info.Value.FileName,
            global::System.IO.Path.GetExtension(info.Value.Path));
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

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> Update([FromBody] CertUpdateDto dto)
    {
        try { await _svc.UpdateAsync(dto, User.GetRealName()); return ApiOk("修改成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
