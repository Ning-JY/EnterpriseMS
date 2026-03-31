using EnterpriseMS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Project;

[Authorize, Route("project")]
public class ProjectController : BaseAuthController
{
    private readonly IProjectService       _projSvc;
    private readonly IDeptService          _deptSvc;
    private readonly IDictService          _dictSvc;
    private readonly IEmployeeQueryService _empQrySvc;
    private readonly IOperLogService       _logSvc;
    private readonly IUnitOfWork           _uow;

    public ProjectController(IProjectService projSvc, IDeptService deptSvc,
        IDictService dictSvc, IEmployeeQueryService empQrySvc,
        IOperLogService logSvc, IUnitOfWork uow, IPermissionService permSvc)
        : base(permSvc)
    {
        _projSvc = projSvc; _deptSvc = deptSvc; _dictSvc = dictSvc;
        _empQrySvc = empQrySvc; _logSvc = logSvc; _uow = uow;
    }  [HasPermission("proj:project:list")]
    public async Task<IActionResult> Index(ProjectQueryDto query)
    {
        var result  = await _projSvc.GetPagedAsync(query);
        var depts   = await _deptSvc.GetTreeAsync();
        var members = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Depts   = depts;
        ViewBag.Members = members;
        ViewBag.Query   = query;
        return View(result);
    }

    [HttpGet("{id}")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var proj = await _projSvc.GetDetailAsync(id);
        if (proj == null) return NotFound();
        ViewBag.AllMembers        = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.DictMilestoneType = await _dictSvc.GetDataByTypeAsync("milestone_type");
        return View(proj);
    }

