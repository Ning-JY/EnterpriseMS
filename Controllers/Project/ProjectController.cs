using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Domain.Constants;
using Microsoft.AspNetCore.Http;
using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using EnterpriseMS.Services.Impl;
using Microsoft.EntityFrameworkCore;

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
    private readonly IReportGeneratorService _reportSvc;

    public ProjectController(IProjectService projSvc, IDeptService deptSvc,
        IDictService dictSvc, IEmployeeQueryService empQrySvc,
        IOperLogService logSvc, IUnitOfWork uow, IPermissionService permSvc,
        IReportGeneratorService reportSvc)
        : base(permSvc)
    {
        _projSvc = projSvc; _deptSvc = deptSvc; _dictSvc = dictSvc;
        _empQrySvc = empQrySvc; _logSvc = logSvc; _uow = uow; _reportSvc = reportSvc;
    }  [HasPermission("proj:project:list")]
    public async Task<IActionResult> Index(ProjectQueryDto query)
    {
        var result  = await _projSvc.GetPagedAsync(query, User.GetUserId());
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
        var proj = await _projSvc.GetDetailAsync(id, User.GetUserId());
        if (proj == null) return NotFound();
        ViewBag.AllMembers        = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.DictMilestoneType = await _dictSvc.GetDataByTypeAsync(DictType.MilestoneType);
        return View(proj);
    }

    // ── 成果报告：选择模板 → 自动填充项目详情 → 生成 Word ──
    [HttpGet("{projectId}/report/generate")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> GenerateReport(long projectId, string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            return BadRequest("请选择报告模板");
        var proj = await _projSvc.GetDetailAsync(projectId, User.GetUserId());
        if (proj == null) return NotFound("项目不存在或无访问权限");

        try
        {
            var fields   = _projSvc.BuildReportFieldValues(proj);
            var bytes    = _reportSvc.GenerateDocument(templateId, fields);
            var tpl      = _reportSvc.GetTemplate(templateId);
            var fileName = $"{proj.ProjName}_{(tpl?.Name ?? "成果报告")}.docx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return BadRequest(ex.Message);
        
        
        }
    }

    // ── 成果报告：预览字段（自动带入 + 待补填），供弹窗展示 ──
    [HttpGet("{projectId}/report/preview-fields")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> ReportPreviewFields(long projectId, string templateId)
    {
        var proj = await _projSvc.GetDetailAsync(projectId, User.GetUserId());
        if (proj == null) return NotFound("项目不存在");
        var tpl = _reportSvc.GetTemplate(templateId);
        if (tpl == null) return BadRequest("模板不存在");

        // 用默认值预填 manual 字段，便于预览时计算字段也能显示
        var defaults = tpl.Fields
            .Where(f => f.Source == "manual")
            .ToDictionary(f => f.Name, f => f.DefaultValue ?? "");
        var all = await _projSvc.BuildReportFieldValuesAsync(proj, tpl, defaults);

        var autoFields = tpl.Fields
            .Where(f => f.Source != "manual")
            .Select(f => new { name = f.Name, label = f.Label, value = all.GetValueOrDefault(f.Name, ""), source = f.Source })
            .ToList();
        var manualFields = tpl.Fields
            .Where(f => f.Source == "manual")
            .Select(f => new
            {
                name = f.Name,
                label = f.Label,
                required = f.Required,
                type = f.Type,
                defaultValue = f.DefaultValue ?? "",
                helpText = f.HelpText ?? "",
                options = f.Options
            })
            .ToList();

        return ApiOk(new { templateId, templateName = tpl.Name, autoFields, manualFields });
    }

    // ── 成果报告：根据项目 + 用户补填字段生成 Word ──
    [HttpPost("{projectId}/report/generate")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> ReportGenerateFromProject(long projectId, [FromBody] ReportGenerateFromProjectRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TemplateId))
            return BadRequest("请选择报告模板");
        var proj = await _projSvc.GetDetailAsync(projectId, User.GetUserId());
        if (proj == null) return NotFound("项目不存在");
        var tpl = _reportSvc.GetTemplate(req.TemplateId);
        if (tpl == null) return BadRequest("模板不存在");

        try
        {
            var fields = await _projSvc.BuildReportFieldValuesAsync(proj, tpl, req.Fields);
            var bytes = _reportSvc.GenerateDocument(tpl.Id, fields);
            var fileName = $"{proj.ProjName}_{tpl.Name}.docx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return BadRequest(ex.Message);
        
        
        }
    }

    [HttpGet("edit/{id}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var proj = await _projSvc.GetDetailAsync(id, User.GetUserId());
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
        // 项目编号前缀改为字典驱动（可在字典管理中动态增删；默认取 IsDefault 项）
        var prefixes = await _dictSvc.GetDataByTypeAsync(DictType.ProjNoPrefix);
        var defaultPrefix = prefixes.FirstOrDefault(p => p.IsDefault == 1)?.DictValue
                          ?? prefixes.FirstOrDefault()?.DictValue ?? "";
        var suffix = await _projSvc.GenerateProjNoSuffixAsync();
        ViewBag.Depts          = depts;
        ViewBag.Members        = members;
        ViewBag.ProjNoPrefixes = prefixes;
        ViewBag.ProjNoPrefix   = defaultPrefix;
        ViewBag.GeneratedNo    = suffix;
        return View();
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:add")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        try
        {
            var id = await _projSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新建项目", $"项目：{dto.ProjName}", "INSERT", id);
            return ApiOk(new { id }, "项目创建成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        
        }
    }

    // ── 投标建项：精简列表 + 一键转项目 ──────────────────────
    [HttpGet("simple-list")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> SimpleList()
    {
        var list = await _projSvc.GetSimpleListAsync();
        return ApiOk(list);
    }

    [HttpPost("quick-create"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:add")]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        try
        {
            var id = await _projSvc.QuickCreateAsync(dto, User.GetRealName());
            return ApiOk(new { id, projName = dto.ProjName }, "项目已创建");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        
        }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        try
        {
            await _projSvc.UpdateAsync(dto, User.GetRealName());
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
    }

    [HttpPost("status"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:status")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusDto dto)
    {
        try
        {
            await _projSvc.ChangeStatusAsync(dto, User.GetRealName());
            return ApiOk("状态已更新");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
    }

    [HttpPost("terminate")]
    [HasPermission("proj:project:terminate")]
    public async Task<IActionResult> Terminate(long id, string reason)
    {
        try
        {
            await _projSvc.TerminateAsync(id, reason, User.GetRealName());
            return ApiOk("项目已终止");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
    }

    // ── 成员 ───────────────────────────────────────────────
    [HttpPost("{projectId}/members")]
    [HasPermission("proj:member:add")]
    public async Task<IActionResult> AddMember(long projectId, [FromBody] CreateMemberDto dto)
    {
        try
        {
            var id = await _projSvc.AddMemberAsync(projectId, dto, User.GetRealName());
            return ApiOk(new { id }, "成员添加成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        
        }
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
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
    }

    [HttpPost("{projectId}/members/{memberId}/remove")]
    [HasPermission("proj:member:add")]
    public async Task<IActionResult> RemoveMember(long projectId, long memberId)
    {
        try
        {
            await _projSvc.RemoveMemberAsync(projectId, memberId, User.GetRealName());
            await _logSvc.LogAsync("移除项目成员", $"项目ID:{projectId} 成员ID:{memberId}", "UPDATE", projectId);
            return ApiOk("成员已移除");
        }
        catch (NotFoundException ex)
        { return ApiFail(ex.Message); }
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
            return ApiOk(new { id }, "节点添加成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        
        }
    }

    [HttpPut("{projectId}/milestones/{milestoneId}")]
    [HasPermission("proj:milestone:edit")]
    public async Task<IActionResult> UpdateMilestone(long projectId, long milestoneId,
        [FromBody] UpdateMilestoneDto dto)
    {
        dto.Id = milestoneId;
        await _projSvc.UpdateMilestoneAsync(projectId, dto, User.GetRealName());
        return ApiOk("修改成功");
    }

    [HttpPost("milestones/{milestoneId}/complete")]
    [HasPermission("proj:milestone:done")]
    public async Task<IActionResult> CompleteMilestone(long milestoneId)
    {
        await _projSvc.CompleteMilestoneAsync(milestoneId, User.GetRealName());
        return ApiOk("节点已标记完成");
    }

    [HttpDelete("milestones/{milestoneId}")]
    [HasPermission("proj:milestone:edit")]
    public async Task<IActionResult> DeleteMilestone(long milestoneId)
    {
        await _projSvc.DeleteMilestoneAsync(milestoneId);
        return ApiOk("删除成功");
    }

    // ── 验收 ───────────────────────────────────────────────
    [HttpPost("{projectId}/acceptances")]
    [HasPermission("proj:acceptance:add")]
    public async Task<IActionResult> AddAcceptance(long projectId,
        [FromBody] CreateAcceptanceDto dto)
    {
        dto.ProjectId = projectId;
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        var id = await _projSvc.AddAcceptanceAsync(dto, User.GetRealName());
        return ApiOk(new { id }, "验收记录已录入");
    }

    // ── 合同管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/contracts"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> AddContract(long projectId,
        [FromForm] CreateContractDto dto, IFormFile? file)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        dto.ProjectId = projectId;
        try
        {
            var id = await _projSvc.AddContractAsync(dto, User.GetRealName());

            // 一步上传附件：统一走 FileUploadHelper（含白名单校验）+ 服务方法持久化，
            // 不再在 Controller 内手写 FileStream / 直连 _uow（收敛分层泄漏，达成上传校验一致）
            var saved = await FileUploadHelper.SaveUploadFile(file, "project/contracts");
            if (saved.HasValue)
            {
                await _projSvc.UploadContractFileAsync(id, saved.Value.name,
                    saved.Value.path, User.GetRealName());
            }

            return ApiOk(new { id }, "合同已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
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
        return ApiOk("合同已删除");
    }

    // ── 发票管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/invoices"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> AddInvoice(long projectId,
        [FromForm] CreateInvoiceDto dto, IFormFile? invoiceFile, IFormFile? paymentFile)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        dto.ProjectId = projectId;
        var id = await _projSvc.AddInvoiceAsync(dto, User.GetRealName());

        // 一步上传发票 / 回款附件：统一走 FileUploadHelper + 服务方法，消除 Controller 手写文件流
        var invSaved = await FileUploadHelper.SaveUploadFile(invoiceFile, "project/invoices");
        if (invSaved.HasValue)
        {
            await _projSvc.UploadInvoiceFileAsync(id, "invoice", invSaved.Value.name,
                invSaved.Value.path, User.GetRealName());
        }

        var paySaved = await FileUploadHelper.SaveUploadFile(paymentFile, "project/invoices");
        if (paySaved.HasValue)
        {
            await _projSvc.UploadInvoiceFileAsync(id, "payment", paySaved.Value.name,
                paySaved.Value.path, User.GetRealName());
        }

        return ApiOk(new { id }, "回款记录已保存");
    }

    [HttpPost("invoices/received/{invoiceId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> ConfirmReceived(long invoiceId, DateTime receivedDate)
    {
        await _projSvc.ConfirmInvoiceReceivedAsync(invoiceId, receivedDate, User.GetRealName());
        return ApiOk("已确认收款");
    }

    [HttpPost("invoices/delete/{invoiceId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteInvoice(long invoiceId)
    {
        try
        {
            await _projSvc.DeleteInvoiceAsync(invoiceId);
            return ApiOk("已删除");
        }
        catch (NotFoundException ex) { return ApiFail(ex.Message); }
    }

    [HttpPost("invoices/file/{invoiceId}/{fileType}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadInvoiceFile(long invoiceId, string fileType, IFormFile file)
    {
        if (file == null || file.Length == 0) return ApiFail("请选择文件");
        var saved = await FileUploadHelper.SaveUploadFile(file, "project/invoices");
        if (!saved.HasValue)
            return ApiFail("文件类型不被允许");
        // 经统一上传辅助 + 服务方法持久化，收敛 Controller 手写文件流
        await _projSvc.UploadInvoiceFileAsync(invoiceId, fileType, saved.Value.name,
            saved.Value.path, User.GetRealName());
        return ApiOk(new { fileName = saved.Value.name }, "上传成功");
    }

    [HttpGet("invoices/file/{invoiceId}/{fileType}")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> DownloadInvoiceFile(long invoiceId, string fileType)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return NotFound();
        var (fp, fn) = fileType == "invoice"
            ? (inv.InvoiceFile, inv.InvoiceFileName)
            : (inv.PaymentFile, inv.PaymentFileName);
        if (string.IsNullOrEmpty(fp) || !global::System.IO.File.Exists(fp))
            return NotFound("文件不存在");
        return FileServingHelper.ServePhysicalFile(fp, fn ?? "附件", global::System.IO.Path.GetExtension(fp));
    }

    [HttpPost("contracts/file/delete/{contractId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteContractFile(long contractId)
    {
        try
        {
            await _projSvc.DeleteContractFileAsync(contractId, User.GetRealName());
            return ApiOk("附件已删除");
        }
        catch (NotFoundException ex) { return ApiFail(ex.Message); }
    }

    [HttpGet("contracts/download/{contractId}")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> DownloadContractFile(long contractId)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId);
        if (contract == null || string.IsNullOrEmpty(contract.FilePath)
            || !global::System.IO.File.Exists(contract.FilePath))
            return NotFound("文件不存在");
        return FileServingHelper.ServePhysicalFile(contract.FilePath, contract.FileName ?? "合同附件",
            global::System.IO.Path.GetExtension(contract.FilePath));
    }

    // 合同附件上传（项目合同）
    [HttpPost("contracts/upload/{contractId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadContractFile(long contractId, IFormFile file)
    {
        if (file == null || file.Length == 0) return ApiFail("请选择文件");
        var saved = await FileUploadHelper.SaveUploadFile(file, "project/contracts");
        if (!saved.HasValue)
            return ApiFail("文件类型不被允许");
        // 经统一上传辅助 + 服务方法持久化，收敛 Controller 手写文件流
        await _projSvc.UploadContractFileAsync(contractId, saved.Value.name,
            saved.Value.path, User.GetRealName());
        return ApiOk(new { fileName = saved.Value.name }, "上传成功");
    }

    // ── 文件管理 ──────────────────────────────────────────────
    [HttpPost("{projectId}/files")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> UploadFile(long projectId, IFormFile file,
        string category, string? description, string? version)
    {
        if (file == null || file.Length == 0)
            return ApiFail("请选择文件");

        // 经统一上传辅助（白名单由 FileUploadHelper 单一管控，大小由全局 500MB 限制），
        // 文件落非 Web 根目录，从根上消除存储型 XSS 与手写文件流。
        var saved = await FileUploadHelper.SaveUploadFile(file, $"project/{projectId}");
        if (!saved.HasValue)
            return ApiFail("文件类型不被允许");

        var fileId = await _projSvc.AddFileAsync(projectId, category,
            saved.Value.name, saved.Value.path, file.Length,
            description, version, User.GetRealName());

        return ApiOk(new
        {
            id = fileId, fileName = saved.Value.name, fileSize = file.Length
        }, "文件上传成功");
    }

    [HttpGet("files/download/{fileId}")]
    [HasPermission("proj:project:list")]
    public async Task<IActionResult> DownloadFile(long fileId)
    {
        var files = await _uow.ProjFiles.GetListAsync(f => f.Id == fileId);
        var f     = files.FirstOrDefault();
        if (f == null || !global::System.IO.File.Exists(f.FilePath))
            return NotFound("文件不存在");
        return FileServingHelper.ServePhysicalFile(f.FilePath, f.FileName, f.FileExt);
    }

    [HttpPost("files/delete/{fileId}")]
    [HasPermission("proj:project:edit")]
    public async Task<IActionResult> DeleteFile(long fileId)
    {
        await _projSvc.DeleteFileAsync(fileId);
        return ApiOk("文件已删除");
    }

    [HttpPost("members/update"), ValidateAntiForgeryToken]
    [HasPermission("proj:member:edit")]
    public async Task<IActionResult> UpdateMember([FromBody] UpdateMemberDto dto)
    {
        try
        {
            await _projSvc.UpdateMemberAsync(0, dto, User.GetRealName());
            return ApiOk("成员信息已更新");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message); 
        }
    }

    // ── 工具方法 ──────────────────────────────────────────────
}
