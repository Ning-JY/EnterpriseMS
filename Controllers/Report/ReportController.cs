using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Controllers;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Report;

[Authorize, Route("report")]
public class ReportController : BaseAuthController
{
    private readonly IUnitOfWork  _uow;
    private readonly AppDbContext _db;

    public ReportController(IUnitOfWork uow, AppDbContext db, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _db = db; }

    // ── 回款报表 ────────────────────────────────────────────
    [HttpGet("receipt")]
    [HasPermission("report:receipt")]
    public async Task<IActionResult> Receipt(int? year, int? deptId, string? keyword)
    {
        year ??= DateTime.Today.Year;

        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();

        // 查询有回款记录的项目
        var invoicesQ = _db.ProjInvoices
            .Include(i => i.Project).ThenInclude(p => p!.Dept)
            .Where(i => !i.IsDeleted && i.Project != null && !i.Project.IsDeleted);

        if (year > 0)
            invoicesQ = invoicesQ.Where(i =>
                (i.IsReceived && i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year) ||
                (!i.IsReceived && i.InvoiceDate.HasValue && i.InvoiceDate.Value.Year == year));

        if (deptId.HasValue)
            invoicesQ = invoicesQ.Where(i => i.Project!.DeptId == deptId);

        if (!string.IsNullOrWhiteSpace(keyword))
            invoicesQ = invoicesQ.Where(i => i.Project!.ProjName.Contains(keyword) ||
                                              i.Project.ProjNo.Contains(keyword));

        var invoices = await invoicesQ
            .OrderBy(i => i.Project!.DeptId)
            .ThenBy(i => i.Project!.ProjNo)
            .ThenBy(i => i.InvoiceDate)
            .ToListAsync();

        // 汇总统计
        var totalAmount   = invoices.Sum(i => i.Amount);
        var totalReceived = invoices.Where(i => i.IsReceived).Sum(i => i.Amount);
        var totalPending  = totalAmount - totalReceived;

        // 按部门分组统计
        var byDept = invoices
            .GroupBy(i => new { i.Project!.DeptId, DeptName = i.Project.Dept?.DeptName ?? "未分配" })
            .Select(g => new
            {
                g.Key.DeptName,
                TotalAmount   = g.Sum(i => i.Amount),
                ReceivedAmount= g.Where(i => i.IsReceived).Sum(i => i.Amount),
                PendingAmount = g.Where(i => !i.IsReceived).Sum(i => i.Amount),
                ReceivedCount = g.Count(i => i.IsReceived),
                TotalCount    = g.Count(),
            }).OrderByDescending(g => g.TotalAmount).ToList();

        // 按月分布（仅已收款）
        var byMonth = Enumerable.Range(1, 12).Select(m => new
        {
            Month    = m,
            Amount   = invoices
                .Where(i => i.IsReceived && i.ReceivedDate?.Month == m)
                .Sum(i => i.Amount)
        }).ToList();

        ViewBag.Year         = year;
        ViewBag.Depts        = depts;
        ViewBag.DeptId       = deptId;
        ViewBag.Keyword      = keyword;
        ViewBag.TotalAmount  = totalAmount;
        ViewBag.TotalReceived= totalReceived;
        ViewBag.TotalPending = totalPending;
        ViewBag.ByDept       = byDept;
        ViewBag.ByMonth      = byMonth;
        return View(invoices);
    }

