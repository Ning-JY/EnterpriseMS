using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Controllers;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Report;

[Authorize, Route("report")]
public class ReportController : BaseAuthController
{
    private readonly IReportService _reportSvc;

    public ReportController(IReportService reportSvc, IPermissionService permSvc)
        : base(permSvc)
    { _reportSvc = reportSvc; }

    // ── 回款报表 ────────────────────────────────────────────
    [HttpGet("receipt")]
    [HasPermission("report:receipt")]
    public async Task<IActionResult> Receipt(int? year, int? deptId, string? keyword)
    {
        var dto = await _reportSvc.GetReceiptReportAsync(year, deptId, keyword);
        ViewBag.Depts        = dto.Depts;
        ViewBag.Year         = dto.Year;
        ViewBag.DeptId       = dto.DeptId;
        ViewBag.Keyword      = dto.Keyword;
        ViewBag.TotalAmount  = dto.TotalAmount;
        ViewBag.TotalReceived= dto.TotalReceived;
        ViewBag.TotalPending = dto.TotalPending;
        ViewBag.ByDept       = dto.ByDept;
        ViewBag.ByMonth      = dto.ByMonth;
        return View(dto.Invoices);
    }

    // ── 产值报表 ────────────────────────────────────────────
    [HttpGet("output")]
    [HasPermission("report:output")]
    public async Task<IActionResult> Output(int? year, int? deptId, string? keyword)
    {
        var dto = await _reportSvc.GetOutputReportAsync(year, deptId, keyword);
        ViewBag.Depts         = dto.Depts;
        ViewBag.Year          = dto.Year;
        ViewBag.DeptId        = dto.DeptId;
        ViewBag.Keyword       = dto.Keyword;
        ViewBag.TotalContract = dto.TotalContract;
        ViewBag.TotalReceived = dto.TotalReceived;
        ViewBag.ByDept        = dto.ByDept;
        return View(dto.Employees);
    }

    // ── 导出回款报表 Excel ──────────────────────────────────
    [HttpGet("receipt/export")]
    [HasPermission("report:receipt")]
    public async Task<IActionResult> ExportReceipt(int? year, int? deptId, string? keyword)
    {
        var (bytes, fileName) = await _reportSvc.ExportReceiptAsync(year, deptId, keyword);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ── 导出产值报表 Excel ──────────────────────────────────
    [HttpGet("output/export")]
    [HasPermission("report:output")]
    public async Task<IActionResult> ExportOutput(int? year, int? deptId, string? keyword)
    {
        var (bytes, fileName) = await _reportSvc.ExportOutputAsync(year, deptId, keyword);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
