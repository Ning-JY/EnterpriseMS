using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Project;

/// <summary>
/// 回款管理（独立聚合页）。
/// 复用 proj_invoice 表（已含 project_id / contract_id 双外键），
/// 与项目详情页内的「合同与回款」共享同一份数据，避免双表不一致。
/// 菜单挂在「项目管理」目录下（/project/receipt）。
/// </summary>
[Authorize, Route("project/receipt")]
public class ReceiptController : BaseAuthController
{
    private readonly IUnitOfWork     _uow;
    private readonly IProjectService _projSvc;

    public ReceiptController(IUnitOfWork uow, IProjectService projSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _uow     = uow;
        _projSvc = projSvc;
    }

    // 容器页
    [HttpGet(""), HasPermission("proj:project:list")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Projects = await _uow.Projects.GetListAsync(p => !p.IsDeleted);
        return View();
    }

    // AJAX 列表（跨项目聚合，支持按项目/合同筛选）
    [HttpGet("list"), HasPermission("proj:project:list")]
    public async Task<IActionResult> List(string? keyword, long? projectId, long? contractId, int page = 1, int size = 15)
    {
        var q = _uow.ProjInvoices.Query();
        q = q.Include(i => i.Project);
        if (projectId.HasValue)  q = q.Where(i => i.ProjectId == projectId.Value);
        if (contractId.HasValue) q = q.Where(i => i.ContractId == contractId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(i => i.ReceiptName.Contains(keyword)
                          || (i.InvoiceNo != null && i.InvoiceNo.Contains(keyword))
                          || (i.Payer != null && i.Payer.Contains(keyword))
                          || (i.Project != null && i.Project.ProjName.Contains(keyword)));

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(i => i.Id)
            .Skip((page - 1) * size).Take(size)
            .Select(i => new ReceiptListDto
            {
                Id           = i.Id,
                ContractId   = i.ContractId,
                ProjectId    = i.ProjectId,
                ProjectName  = i.Project != null ? i.Project.ProjName : "",
                ReceiptName  = i.ReceiptName,
                InvoiceNo    = i.InvoiceNo,
                InvoiceType  = i.InvoiceType,
                Amount       = i.Amount,
                TaxRate      = i.TaxRate,
                InvoiceDate  = i.InvoiceDate,
                Payer        = i.Payer,
                IsReceived   = i.IsReceived,
                ReceivedDate = i.ReceivedDate,
                Remark       = i.Remark,
                CreatedAt    = i.CreatedAt
            }).ToListAsync();

        // 批量补充合同名（避免 N+1）
        var cids = items.Where(x => x.ContractId.HasValue).Select(x => x.ContractId!.Value).Distinct().ToList();
        if (cids.Count > 0)
        {
            var contracts = await _uow.ProjContracts.GetListAsync(c => cids.Contains(c.Id));
            foreach (var it in items)
            {
                if (it.ContractId.HasValue)
                {
                    var c = contracts.FirstOrDefault(x => x.Id == it.ContractId.Value);
                    if (c != null)
                        it.ContractName = string.IsNullOrEmpty(c.ContractName)
                            ? c.ContractNo
                            : $"{c.ContractName} ({c.ContractNo})";
                }
            }
        }

        var paged = new PagedResult<ReceiptListDto> { Total = total, Items = items, Page = page, PageSize = size };
        return ApiOk(paged);
    }

    // 以合同为主：列出合同并聚合回款总额
    [HttpGet("contract-list"), HasPermission("proj:project:list")]
    public async Task<IActionResult> ContractList(string? keyword, long? projectId, int page = 1, int size = 15)
    {
        var q = _uow.ProjContracts.Query().Where(c => !c.IsDeleted);
        if (projectId.HasValue) q = q.Where(c => c.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => (c.ContractName != null && c.ContractName.Contains(keyword))
                          || c.ContractNo.Contains(keyword));

        var total = await q.CountAsync();
        var contracts = await q.Include(c => c.Project)
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync();

        // 聚合每个合同的回款（总额 / 已收 / 笔数）
        var cids = contracts.Select(c => c.Id).ToList();
        var agg = await _uow.ProjInvoices.Query()
            .Where(i => i.ContractId != null && cids.Contains(i.ContractId.Value))
            .GroupBy(i => i.ContractId.Value)
            .Select(g => new
            {
                Cid      = g.Key,
                Total    = g.Sum(x => x.Amount),
                Received = g.Sum(x => x.IsReceived ? x.Amount : 0m),
                Count    = g.Count()
            }).ToListAsync();
        var aggMap = agg.ToDictionary(x => x.Cid, x => x);

        var items = contracts.Select(c =>
        {
            var a = aggMap.TryGetValue(c.Id, out var av) ? av : null;
            var receiptTotal   = a?.Total ?? 0m;
            var receivedTotal  = a?.Received ?? 0m;
            return new ContractReceiptSummaryDto
            {
                ContractId     = c.Id,
                ProjectId      = c.ProjectId,
                ProjectName    = c.Project != null ? c.Project.ProjName : "",
                ContractName   = string.IsNullOrEmpty(c.ContractName) ? c.ContractNo : c.ContractName,
                ContractNo     = c.ContractNo,
                ContractType   = c.ContractType,
                ContractAmount = c.Amount,
                ReceiptTotal   = receiptTotal,
                ReceivedTotal  = receivedTotal,
                UnreceivedTotal= receiptTotal - receivedTotal,
                ReceiptCount   = a?.Count ?? 0
            };
        }).ToList();

        var paged = new PagedResult<ContractReceiptSummaryDto> { Total = total, Items = items, Page = page, PageSize = size };
        return ApiOk(paged);
    }

    // 合同详情：展示该合同下所有回款记录
    [HttpGet("detail/{contractId}"), HasPermission("proj:project:list")]
    public async Task<IActionResult> Detail(long contractId)
    {
        var c = await _uow.ProjContracts.Query()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == contractId && !x.IsDeleted);
        if (c == null) return NotFound();

        var invs = await _uow.ProjInvoices.Query().Where(i => i.ContractId == contractId).ToListAsync();
        ViewBag.Contract       = c;
        ViewBag.ProjectName    = c.Project?.ProjName ?? "";
        ViewBag.ReceiptTotal   = invs.Sum(i => i.Amount);
        ViewBag.ReceivedTotal  = invs.Where(i => i.IsReceived).Sum(i => i.Amount);
        ViewBag.ReceiptCount   = invs.Count;
        return View();
    }

