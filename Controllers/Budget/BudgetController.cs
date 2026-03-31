using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Domain.Entities.Budget;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Budget;

[Authorize, Route("budget")]
public class BudgetController : BaseAuthController
{
    private readonly IUnitOfWork _uow;
    private readonly IDeptService _deptSvc;
    private readonly IEmployeeQueryService _empQrySvc;
    private readonly IOperLogService _logSvc;
    private readonly AppDbContext _db;

    public BudgetController(IUnitOfWork uow, IDeptService deptSvc,
        IEmployeeQueryService empQrySvc, IOperLogService logSvc, AppDbContext db,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _uow = uow; _deptSvc = deptSvc; _empQrySvc = empQrySvc; _logSvc = logSvc; _db = db;
    }

    [HasPermission("budget:task:list")]
    public async Task<IActionResult> Index(string? keyword, int? taskType,
        int? status, int page = 1, int size = 15)
    {
        var q = _uow.BudgetTasks.Query()
            .Include(t => t.Dept)
            .Include(t => t.TechLeader)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(t => t.TaskName.Contains(keyword) || t.OwnerName.Contains(keyword));
        if (taskType.HasValue) q = q.Where(t => t.TaskType == taskType);
        if (status.HasValue)   q = q.Where(t => t.Status == status);

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(t => t.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();

        ViewBag.Depts    = await _deptSvc.GetTreeAsync();
        ViewBag.Members  = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Keyword  = keyword; ViewBag.TaskType = taskType; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)size);
        return View(list);
    }

    [HttpGet("{id}")]
    [HasPermission("budget:task:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var task = await _uow.BudgetTasks.Query(false)
            .Include(t => t.Dept)
            .Include(t => t.TechLeader)
            .Include(t => t.BizLeader)
            .Include(t => t.Sections)
            .Include(t => t.Opinions)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();

        ViewBag.Members = await _empQrySvc.GetAllOnJobAsync();
        return View(task);
    }

    [HttpGet("edit/{id}")]
    [HasPermission("budget:task:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var task = await _uow.BudgetTasks.GetByIdAsync(id);
        if (task == null) return NotFound();
        var depts   = await _deptSvc.GetTreeAsync();
        var members = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Depts   = depts;
        ViewBag.Members = members;
        return View(task);
    }

    [HttpGet("create")]
    [HasPermission("budget:task:add")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Depts   = await _deptSvc.GetTreeAsync();
        ViewBag.Members = await _empQrySvc.GetAllOnJobAsync();
        return View();
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("budget:task:add")]
    public async Task<IActionResult> Create([FromBody] CreateBudgetTaskRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TaskName))
            return Json(ApiResult<object>.Fail("任务名称不能为空"));

        var count = await _uow.BudgetTasks.CountAsync();
        var task = new BudgetTask
        {
            TaskNo       = $"YS-{DateTime.Now.Year}-{(count+1):D3}",
            TaskName     = req.TaskName,
            TaskType     = req.TaskType,
            DeptId       = req.DeptId,
            OwnerName    = req.OwnerName,
            OwnerContact = req.OwnerContact,
            OwnerPhone   = req.OwnerPhone,
            ContractorName = req.ContractorName,
            BuildingScale  = req.BuildingScale,
            SubmitAmount   = req.SubmitAmount,
            QuotaBasis   = req.QuotaBasis,
            FeeStandard  = req.FeeStandard,
            TechLeaderId = req.TechLeaderId,
            BizLeaderId  = req.BizLeaderId,
            PlanDate     = req.PlanDate,
            Status       = 0,
            Remark       = req.Remark,
            CreatedBy    = User.GetRealName(),
        };
        await _uow.BudgetTasks.AddAsync(task);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("新增概预算任务", $"任务：{task.TaskName}", "INSERT", task.Id);
        return Json(ApiResult<object>.Ok(new { id = task.Id }, "任务已创建"));
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("budget:task:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateBudgetTaskRequest req)
    {
        var task = await _uow.BudgetTasks.GetByIdAsync(req.Id);
        if (task == null) return Json(ApiResult<object>.Fail("任务不存在"));

        task.TaskName      = req.TaskName;
        task.OwnerName     = req.OwnerName;
        task.OwnerContact  = req.OwnerContact;
        task.OwnerPhone    = req.OwnerPhone;
        task.ContractorName= req.ContractorName;
        task.BuildingScale = req.BuildingScale;
        task.SubmitAmount  = req.SubmitAmount;
        task.ApprovedAmount= req.ApprovedAmount;
        task.QuotaBasis    = req.QuotaBasis;
        task.FeeStandard   = req.FeeStandard;
        task.TechLeaderId  = req.TechLeaderId;
        task.BizLeaderId   = req.BizLeaderId;
        task.PlanDate      = req.PlanDate;
        task.Remark        = req.Remark;
        task.UpdatedBy     = User.GetRealName();

        // 自动计算核减率
        if (task.SubmitAmount.HasValue && task.ApprovedAmount.HasValue && task.SubmitAmount > 0)
            task.ReductionRate = (task.SubmitAmount - task.ApprovedAmount) / task.SubmitAmount * 100;

        _uow.BudgetTasks.Update(task);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("修改成功"));
    }

    [HttpPost("submit/{id}")]
    [HasPermission("budget:task:submit")]
    public async Task<IActionResult> Submit(long id)
    {
        var task = await _uow.BudgetTasks.GetByIdAsync(id);
        if (task == null) return Json(ApiResult<object>.Fail("任务不存在"));
        task.Status    = 2; // 内审中
        task.UpdatedBy = User.GetRealName();
        _uow.BudgetTasks.Update(task);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("提交内审", $"任务：{task.TaskName}", "UPDATE", id);
        return Json(ApiResult<object>.Ok("已提交内审"));
    }

    // ── 费用分部明细 ─────────────────────────────────────────
    [HttpPost("{taskId}/sections"), ValidateAntiForgeryToken]
    [HasPermission("budget:task:edit")]
    public async Task<IActionResult> AddSection(long taskId,
        [FromBody] CreateSectionRequest req)
    {
        var section = new BudgetSection
        {
            TaskId       = taskId,
            SectionNo    = req.SectionNo,
            SectionName  = req.SectionName,
            Category     = req.Category,
            ContractAmount = req.ContractAmount,
            SubmitAmount = req.SubmitAmount,
            Status       = 0,
            CreatedBy    = User.GetRealName(),
        };
        await _uow.BudgetSections.AddAsync(section);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("分部明细已添加"));
    }

    [HttpPost("sections/{sectionId}/approve"), ValidateAntiForgeryToken]
    [HasPermission("budget:task:edit")]
    public async Task<IActionResult> ApproveSection(long sectionId, decimal approvedAmount)
    {
        var section = await _uow.BudgetSections.GetByIdAsync(sectionId);
        if (section == null) return Json(ApiResult<object>.Fail("记录不存在"));
        section.ApprovedAmount = approvedAmount;
        section.Status         = 2;
        section.UpdatedBy      = User.GetRealName();
        _uow.BudgetSections.Update(section);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("审定金额已录入"));
    }

    // ── 评审意见 ─────────────────────────────────────────────
    [HttpPost("{taskId}/opinions"), ValidateAntiForgeryToken]
    [HasPermission("budget:opinion:add")]
    public async Task<IActionResult> AddOpinion(long taskId,
        [FromBody] CreateOpinionRequest req)
    {
        var count = await _uow.ReviewOpinions.CountAsync(o => o.TaskId == taskId);
        var opinion = new ReviewOpinion
        {
            TaskId        = taskId,
            OpinionNo     = $"YJ-{(count+1):D3}",
            OpinionType   = req.OpinionType,
            Category      = req.Category,
            Amount        = req.Amount,
            Content       = req.Content,
            Basis         = req.Basis,
            ConfirmStatus = 0,
            CreatedBy     = User.GetRealName(),
        };
        await _uow.ReviewOpinions.AddAsync(opinion);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(new { id = opinion.Id }, "意见已录入"));
    }

    [HttpPost("opinions/{opinionId}/confirm")]
    [HasPermission("budget:opinion:add")]
    public async Task<IActionResult> ConfirmOpinion(long opinionId)
    {
        var op = await _uow.ReviewOpinions.GetByIdAsync(opinionId);
        if (op == null) return Json(ApiResult<object>.Fail("意见不存在"));
        op.ConfirmStatus = 1;
        op.UpdatedBy     = User.GetRealName();
        _uow.ReviewOpinions.Update(op);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("已确认"));
    }
}

