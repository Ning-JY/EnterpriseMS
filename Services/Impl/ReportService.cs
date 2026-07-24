using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 报表服务：把原 ReportController 内联的 EF 查询与聚合逻辑收敛到 Service 层。
/// 统一通过 IUnitOfWork 仓储访问数据，Controller 不再直连 DbContext。
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    { _uow = uow; }

    // ── 回款报表 ────────────────────────────────────────────
    public async Task<ReceiptReportDto> GetReceiptReportAsync(int? year, long? deptId, string? keyword)
    {
        year ??= DateTime.UtcNow.Date.Year;

        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();

        var invoicesQ = _uow.ProjInvoices.Query()
            .Include(i => i.Project).ThenInclude(p => p.Dept)
            .Where(i => !i.IsDeleted && i.Project != null && !i.Project.IsDeleted);

        if (year > 0)
            invoicesQ = invoicesQ.Where(i =>
                (i.IsReceived && i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year) ||
                (!i.IsReceived && i.InvoiceDate.HasValue && i.InvoiceDate.Value.Year == year));

        if (deptId.HasValue)
            invoicesQ = invoicesQ.Where(i => i.Project.DeptId == deptId);

        if (!string.IsNullOrWhiteSpace(keyword))
            invoicesQ = invoicesQ.Where(i => i.Project.ProjName.Contains(keyword) ||
                                              i.Project.ProjNo.Contains(keyword));

        var invoices = await invoicesQ
            .OrderBy(i => i.Project.DeptId)
            .ThenBy(i => i.Project.ProjNo)
            .ThenBy(i => i.InvoiceDate)
            .ToListAsync();

        var totalAmount   = invoices.Sum(i => i.Amount);
        var totalReceived = invoices.Where(i => i.IsReceived).Sum(i => i.Amount);
        var totalPending  = totalAmount - totalReceived;

        var byDept = invoices
            .GroupBy(i => new { i.Project.DeptId, DeptName = i.Project.Dept?.DeptName ?? "未分配" })
            .Select(g => new ReceiptByDeptDto
            {
                DeptName      = g.Key.DeptName,
                TotalAmount   = g.Sum(i => i.Amount),
                ReceivedAmount= g.Where(i => i.IsReceived).Sum(i => i.Amount),
                PendingAmount = g.Where(i => !i.IsReceived).Sum(i => i.Amount),
                ReceivedCount = g.Count(i => i.IsReceived),
                TotalCount    = g.Count(),
            }).OrderByDescending(g => g.TotalAmount).ToList();

        var byMonth = Enumerable.Range(1, 12).Select(m => new ReceiptByMonthDto
        {
            Month  = m,
            Amount = invoices
                .Where(i => i.IsReceived && i.ReceivedDate?.Month == m)
                .Sum(i => i.Amount)
        }).ToList();

        return new ReceiptReportDto
        {
            Depts        = depts,
            Year         = year,
            DeptId       = deptId,
            Keyword      = keyword,
            TotalAmount  = totalAmount,
            TotalReceived= totalReceived,
            TotalPending = totalPending,
            ByDept       = byDept,
            ByMonth      = byMonth,
            Invoices     = invoices,
        };
    }

    // ── 产值报表 ────────────────────────────────────────────
    public async Task<OutputReportDto> GetOutputReportAsync(int? year, long? deptId, string? keyword)
    {
        year ??= DateTime.UtcNow.Date.Year;

        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();

        var membersQ = _uow.ProjMembers.Query()
            .Include(m => m.Project).ThenInclude(p => p.Dept)
            .Include(m => m.Employee)
            .Where(m => !m.IsDeleted && m.Status == 0
                     && m.Project != null && !m.Project.IsDeleted
                     && m.Project.ProgressStatus != 9); // 排除已终止

        if (deptId.HasValue)
            membersQ = membersQ.Where(m => m.Employee != null && m.Employee.DeptId == deptId);

        if (!string.IsNullOrWhiteSpace(keyword))
            membersQ = membersQ.Where(m => m.Employee != null &&
                (m.Employee.RealName.Contains(keyword) || m.Project.ProjName.Contains(keyword)));

        var members = await membersQ
            .OrderBy(m => m.Employee != null ? m.Employee.DeptId : 0)
            .ThenBy(m => m.Employee != null ? m.Employee.RealName : "")
            .ToListAsync();

        var receivedByProj = await _uow.ProjInvoices.Query()
            .Where(i => !i.IsDeleted && i.IsReceived &&
                        i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year)
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Received = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Received);

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
                return new OutputEmployeeRowDto
                {
                    EmpId         = g.Key.EmpId,
                    EmpName       = g.Key.EmpName,
                    DeptName      = g.Key.DeptName,
                    ProjectCount  = projList.Select(p => p.ProjectId).Distinct().Count(),
                    ContractValue = Math.Round(contractValue, 2),
                    ReceivedValue = Math.Round(receivedValue, 2),
                    Projects      = projList,
                };
            })
            .OrderByDescending(e => e.ContractValue)
            .ToList();

        var byDept2 = byEmployee
            .GroupBy(e => e.DeptName)
            .Select(g => new OutputByDeptDto
            {
                DeptName      = g.Key,
                EmpCount      = g.Count(),
                ContractValue = g.Sum(e => e.ContractValue),
                ReceivedValue = g.Sum(e => e.ReceivedValue),
            })
            .OrderByDescending(g => g.ContractValue)
            .ToList();

        return new OutputReportDto
        {
            Depts         = depts,
            Year          = year,
            DeptId        = deptId,
            Keyword       = keyword,
            TotalContract = byEmployee.Sum(e => e.ContractValue),
            TotalReceived = byEmployee.Sum(e => e.ReceivedValue),
            ByDept        = byDept2,
            Employees     = byEmployee,
        };
    }

    // ── 导出回款报表 Excel ──────────────────────────────────
    public async Task<(byte[] Bytes, string FileName)> ExportReceiptAsync(int? year, long? deptId, string? keyword)
    {
        year ??= DateTime.UtcNow.Date.Year;

        var invoicesQ = _uow.ProjInvoices.Query()
            .Include(i => i.Project).ThenInclude(p => p.Dept)
            .Where(i => !i.IsDeleted && i.Project != null && !i.Project.IsDeleted);

        if (year > 0)
            invoicesQ = invoicesQ.Where(i =>
                (i.IsReceived && i.ReceivedDate.HasValue && i.ReceivedDate.Value.Year == year) ||
                (!i.IsReceived && i.InvoiceDate.HasValue && i.InvoiceDate.Value.Year == year));

        if (deptId.HasValue) invoicesQ = invoicesQ.Where(i => i.Project.DeptId == deptId);
        if (!string.IsNullOrWhiteSpace(keyword))
            invoicesQ = invoicesQ.Where(i => i.Project.ProjName.Contains(keyword));

        var list = await invoicesQ.OrderBy(i => i.Project.DeptId)
                                  .ThenBy(i => i.Project.ProjNo)
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

        using var ms = new MemoryStream();
        await ms.SaveAsAsync(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return (ms.ToArray(), $"回款报表_{year}年.xlsx");
    }

    // ── 导出产值报表 Excel ──────────────────────────────────
    public async Task<(byte[] Bytes, string FileName)> ExportOutputAsync(int? year, long? deptId, string? keyword)
    {
        year ??= DateTime.UtcNow.Date.Year;

        var membersQ = _uow.ProjMembers.Query()
            .Include(m => m.Project).ThenInclude(p => p.Dept)
            .Include(m => m.Employee).ThenInclude(e => e.Dept)
            .Where(m => !m.IsDeleted && m.Status == 0
                     && m.Project != null && !m.Project.IsDeleted
                     && m.Project.ProgressStatus != 9);

        if (deptId.HasValue) membersQ = membersQ.Where(m => m.Employee != null && m.Employee.DeptId == deptId);
        if (!string.IsNullOrWhiteSpace(keyword))
            membersQ = membersQ.Where(m => m.Employee != null &&
                m.Employee.RealName.Contains(keyword));

        var members = await membersQ.OrderBy(m => m.Employee!.DeptId).ToListAsync();

        var receivedByProj = await _uow.ProjInvoices.Query()
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
                项目状态  = ProjectProgress.GetProgressText(m.Project.ProgressStatus),
            };
        });

        using var ms = new MemoryStream();
        await ms.SaveAsAsync(rows);
        ms.Seek(0, SeekOrigin.Begin);
        return (ms.ToArray(), $"产值报表_{year}年.xlsx");
    }
}