    // ── 产值报表 ────────────────────────────────────────────
    [HttpGet("output")]
    [HasPermission("report:output")]
    public async Task<IActionResult> Output(int? year, int? deptId, string? keyword)
    {
        year ??= DateTime.Today.Year;

        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();

        // 有效项目成员（Status=0在职参与）
        var membersQ = _db.Set<EnterpriseMS.Domain.Entities.Project.ProjectMember>()
            .Include(m => m.Project).ThenInclude(p => p!.Dept)
            .Include(m => m.Employee)
            .Where(m => !m.IsDeleted && m.Status == 0
                     && m.Project != null && !m.Project.IsDeleted
                     && m.Project.ProgressStatus != 9); // 排除已终止

        if (deptId.HasValue)
            membersQ = membersQ.Where(m => m.Employee != null && m.Employee.DeptId == deptId);

        if (!string.IsNullOrWhiteSpace(keyword))
            membersQ = membersQ.Where(m => m.Employee != null &&
                (m.Employee.RealName.Contains(keyword) || m.Project!.ProjName.Contains(keyword)));

        var members = await membersQ
            .OrderBy(m => m.Employee != null ? m.Employee.DeptId : 0)
            .ThenBy(m => m.Employee != null ? m.Employee.RealName : "")
            .ToListAsync();

        // 查当年已收款（用于计算已实现产值）
        var receivedByProj = await _db.ProjInvoices
            .Where(i => !i.IsDeleted && i.IsReceived &&
                        i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year)
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Received = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Received);

        // 按员工汇总
        var byEmployee = members
            .GroupBy(m => new
            {
                EmpId    = m.EmployeeId,
                EmpName  = m.Employee?.RealName ?? "未知",
                DeptName = m.Employee?.Dept?.DeptName ?? "未分配",
            })
            .Select(g =>
            {
                var projList = g.ToList();
                decimal contractValue = 0m, receivedValue = 0m;
                foreach (var pm in projList)
                {
                    var actual   = pm.Project!.ActualContractAmount;
                    contractValue += actual * pm.Ratio / 100;
                    var projRec   = receivedByProj.GetValueOrDefault(pm.ProjectId, 0m);
                    receivedValue += projRec * pm.Ratio / 100;
                }
                return new
                {
                    g.Key.EmpId, g.Key.EmpName, g.Key.DeptName,
                    ProjectCount  = projList.Select(p => p.ProjectId).Distinct().Count(),
                    ContractValue = Math.Round(contractValue, 2),
                    ReceivedValue = Math.Round(receivedValue, 2),
                    Projects      = projList,
                };
            })
            .OrderByDescending(e => e.ContractValue)
            .ToList();

        // 按部门汇总
        var byDept2 = byEmployee
            .GroupBy(e => e.DeptName)
            .Select(g => new
            {
                DeptName      = g.Key,
                EmpCount      = g.Count(),
                ContractValue = g.Sum(e => e.ContractValue),
                ReceivedValue = g.Sum(e => e.ReceivedValue),
            })
            .OrderByDescending(g => g.ContractValue)
            .ToList();