// ── Request 模型 ──────────────────────────────────────────────
public class CreateBudgetTaskRequest
{
    public string   TaskName      { get; set; } = "";
    public int      TaskType      { get; set; }
    public long?    DeptId        { get; set; }
    public string   OwnerName     { get; set; } = "";
    public string?  OwnerContact  { get; set; }
    public string?  OwnerPhone    { get; set; }
    public string?  ContractorName{ get; set; }
    public string?  BuildingScale { get; set; }
    public decimal? SubmitAmount  { get; set; }
    public string?  QuotaBasis    { get; set; }
    public string?  FeeStandard   { get; set; }
    public long?    TechLeaderId  { get; set; }
    public long?    BizLeaderId   { get; set; }
    public DateTime? PlanDate     { get; set; }
    public string?  Remark        { get; set; }
}
public class UpdateBudgetTaskRequest : CreateBudgetTaskRequest
{
    public long     Id             { get; set; }
    public decimal? ApprovedAmount { get; set; }
}
public class CreateSectionRequest
{
    public int      SectionNo     { get; set; }
    public string   SectionName   { get; set; } = "";
    public string   Category      { get; set; } = "";
    public decimal? ContractAmount { get; set; }
    public decimal? SubmitAmount   { get; set; }
}
public class CreateOpinionRequest
{
    public int      OpinionType { get; set; }
    public string?  Category    { get; set; }
    public decimal  Amount      { get; set; }
    public string   Content     { get; set; } = "";
    public string?  Basis       { get; set; }
}
