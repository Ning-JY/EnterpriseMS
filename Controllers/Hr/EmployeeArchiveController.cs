using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.Hr;

// ── 员工档案子实体（详情页 5 个 Tab 的增删改/上传）────────────
// 所有写操作要求 hr:employee:edit，读操作要求 hr:employee:list，与详情页访问权限一致。
[Authorize, Route("hr/employee/archive")]
public class EmployeeArchiveController : BaseAuthController
{
    private readonly IContractService            _contractSvc;
    private readonly ICertificateService         _certSvc;
    private readonly IEducationService           _eduSvc;
    private readonly IWorkExpService             _workSvc;
    private readonly IEmployeeAttachmentService  _attachSvc;

    public EmployeeArchiveController(IContractService contractSvc, ICertificateService certSvc,
        IEducationService eduSvc, IWorkExpService workSvc, IEmployeeAttachmentService attachSvc,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _contractSvc = contractSvc; _certSvc = certSvc;
        _eduSvc = eduSvc; _workSvc = workSvc; _attachSvc = attachSvc;
    }

    /* ========== 合同 ========== */
    [HttpGet("contract/list/{empId}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> ContractList(long empId)
    {
        var list = await _contractSvc.GetPagedAsync(null, null, 1, int.MaxValue);
        var items = list.Items.Where(c => c.EmployeeId == empId).OrderByDescending(c => c.StartDate)
            .Select(c => new
            {
                c.Id, c.ContractNo, c.ContractType,
                StartDate = c.StartDate.ToString("yyyy-MM-dd"),
                EndDate = c.EndDate.ToString("yyyy-MM-dd"),
                SignDate = c.SignDate?.ToString("yyyy-MM-dd"),
                c.Status, c.FileName,
                HasFile = !string.IsNullOrEmpty(c.FilePath),
                DownloadUrl = "/hr/employee/archive/contract/download/" + c.Id
            }).ToList();
        return ApiOk(items);
    }

    [HttpPost("contract/save")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> ContractSave(
        [FromForm] long Id, [FromForm] long EmployeeId, [FromForm] string ContractNo,
        [FromForm] string ContractType, [FromForm] DateTime? StartDate, [FromForm] DateTime? EndDate,
        [FromForm] DateTime? SignDate, [FromForm] int Status, [FromForm] string? Remark, IFormFile? file)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ContractNo)) return ApiFail("请填写合同编号");
            if (Id > 0)
            {
                await _contractSvc.UpdateAsync(new ContractUpdateDto
                {
                    Id = Id, ContractNo = ContractNo, ContractType = ContractType,
                    StartDate = StartDate ?? DateTime.MinValue, EndDate = EndDate ?? DateTime.MinValue,
                    SignDate = SignDate, Status = Status, Remark = Remark
                }, User.GetRealName());
                if (file != null && file.Length > 0)
                    await _contractSvc.UploadAsync(Id, file, User.GetRealName());
                return ApiOk("合同已更新");
            }
            if (!StartDate.HasValue || !EndDate.HasValue) return ApiFail("请填写合同起止日期");
            await _contractSvc.CreateWithFileAsync(EmployeeId, ContractNo, ContractType,
                StartDate.Value, EndDate.Value, SignDate, Remark, file, User.GetRealName());
            return ApiOk("合同已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("contract/upload/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> ContractUpload(long id, IFormFile file)
    {
        try
        {
            var r = await _contractSvc.UploadAsync(id, file, User.GetRealName());
            return ApiOk(new { filePath = r.Value.path, fileName = r.Value.name }, "附件上传成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("contract/delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> ContractDelete(long id)
    {
        try { await _contractSvc.DeleteAsync(id); return ApiOk("合同已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpGet("contract/download/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> ContractDownload(long id)
    {
        var info = await _contractSvc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path)) return NotFound();
        var inline = Request.Query["inline"] == "1";
        return FileServingHelper.ServePhysicalFile(info.Value.Path, info.Value.FileName,
            global::System.IO.Path.GetExtension(info.Value.Path), inline);
    }

    /* ========== 证书 ========== */
    [HttpGet("cert/list/{empId}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> CertList(long empId)
    {
        var list = await _certSvc.GetPagedAsync(null, null, 1, int.MaxValue);
        var items = list.Items.Where(c => c.EmployeeId == empId).OrderByDescending(c => c.IssueDate)
            .Select(c => new
            {
                c.Id, c.CertName, c.CertType, c.CertNo,
                IssueDate = c.IssueDate?.ToString("yyyy-MM-dd"),
                ExpireDate = c.ExpireDate?.ToString("yyyy-MM-dd"),
                c.Status, c.FileName,
                HasFile = !string.IsNullOrEmpty(c.FilePath),
                DownloadUrl = "/hr/employee/archive/cert/download/" + c.Id
            }).ToList();
        return ApiOk(items);
    }

    [HttpPost("cert/save")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> CertSave(
        [FromForm] long Id, [FromForm] long EmployeeId, [FromForm] string CertName,
        [FromForm] string CertType, [FromForm] string? CertNo, [FromForm] string? IssueOrg,
        [FromForm] DateTime? IssueDate, [FromForm] DateTime? ExpireDate, [FromForm] int Status,
        IFormFile? file)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CertName)) return ApiFail("请填写证书名称");
            if (Id > 0)
            {
                await _certSvc.UpdateAsync(new CertUpdateDto
                {
                    Id = Id, CertName = CertName, CertType = CertType, CertNo = CertNo,
                    ExpireDate = ExpireDate, Status = Status
                }, User.GetRealName());
                if (file != null && file.Length > 0)
                    await _certSvc.UploadAsync(Id, file, User.GetRealName());
                return ApiOk("证书已更新");
            }
            await _certSvc.CreateWithFileAsync(EmployeeId, CertName, CertType, CertNo,
                IssueOrg, IssueDate, ExpireDate, file, User.GetRealName());
            return ApiOk("证书已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("cert/upload/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> CertUpload(long id, IFormFile file)
    {
        try
        {
            var r = await _certSvc.UploadAsync(id, file, User.GetRealName());
            return ApiOk(new { filePath = r.Value.path, fileName = r.Value.name }, "证书附件上传成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("cert/delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> CertDelete(long id)
    {
        try { await _certSvc.DeleteAsync(id); return ApiOk("证书已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpGet("cert/download/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> CertDownload(long id)
    {
        var info = await _certSvc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path)) return NotFound();
        var inline = Request.Query["inline"] == "1";
        return FileServingHelper.ServePhysicalFile(info.Value.Path, info.Value.FileName,
            global::System.IO.Path.GetExtension(info.Value.Path), inline);
    }

    /* ========== 教育经历 ========== */
    [HttpGet("edu/list/{empId}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> EduList(long empId)
    {
        var list = await _eduSvc.GetListAsync(empId);
        var items = list.Select(e => new
        {
            e.Id, e.SchoolName, e.Major, e.Degree,
            StartDate = e.StartDate?.ToString("yyyy-MM"),
            EndDate = e.EndDate?.ToString("yyyy-MM"),
            IsFullTime = e.IsFullTime ? "是" : "否",
            e.Remark
        }).ToList();
        return ApiOk(items);
    }

    [HttpPost("edu/save")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> EduSave([FromForm] EducationDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.SchoolName)) return ApiFail("请填写学校名称");
            if (dto.Id > 0) await _eduSvc.UpdateAsync(dto);
            else await _eduSvc.CreateAsync(new CreateEducationDto
            {
                SchoolName = dto.SchoolName, Major = dto.Major, Degree = dto.Degree,
                StartDate = dto.StartDate, EndDate = dto.EndDate,
                IsFullTime = dto.IsFullTime, Remark = dto.Remark
            }, dto.EmployeeId, User.GetRealName());
            return ApiOk("教育经历已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("edu/delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> EduDelete(long id)
    {
        try { await _eduSvc.DeleteAsync(id); return ApiOk("教育经历已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    /* ========== 工作经历 ========== */
    [HttpGet("work/list/{empId}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> WorkList(long empId)
    {
        var list = await _workSvc.GetListAsync(empId);
        var items = list.Select(w => new
        {
            w.Id, w.CompanyName, w.Position,
            StartDate = w.StartDate?.ToString("yyyy-MM"),
            EndDate = w.EndDate?.ToString("yyyy-MM"),
            w.Remark
        }).ToList();
        return ApiOk(items);
    }

    [HttpPost("work/save")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> WorkSave([FromForm] WorkExpDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName)) return ApiFail("请填写公司名称");
            if (dto.Id > 0) await _workSvc.UpdateAsync(dto);
            else await _workSvc.CreateAsync(new CreateWorkExpDto
            {
                CompanyName = dto.CompanyName, Position = dto.Position,
                StartDate = dto.StartDate, EndDate = dto.EndDate, Remark = dto.Remark
            }, dto.EmployeeId, User.GetRealName());
            return ApiOk("工作经历已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("work/delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> WorkDelete(long id)
    {
        try { await _workSvc.DeleteAsync(id); return ApiOk("工作经历已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    /* ========== 附件 ========== */
    [HttpGet("attach/list/{empId}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> AttachList(long empId)
    {
        var list = await _attachSvc.GetListAsync(empId);
        var items = list.Select(a => new
        {
            a.Id, a.FileName, a.FileSize, a.FileType, a.Remark,
            UploadTime = a.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            DownloadUrl = "/hr/employee/archive/attach/download/" + a.Id
        }).ToList();
        return ApiOk(items);
    }

    [HttpPost("attach/upload")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> AttachUpload(
        [FromForm] long EmployeeId, [FromForm] string? Remark, IFormFile file)
    {
        try
        {
            var id = await _attachSvc.UploadAsync(EmployeeId, file, Remark, User.GetRealName());
            return ApiOk(new { id }, "附件已上传");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("attach/delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> AttachDelete(long id)
    {
        try { await _attachSvc.DeleteAsync(id); return ApiOk("附件已删除"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpGet("attach/download/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> AttachDownload(long id)
    {
        var info = await _attachSvc.GetDownloadInfoAsync(id);
        if (info == null || !global::System.IO.File.Exists(info.Value.Path)) return NotFound();
        var inline = Request.Query["inline"] == "1";
        return FileServingHelper.ServePhysicalFile(info.Value.Path, info.Value.FileName,
            global::System.IO.Path.GetExtension(info.Value.Path), inline);
    }
}