        ViewBag.Year          = year;
        ViewBag.Depts         = depts;
        ViewBag.DeptId        = deptId;
        ViewBag.Keyword       = keyword;
        ViewBag.TotalContract = byEmployee.Sum(e => e.ContractValue);
        ViewBag.TotalReceived = byEmployee.Sum(e => e.ReceivedValue);
        ViewBag.ByDept        = byDept2;
        return View(byEmployee);
    }

    // ── 导出回款报表 Excel ──────────────────────────────────
    [HttpGet("receipt/export")]
    [HasPermission("report:receipt")]
    public async Task<IActionResult> ExportReceipt(int? year, int? deptId, string? keyword)
    {
        year ??= DateTime.Today.Year;

        var invoicesQ = _db.ProjInvoices
            .Include(i => i.Project).ThenInclude(p => p!.Dept)
            .Where(i => !i.IsDeleted && i.Project != null && !i.Project.IsDeleted);

        if (year > 0)
            invoicesQ = invoicesQ.Where(i =>
                (i.IsReceived && i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year) ||
                (!i.IsReceived && i.InvoiceDate.HasValue && i.InvoiceDate.Value.Year == year));

        if (deptId.HasValue) invoicesQ = invoicesQ.Where(i => i.Project!.DeptId == deptId);
        if (!string.IsNullOrWhiteSpace(keyword))
            invoicesQ = invoicesQ.Where(i => i.Project!.ProjName.Contains(keyword));

        var list = await invoicesQ.OrderBy(i => i.Project!.DeptId)
                                  .ThenBy(i => i.Project!.ProjNo)
                                  .ThenBy(i => i.InvoiceDate).ToListAsync();

        var rows = list.Select((i, idx) => new
        {
            序号       = idx + 1,
            部门       = i.Project?.Dept?.DeptName ?? "",
            项目编号   = i.Project?.ProjNo ?? "",
            项目名称   = i.Project?.ProjName ?? "",
            回款批次   = i.ReceiptName,
            发票号     = i.InvoiceNo ?? "",
            发票类型   = i.InvoiceType,
            金额_万元  = i.Amount,
            税率_百分比= i.TaxRate.HasValue ? i.TaxRate.Value.ToString("N1") + "%" : "",
            开票日期   = i.InvoiceDate?.ToString("yyyy-MM-dd") ?? "",
            付款方     = i.Payer ?? "",
            是否收款   = i.IsReceived ? "已收款" : "未收款",
            收款日期   = i.ReceivedDate?.ToString("yyyy-MM-dd") ?? "",
            备注       = i.Remark ?? "",
        });

        var ms = new MemoryStream();
        await ms.SaveAsAsync(rows);
        ms.Seek(0, SeekOrigin.Begin);
        var fileName = Uri.EscapeDataString($"回款报表_{year}年_{DateTime.Now:MMdd}.xlsx");
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"回款报表_{year}年.xlsx");
    }

    // ── 导出产值报表 Excel ──────────────────────────────────
    [HttpGet("output/export")]
    [HasPermission("report:output")]
    public async Task<IActionResult> ExportOutput(int? year, int? deptId, string? keyword)
    {
        year ??= DateTime.Today.Year;

        var membersQ = _db.Set<EnterpriseMS.Domain.Entities.Project.ProjectMember>()
            .Include(m => m.Project).ThenInclude(p => p!.Dept)
            .Include(m => m.Employee).ThenInclude(e => e!.Dept)
            .Where(m => !m.IsDeleted && m.Status == 0
                     && m.Project != null && !m.Project.IsDeleted
                     && m.Project.ProgressStatus != 9);

        if (deptId.HasValue) membersQ = membersQ.Where(m => m.Employee != null && m.Employee.DeptId == deptId);
        if (!string.IsNullOrWhiteSpace(keyword))
            membersQ = membersQ.Where(m => m.Employee != null &&
                m.Employee.RealName.Contains(keyword));

        var members = await membersQ.OrderBy(m => m.Employee!.DeptId).ToListAsync();

        var receivedByProj = await _db.ProjInvoices
            .Where(i => !i.IsDeleted && i.IsReceived &&
                        i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year)
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Received = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Received);

        var rows = members.Select((m, idx) =>
        {
            var actual        = m.Project!.ActualContractAmount;
            var contractValue = Math.Round(actual * m.Ratio / 100, 2);
            var projRec       = receivedByProj.GetValueOrDefault(m.ProjectId, 0m);
            var receivedValue = Math.Round(projRec * m.Ratio / 100, 2);
            return new
            {
                序号      = idx + 1,
                员工部门  = m.Employee?.Dept?.DeptName ?? "",
                员工姓名  = m.Employee?.RealName ?? "",
                项目编号  = m.Project.ProjNo,
                项目名称  = m.Project.ProjName,
                项目部门  = m.Project.Dept?.DeptName ?? "",
                角色      = m.Role,
                占比_百分 = m.Ratio,
                合同产值_万= contractValue,
                已实现产值_万= receivedValue,
                项目状态  = GetProgressText(m.Project.ProgressStatus),
            };
        });

        var ms = new MemoryStream();
        await ms.SaveAsAsync(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"产值报表_{year}年.xlsx");
    }

    private static string GetProgressText(int s) => s switch
    {
        0 => "前期商务", 1 => "预计启动", 2 => "标书制作中", 3 => "投标/磋商中",
        4 => "已中标·签合同中", 5 => "已签回合同", 6 => "执行中",
        7 => "成果提交", 8 => "已完成", 9 => "已终止", _ => "未知"
    };
}