    // 新增 / 编辑表单（弹窗）
    [HttpGet("form"), HasPermission("proj:project:list")]
    public async Task<IActionResult> Form(long? id, long? contractId = null, long? projectId = null)
    {
        ViewBag.Projects = await _uow.Projects.GetListAsync(p => !p.IsDeleted);
        ReceiptListDto? model = null;
        if (id.HasValue)
        {
            var inv = await _uow.ProjInvoices.GetByIdAsync(id.Value);
            if (inv != null)
            {
                model = new ReceiptListDto
                {
                    Id          = inv.Id,
                    ContractId  = inv.ContractId,
                    ProjectId   = inv.ProjectId,
                    ReceiptName = inv.ReceiptName,
                    InvoiceNo   = inv.InvoiceNo,
                    InvoiceType = inv.InvoiceType,
                    Amount      = inv.Amount,
                    TaxRate     = inv.TaxRate,
                    InvoiceDate = inv.InvoiceDate,
                    Payer       = inv.Payer,
                    Remark      = inv.Remark
                };
            }
        }
        ViewBag.Model         = model;
        ViewBag.PreProjectId  = projectId;
        ViewBag.PreContractId = contractId;
        var reloadTable = Request.Query["tableId"].ToString();
        ViewBag.ReloadTable = string.IsNullOrEmpty(reloadTable) ? "receiptTable" : reloadTable;
        return View();
    }

    // 按项目加载合同（前端联动下拉）
    [HttpGet("contracts"), HasPermission("proj:project:list")]
    public async Task<IActionResult> Contracts(long projectId)
    {
        var list = await _uow.ProjContracts.GetListAsync(c => c.ProjectId == projectId && !c.IsDeleted);
        var data = list.Select(c => new
        {
            id   = c.Id,
            name = (string.IsNullOrEmpty(c.ContractName) ? c.ContractNo : c.ContractName)
                 + (string.IsNullOrEmpty(c.ContractNo) ? "" : $" ({c.ContractNo})")
        }).ToList();
        return ApiOk(data);
    }

    [HttpPost("create"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Create([FromForm] CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid) return ApiFail(GetErrors());
        if (dto.ProjectId <= 0)  return ApiFail("请选择关联项目");
        var id = await _projSvc.AddInvoiceAsync(dto, User.GetRealName());
        return ApiOk(new { id }, "回款记录已保存");
    }

    [HttpPost("edit/{id}"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Edit(long id, [FromForm] CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid) return ApiFail(GetErrors());
        await _projSvc.UpdateInvoiceAsync(id, dto, User.GetRealName());
        return ApiOk("已保存");
    }

    [HttpPost("received/{id}"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Received(long id, DateTime receivedDate)
    {
        await _projSvc.ConfirmInvoiceReceivedAsync(id, receivedDate, User.GetRealName());
        return ApiOk("已确认收款");
    }

    [HttpPost("delete/{id}"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _projSvc.DeleteInvoiceAsync(id);
            return ApiOk("已删除");
        }
        catch (NotFoundException ex) { return ApiFail(ex.Message); }
    }
}
