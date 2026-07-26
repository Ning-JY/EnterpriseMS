using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace EnterpriseMS.Controllers.Hr;

// ── 合同管理 ──────────────────────────────────────────────────
[Authorize, Route("hr/contract")]
public class ContractController : BaseAuthController
{
    private readonly IContractService       _svc;
    private readonly IDictService           _dictSvc;
    private readonly IEmployeeQueryService  _empQrySvc;

    public ContractController(IContractService svc, IDictService dictSvc,
        IEmployeeQueryService empQrySvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _svc = svc; _dictSvc = dictSvc; _empQrySvc = empQrySvc;
    }

    [HasPermission("hr:contract:list")]
    public async Task<IActionResult> Index(string? keyword, int? status, int page = 1, int size = 15)
    {
        var paged = await _svc.GetPagedAsync(keyword, status, page, size);
        ViewBag.WarnCount     = paged.WarnCount;
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync(DictType.ContractType);
        var contractStatusOpts = await _dictSvc.GetDataByTypeAsync(DictType.ContractStatus);
        ViewBag.ContractStatusOptions = contractStatusOpts;
        var contractStatusMap = new Dictionary<int, string>();
        foreach (var o in contractStatusOpts) if (int.TryParse(o.DictValue, out var v)) contractStatusMap[v] = o.DictLabel;
        ViewBag.ContractStatusMap = contractStatusMap;
        ViewBag.Employees     = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Keyword = keyword; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Total = paged.Total; ViewBag.Size = size;
        return View(paged.Items);
    }

    [HttpPost("create-with-file")]
    [HasPermission("hr:contract:add")]
    public async Task<IActionResult> CreateWithFile(
        [FromForm] long EmployeeId, [FromForm] string ContractNo,
        [FromForm] string ContractType, [FromForm] DateTime StartDate,
        [FromForm] DateTime EndDate, [FromForm] DateTime? SignDate,
        [FromForm] string? Remark, IFormFile? file)
    {
        try
        {
            await _svc.CreateWithFileAsync(EmployeeId, ContractNo, ContractType,
                StartDate, EndDate, SignDate, Remark, file, User.GetRealName());
            return ApiOk("合同已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("upload/{id}")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> Upload(long id, IFormFile file)
    {
        try
        {
            var result = await _svc.UploadAsync(id, file, User.GetRealName());
            return ApiOk(new { filePath = result.Value.path, fileName = result.Value.name }, "附件上传成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpGet("download/{id}")]
    [HasPermission("hr:contract:list")]
    public async Task<IActionResult> Download(long id)
    {
        var info = await _svc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path)) return NotFound();
        return FileServingHelper.ServePhysicalFile(info.Value.Path, info.Value.FileName,
            global::System.IO.Path.GetExtension(info.Value.Path));
    }

    [HttpPost("delete/{id}")]
    [HasPermission("hr:contract:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _svc.DeleteAsync(id); return ApiOk("合同已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("file/delete/{id}")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> DeleteFile(long id)
    {
        try { await _svc.DeleteFileAsync(id, User.GetRealName()); return ApiOk("附件已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("terminate")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> Terminate(long id)
    {
        try { await _svc.TerminateAsync(id, User.GetRealName()); return ApiOk("合同已终止"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