    [HttpGet("edit/{id}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var proj = await _projSvc.GetDetailAsync(id);
        if (proj == null) return NotFound();
        if (proj.ProgressStatus == 9)
            return RedirectToAction("Detail", new { id });

        var depts   = await _deptSvc.GetTreeAsync();
        var members = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Depts   = depts;
        ViewBag.Members = members;
        ViewBag.IsEdit  = true;
        return View(proj);
    }

    [HttpGet("create")]
    [HasPermission("proj:project:add")]
    public async Task<IActionResult> Create()
    {
        var depts   = await _deptSvc.GetTreeAsync();
        var members = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Depts   = depts;
        ViewBag.Members = members;
        ViewBag.ProjNo  = await _projSvc.GenerateProjNoAsync();
        return View();
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:add")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            var id = await _projSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新建项目", $"项目：{dto.ProjName}", "INSERT", id);
            return Json(ApiResult<object>.Ok(new { id }, "项目创建成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            await _projSvc.UpdateAsync(dto, User.GetRealName());
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("status"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:status")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusDto dto)
    {
        try
        {
            await _projSvc.ChangeStatusAsync(dto, User.GetRealName());
            return Json(ApiResult<object>.Ok("状态已更新"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("terminate")]
    [HasPermission("proj:project:terminate")]
    public async Task<IActionResult> Terminate(long id, string reason)
    {
        try
        {
            await _projSvc.TerminateAsync(id, reason, User.GetRealName());
            return Json(ApiResult<object>.Ok("项目已终止"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 成员 ───────────────────────────────────────────────
    [HttpPost("{projectId}/members")]
    [HasPermission("proj:member:add")]
    public async Task<IActionResult> AddMember(long projectId, [FromBody] CreateMemberDto dto)
    {
        try
        {
            var id = await _projSvc.AddMemberAsync(projectId, dto, User.GetRealName());
            return Json(ApiResult<object>.Ok(new { id }, "成员添加成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPut("{projectId}/members/{memberId}")]
    [HasPermission("proj:member:edit")]
    public async Task<IActionResult> UpdateMember(long projectId, long memberId,
        [FromBody] UpdateMemberDto dto)
    {
        dto.Id = memberId;
        try
        {
            await _projSvc.UpdateMemberAsync(projectId, dto, User.GetRealName());
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("{projectId}/members/{memberId}/remove")]
    [HasPermission("proj:member:add")]
    public async Task<IActionResult> RemoveMember(long projectId, long memberId)
    {
        try
        {
            await _projSvc.RemoveMemberAsync(projectId, memberId, User.GetRealName());
            await _logSvc.LogAsync("移除项目成员", $"项目ID:{projectId} 成员ID:{memberId}", "UPDATE", projectId);
            return Json(ApiResult<object>.Ok("成员已移除"));
        }
        catch (NotFoundException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 里程碑 ─────────────────────────────────────────────
    [HttpPost("{projectId}/milestones")]
    [HasPermission("proj:milestone:add")]
    public async Task<IActionResult> AddMilestone(long projectId,
        [FromBody] CreateMilestoneDto dto)
    {
        try
        {
            var id = await _projSvc.AddMilestoneAsync(projectId, dto, User.GetRealName());
            return Json(ApiResult<object>.Ok(new { id }, "节点添加成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPut("{projectId}/milestones/{milestoneId}")]
    [HasPermission("proj:milestone:edit")]
    public async Task<IActionResult> UpdateMilestone(long projectId, long milestoneId,
        [FromBody] UpdateMilestoneDto dto)
    {
        dto.Id = milestoneId;
        await _projSvc.UpdateMilestoneAsync(projectId, dto, User.GetRealName());
        return Json(ApiResult<object>.Ok("修改成功"));
    }

    [HttpPost("milestones/{milestoneId}/complete")]
    [HasPermission("proj:milestone:done")]
    public async Task<IActionResult> CompleteMilestone(long milestoneId)
    {
        await _projSvc.CompleteMilestoneAsync(milestoneId, User.GetRealName());
        return Json(ApiResult<object>.Ok("节点已标记完成"));
    }

    [HttpDelete("milestones/{milestoneId}")]
    [HasPermission("proj:milestone:edit")]
    public async Task<IActionResult> DeleteMilestone(long milestoneId)
    {
        await _projSvc.DeleteMilestoneAsync(milestoneId);
        return Json(ApiResult<object>.Ok("删除成功"));
    }

    // ── 验收 ───────────────────────────────────────────────
    [HttpPost("{projectId}/acceptances")]
    [HasPermission("proj:acceptance:add")]
    public async Task<IActionResult> AddAcceptance(long projectId,
        [FromBody] CreateAcceptanceDto dto)
    {
        dto.ProjectId = projectId;
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        var id = await _projSvc.AddAcceptanceAsync(dto, User.GetRealName());
        return Json(ApiResult<object>.Ok(new { id }, "验收记录已录入"));
    }

    // ── 合同管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/contracts"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> AddContract(long projectId,
        [FromBody] CreateContractDto dto)
    {
        dto.ProjectId = projectId;
        try
        {
            var id = await _projSvc.AddContractAsync(dto, User.GetRealName());
            return Json(ApiResult<object>.Ok(new { id }, "合同已保存"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    //[HttpPost("contracts/upload/{contractId}")]
    //[HasPermission("proj:project:edit")]
    //public async Task<IActionResult> UploadContractFile(long contractId, IFormFile file)
    //    => await UploadFileToEntity("contracts", contractId, file);

    [HttpPost("{projectId}/contracts/delete/{contractId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteContract(long contractId)
    {
        await _projSvc.DeleteContractAsync(contractId);
        return Json(ApiResult<object>.Ok("合同已删除"));
    }

    // ── 发票管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/invoices"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> AddInvoice(long projectId,
        [FromBody] CreateInvoiceDto dto)
    {
        dto.ProjectId = projectId;
        var id = await _projSvc.AddInvoiceAsync(dto, User.GetRealName());
        return Json(ApiResult<object>.Ok(new { id }, "发票已录入"));
    }

    [HttpPost("invoices/received/{invoiceId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> ConfirmReceived(long invoiceId, DateTime receivedDate)
    {
        await _projSvc.ConfirmInvoiceReceivedAsync(invoiceId, receivedDate, User.GetRealName());
        return Json(ApiResult<object>.Ok("已确认收款"));
    }

    [HttpPost("invoices/delete/{invoiceId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteInvoice(long invoiceId)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return Json(ApiResult<object>.Fail("记录不存在"));
        _uow.ProjInvoices.SoftDelete(inv);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("已删除"));
    }

    [HttpPost("invoices/file/{invoiceId}/{fileType}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadInvoiceFile(long invoiceId, string fileType, IFormFile file)
    {
        if (file == null || file.Length == 0) return Json(ApiResult<object>.Fail("请选择文件"));
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return Json(ApiResult<object>.Fail("记录不存在"));

        var dir = global::System.IO.Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot","uploads","project","invoices");
        global::System.IO.Directory.CreateDirectory(dir);
        var ext  = global::System.IO.Path.GetExtension(file.FileName);
        var save = $"{invoiceId}_{fileType}_{Guid.NewGuid():N}{ext}";
        var fpath = global::System.IO.Path.Combine(dir, save);
        using (var fs = new global::System.IO.FileStream(fpath, global::System.IO.FileMode.Create))
            await file.CopyToAsync(fs);

        if (fileType == "invoice")
        {
            if (!string.IsNullOrEmpty(inv.InvoiceFile) && global::System.IO.File.Exists(inv.InvoiceFile))
                global::System.IO.File.Delete(inv.InvoiceFile);
            inv.InvoiceFile     = fpath;
            inv.InvoiceFileName = file.FileName;
        }
        else
        {
            if (!string.IsNullOrEmpty(inv.PaymentFile) && global::System.IO.File.Exists(inv.PaymentFile))
                global::System.IO.File.Delete(inv.PaymentFile);
            inv.PaymentFile     = fpath;
            inv.PaymentFileName = file.FileName;
        }
        inv.UpdatedBy = User.GetRealName();
        _uow.ProjInvoices.Update(inv);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(new { fileName = file.FileName }, "上传成功"));
    }

    [HttpGet("invoices/file/{invoiceId}/{fileType}")]
    public async Task<IActionResult> DownloadInvoiceFile(long invoiceId, string fileType)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return NotFound();
        var (fp, fn) = fileType == "invoice"
            ? (inv.InvoiceFile, inv.InvoiceFileName)
            : (inv.PaymentFile, inv.PaymentFileName);
        if (string.IsNullOrEmpty(fp) || !global::System.IO.File.Exists(fp))
            return NotFound("文件不存在");
        var bytes = await global::System.IO.File.ReadAllBytesAsync(fp);
        return File(bytes, "application/octet-stream", fn ?? "附件");
    }

    [HttpPost("contracts/file/delete/{contractId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteContractFile(long contractId)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId);
        if (contract == null) return Json(ApiResult<object>.Fail("合同不存在"));
        if (!string.IsNullOrEmpty(contract.FilePath) && global::System.IO.File.Exists(contract.FilePath))
            global::System.IO.File.Delete(contract.FilePath);
        contract.FilePath  = null;
        contract.FileName  = null;
        contract.UpdatedBy = User.GetRealName();
        _uow.ProjContracts.Update(contract);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("附件已删除"));
    }

    [HttpGet("contracts/download/{contractId}")]
    public async Task<IActionResult> DownloadContractFile(long contractId)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId);
        if (contract == null || string.IsNullOrEmpty(contract.FilePath)
            || !global::System.IO.File.Exists(contract.FilePath))
            return NotFound("文件不存在");
        var bytes = await global::System.IO.File.ReadAllBytesAsync(contract.FilePath);
        return File(bytes, "application/octet-stream", contract.FileName ?? "合同附件");
    }

    // 合同附件上传（项目合同）
    [HttpPost("contracts/upload/{contractId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadContractFile(long contractId, IFormFile file)
    {
        if (file == null || file.Length == 0) return Json(ApiResult<object>.Fail("请选择文件"));
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId);
        if (contract == null) return Json(ApiResult<object>.Fail("合同不存在"));

        var dir = global::System.IO.Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot","uploads","project","contracts");
        global::System.IO.Directory.CreateDirectory(dir);
        var ext  = global::System.IO.Path.GetExtension(file.FileName);
        var save = $"{contractId}_{Guid.NewGuid():N}{ext}";
        var fpath = global::System.IO.Path.Combine(dir, save);
        using (var fs = new global::System.IO.FileStream(fpath, global::System.IO.FileMode.Create))
            await file.CopyToAsync(fs);

        if (!string.IsNullOrEmpty(contract.FilePath) && global::System.IO.File.Exists(contract.FilePath))
            global::System.IO.File.Delete(contract.FilePath);
        contract.FilePath  = fpath;
        contract.FileName  = file.FileName;
        contract.UpdatedBy = User.GetRealName();
        _uow.ProjContracts.Update(contract);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(new { fileName = file.FileName }, "上传成功"));
    }

    // ── 文件管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/files")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadFile(long projectId, IFormFile file,
        string category, string? description, string? version)
    {
        if (file == null || file.Length == 0)
            return Json(ApiResult<object>.Fail("请选择文件"));
        if (file.Length > 50 * 1024 * 1024)
            return Json(ApiResult<object>.Fail("文件大小不能超过50MB"));

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot", "uploads", "project", projectId.ToString());
        Directory.CreateDirectory(uploadDir);

        var ext      = Path.GetExtension(file.FileName);
        var saveName = $"{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(uploadDir, saveName);

        using (var fs = new FileStream(savePath, FileMode.Create))
            await file.CopyToAsync(fs);

        var relativePath = $"/uploads/project/{projectId}/{saveName}";
        var fileId = await _projSvc.AddFileAsync(projectId, category,
            file.FileName, savePath, file.Length,
            description, version, User.GetRealName());

        return Json(ApiResult<object>.Ok(new
        {
            id = fileId, fileName = file.FileName,
            filePath = relativePath, fileSize = file.Length
        }, "文件上传成功"));
    }

    [HttpGet("files/download/{fileId}")]
    public async Task<IActionResult> DownloadFile(long fileId)
    {
        var files = await _uow.ProjFiles.GetListAsync(f => f.Id == fileId);
        var f     = files.FirstOrDefault();
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在");
        var bytes = await global::System.IO.File.ReadAllBytesAsync(f.FilePath);
        var mime  = GetMimeType(f.FileExt ?? "bin");
        return File(bytes, mime, f.FileName);
    }

    [HttpPost("files/delete/{fileId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteFile(long fileId)
    {
        await _projSvc.DeleteFileAsync(fileId);
        return Json(ApiResult<object>.Ok("文件已删除"));
    }

    [HttpPost("members/update"), ValidateAntiForgeryToken]
    [HasPermission("proj:member:edit")]
    public async Task<IActionResult> UpdateMember([FromBody] UpdateMemberDto dto)
    {
        try
        {
            await _projSvc.UpdateMemberAsync(0, dto, User.GetRealName());
            return Json(ApiResult<object>.Ok("成员信息已更新"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    // ── 工具方法 ──────────────────────────────────────────────
    private async Task<IActionResult> UploadFileToEntity(string folder, long entityId, IFormFile file)
    {
        if (file == null || file.Length == 0) return Json(ApiResult<object>.Fail("请选择文件"));
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);
        var ext = Path.GetExtension(file.FileName);
        var saveName = $"{entityId}_{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(dir, saveName);
        using (var fs = new FileStream(savePath, FileMode.Create))
            await file.CopyToAsync(fs);
        return Json(ApiResult<object>.Ok(new
        {
            filePath = $"/uploads/{folder}/{saveName}",
            fileName = file.FileName
        }, "上传成功"));
    }

    private static string GetMimeType(string ext) => ext.ToLower() switch
    {
        "pdf"  => "application/pdf",
        "doc"  => "application/msword",
        "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"  => "application/vnd.ms-excel",
        "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "jpg" or "jpeg" => "image/jpeg",
        "png"  => "image/png",
        "zip"  => "application/zip",
        _      => "application/octet-stream",
    };

    private string GetErrors() => string.Join("；",
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
}
