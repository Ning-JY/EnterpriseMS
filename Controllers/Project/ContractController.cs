using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Project;

/// <summary>
/// 合同管理（独立聚合页）。
/// 合同主数据独立管理，通过 proj_contract_link 与项目多对多关联。
/// 菜单挂在「项目管理」目录下（/project/contract）。
/// </summary>
[Authorize, Route("project/contract")]
public class ContractController : BaseAuthController
{
    private readonly IUnitOfWork _uow;
    private readonly IDictService _dictSvc;

    public ContractController(IUnitOfWork uow, IDictService dictSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _uow = uow;
        _dictSvc = dictSvc;
    }

    // 容器页
    [HttpGet(""), HasPermission("proj:project:list")]
    public async Task<IActionResult> Index()
    {
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync(DictType.ProjContractType);
        ViewBag.AllProjects = await _uow.Projects.Query().Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Id).Take(200).ToListAsync();
        return View("~/Views/Project/Contract/Index.cshtml");
    }

    // 详情页（含关联项目）
    [HttpGet("{id}"), HasPermission("proj:project:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var c = await _uow.ProjectContractsNew.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return NotFound();
        var links = await _uow.ProjectContractLinks.Query()
            .Where(l => l.ContractId == id && !l.IsDeleted)
            .Include(l => l.Project)
            .ToListAsync();
        ViewBag.LinkedProjects = links
            .Where(l => l.Project != null && !l.Project.IsDeleted)
            .Select(l => new LinkedProjectDto
            {
                ProjectId = l.ProjectId,
                ProjNo = l.Project!.ProjNo ?? "",
                ProjName = l.Project!.ProjName ?? ""
            }).ToList();
        return View("~/Views/Project/Contract/Detail.cshtml", c);
    }

    // 详情 JSON（供编辑弹窗用）
    [HttpGet("detail-json/{id}"), HasPermission("proj:project:list")]
    public async Task<IActionResult> DetailJson(long id)
    {
        var c = await _uow.ProjectContractsNew.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return ApiFail("合同不存在");
        var links = await _uow.ProjectContractLinks.Query()
            .Where(l => l.ContractId == id && !l.IsDeleted)
            .Include(l => l.Project)
            .ToListAsync();
        return ApiOk(new ContractDto
        {
            Id = c.Id, ContractNo = c.ContractNo, ContractType = c.ContractType,
            ContractName = c.ContractName, PartyA = c.PartyA, PartyB = c.PartyB,
            Amount = c.Amount, SignDate = c.SignDate, StartDate = c.StartDate, EndDate = c.EndDate,
            FilePath = c.FilePath, FileName = c.FileName, Status = c.Status, Remark = c.Remark,
            LinkedProjects = links.Where(l => l.Project != null && !l.Project.IsDeleted)
                .Select(l => new LinkedProjectDto
                {
                    ProjectId = l.ProjectId,
                    ProjNo = l.Project!.ProjNo ?? "",
                    ProjName = l.Project!.ProjName ?? ""
                }).ToList()
        });
    }

    // AJAX 列表
    [HttpGet("list"), HasPermission("proj:project:list")]
    public async Task<IActionResult> List(string? keyword, string? contractType, int page = 1, int size = 15)
    {
        var q = _uow.ProjectContractsNew.Query().Where(c => !c.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => c.ContractNo.Contains(keyword) || (c.ContractName ?? "").Contains(keyword)
                || c.PartyA.Contains(keyword) || c.PartyB.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(contractType))
            q = q.Where(c => c.ContractType == contractType);

        var total = await q.CountAsync();
        var pageItems = await q.OrderByDescending(c => c.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        var pageIds = pageItems.Select(c => c.Id).ToList();
        var linkCounts = await _uow.ProjectContractLinks.Query()
            .Where(l => !l.IsDeleted && pageIds.Contains(l.ContractId))
            .GroupBy(l => l.ContractId)
            .Select(g => new { contractId = g.Key, count = g.Count() })
            .ToListAsync();
        var countMap = linkCounts.ToDictionary(x => x.contractId, x => x.count);
        var items = pageItems.Select(c => new
        {
            c.Id, c.ContractNo, c.ContractType, c.ContractName,
            c.PartyA, c.PartyB, c.Amount, c.SignDate, c.Status,
            linkedCount = countMap.TryGetValue(c.Id, out var n) ? n : 0
        }).ToList();
        return ApiOk(new { total, items });
    }

    // 新建 / 编辑保存
    [HttpPost("save"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Save([FromBody] SaveContractDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ContractNo))
            return ApiFail("合同编号不能为空");

        Contract c;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            c = await _uow.ProjectContractsNew.GetByIdAsync(dto.Id.Value);
            if (c == null || c.IsDeleted) return ApiFail("合同不存在");
        }
        else
        {
            c = new Contract { CreatedAt = DateTime.UtcNow, CreatedBy = User.GetUserId().ToString() };
            await _uow.ProjectContractsNew.AddAsync(c);
        }

        c.ContractNo = dto.ContractNo.Trim();
        c.ContractType = dto.ContractType ?? "主合同";
        c.ContractName = dto.ContractName;
        c.PartyA = dto.PartyA ?? "";
        c.PartyB = dto.PartyB ?? "";
        c.Amount = dto.Amount;
        c.SignDate = dto.SignDate;
        c.StartDate = dto.StartDate;
        c.EndDate = dto.EndDate;
        c.Remark = dto.Remark;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = User.GetUserId().ToString();
        await _uow.SaveChangesAsync();

        // 同步关联项目
        var wantIds = dto.ProjectIds?.Distinct().ToList() ?? new();
        var existing = await _uow.ProjectContractLinks.Query()
            .Where(l => l.ContractId == c.Id && !l.IsDeleted).ToListAsync();
        var existingIds = existing.Select(l => l.ProjectId).ToHashSet();

        foreach (var pid in wantIds.Where(p => !existingIds.Contains(p)))
        {
            await _uow.ProjectContractLinks.AddAsync(new ProjectContractLink
            {
                ProjectId = pid, ContractId = c.Id,
                CreatedAt = DateTime.UtcNow, CreatedBy = User.GetUserId().ToString()
            });
        }
        foreach (var link in existing.Where(l => !wantIds.Contains(l.ProjectId)))
        {
            link.IsDeleted = true;
            link.UpdatedAt = DateTime.UtcNow;
        }
        await _uow.SaveChangesAsync();

        return ApiOk(new { id = c.Id });
    }

    // 删除合同（仅当未关联任何项目时允许彻底删除，否则先取消关联）
    [HttpPost("delete/{id}"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> Delete(long id)
    {
        var c = await _uow.ProjectContractsNew.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return ApiFail("合同不存在");
        var linkCount = await _uow.ProjectContractLinks.Query()
            .CountAsync(l => l.ContractId == id && !l.IsDeleted);
        if (linkCount > 0)
            return ApiFail($"该合同已关联 {linkCount} 个项目，请先取消关联后再删除");
        c.IsDeleted = true;
        c.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return ApiOk(new { });
    }

    // 关联项目
    [HttpPost("{id}/link"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> LinkProject(long id, [FromBody] LinkProjectDto dto)
    {
        var c = await _uow.ProjectContractsNew.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return ApiFail("合同不存在");
        var exists = await _uow.ProjectContractLinks.Query()
            .AnyAsync(l => l.ContractId == id && l.ProjectId == dto.ProjectId && !l.IsDeleted);
        if (exists) return ApiFail("已关联该项目");
        await _uow.ProjectContractLinks.AddAsync(new ProjectContractLink
        {
            ProjectId = dto.ProjectId, ContractId = id,
            CreatedAt = DateTime.UtcNow, CreatedBy = User.GetUserId().ToString()
        });
        await _uow.SaveChangesAsync();
        return ApiOk(new { });
    }

    // 取消关联项目
    [HttpPost("{id}/unlink/{projectId}"), ValidateAntiForgeryToken, HasPermission("proj:project:list")]
    public async Task<IActionResult> UnlinkProject(long id, long projectId)
    {
        var link = await _uow.ProjectContractLinks.Query()
            .FirstOrDefaultAsync(l => l.ContractId == id && l.ProjectId == projectId && !l.IsDeleted);
        if (link == null) return ApiFail("关联不存在");
        link.IsDeleted = true;
        link.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return ApiOk(new { });
    }

    // 供项目页"关联已有合同"下拉用
    [HttpGet("options"), HasPermission("proj:project:list")]
    public async Task<IActionResult> Options(string? keyword)
    {
        var q = _uow.ProjectContractsNew.Query().Where(c => !c.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => c.ContractNo.Contains(keyword) || (c.ContractName ?? "").Contains(keyword));
        var items = await q.OrderByDescending(c => c.Id).Take(50)
            .Select(c => new { c.Id, c.ContractNo, c.ContractName, c.PartyA, c.Amount }).ToListAsync();
        return ApiOk(items);
    }

    public class LinkProjectDto
    {
        public long ProjectId { get; set; }
    }
}
