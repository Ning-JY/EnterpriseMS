using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Hr;

[Authorize, Route("hr/employee")]
public class HrImportController : BaseAuthController
{
    private readonly IUnitOfWork     _uow;
    private readonly AppDbContext    _db;
    private readonly IDeptService    _deptSvc;
    private readonly IOperLogService _logSvc;

    public HrImportController(IUnitOfWork uow, AppDbContext db,
        IDeptService deptSvc, IOperLogService logSvc, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _db = db; _deptSvc = deptSvc; _logSvc = logSvc; }

    // ── 导入页面 ─────────────────────────────────────────────
    [HttpGet("import")]
    [HasPermission("hr:employee:import")]
    public async Task<IActionResult> Import()
    {
        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();
        ViewBag.Depts = depts;
        return View("~/Views/HrImport/Import.cshtml");
    }

    // ── 下载导入模板 ─────────────────────────────────────────
    [HttpGet("import/template")]
    [HasPermission("hr:employee:import")]
    public IActionResult DownloadTemplate()
    {
        var templateRows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["工号"]       = "",
                ["姓名*"]      = "张三",
                ["性别"]       = "男",
                ["手机"]       = "13800138000",
                ["邮箱"]       = "zhangsan@example.com",
                ["身份证号"]   = "",
                ["部门"]       = "第一事业部",
                ["入职日期"]   = "2024-03-01",
                ["试用期截止"] = "2024-09-01",
                ["备注"]       = "",
            },
            new()
            {
                ["工号"]       = "EMP20240002",
                ["姓名*"]      = "李四",
                ["性别"]       = "女",
                ["手机"]       = "13900139000",
                ["邮箱"]       = "",
                ["身份证号"]   = "",
                ["部门"]       = "第二事业部",
                ["入职日期"]   = "2024-01-15",
                ["试用期截止"] = "",
                ["备注"]       = "社招入职",
            },
        };

        var ms = new MemoryStream();
        ms.SaveAs(templateRows);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "员工批量导入模板.xlsx");
    }

    // ── 执行导入 ─────────────────────────────────────────────
    [HttpPost("import/execute"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:import")]
    public async Task<IActionResult> Execute(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(ApiResult<object>.Fail("请选择 Excel 文件"));

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx" && ext != ".xls")
            return Json(ApiResult<object>.Fail("仅支持 .xlsx / .xls 格式"));

        if (file.Length > 10 * 1024 * 1024)
            return Json(ApiResult<object>.Fail("文件不能超过 10MB"));

        // 读取部门列表（名称→ID 映射）
        var depts = await _uow.Depts.Query().Where(d => d.Status == 1).ToListAsync();
        var deptMap = depts.ToDictionary(
            d => d.DeptName.Trim(), d => d.Id, StringComparer.OrdinalIgnoreCase);

        // 现有工号集合（用于去重）
        var existingNos = await _db.Employees
            .Where(e => !e.IsDeleted)
            .Select(e => e.EmpNo)
            .ToHashSetAsync();

        var successList = new List<string>();
        var skipList    = new List<string>();
        var errorList   = new List<string>();

        using var stream = file.OpenReadStream();
        var rows = stream.Query(useHeaderRow: true).ToList();

        if (!rows.Any())
            return Json(ApiResult<object>.Fail("Excel 内容为空，请使用标准模板"));

        var toInsert = new List<Employee>();
        int rowIndex = 2;

        foreach (IDictionary<string, object> row in rows)
        {
            string rowTag = $"第{rowIndex}行";
            rowIndex++;

            try
            {
                // ── 必填字段校验 ────────────────────────────
                var realName = GetStr(row, "姓名*");
                if (string.IsNullOrWhiteSpace(realName))
                { errorList.Add($"{rowTag}：姓名不能为空"); continue; }

                // ── 工号处理 ────────────────────────────────
                var empNo = GetStr(row, "工号");
                if (string.IsNullOrWhiteSpace(empNo))
                    empNo = await GenerateEmpNoAsync();

                empNo = empNo.Trim();

                // 重复检查
                if (existingNos.Contains(empNo))
                { skipList.Add($"{empNo} {realName}（工号已存在，跳过）"); continue; }

                // ── 性别 ────────────────────────────────────
                var genderStr = GetStr(row, "性别");
                int gender = genderStr == "女" || genderStr == "2" ? 2 : 1;

                // ── 部门 ────────────────────────────────────
                long? deptId = null;
                var deptName = GetStr(row, "部门");
                if (!string.IsNullOrWhiteSpace(deptName))
                {
                    if (deptMap.TryGetValue(deptName.Trim(), out var did))
                        deptId = did;
                    else
                        errorList.Add($"{rowTag} [{empNo}]：部门 {deptName} 不存在，已忽略部门字段");
                }

                // ── 构造实体 ────────────────────────────────
                var emp = new Employee
                {
                    Id               = SnowflakeId.Next(),
                    EmpNo            = empNo,
                    RealName         = realName.Trim(),
                    Gender           = gender,
                    Phone            = GetStrOrNull(row, "手机"),
                    Email            = GetStrOrNull(row, "邮箱"),
                    IdCard           = GetStrOrNull(row, "身份证号"),
                    DeptId           = deptId,
                    EntryDate        = ParseDateNull(GetStr(row, "入职日期")),
                    ProbationEndDate = ParseDateNull(GetStr(row, "试用期截止")),
                    Remark           = GetStrOrNull(row, "备注"),
                    Status           = 0, // 试用期
                    CreatedBy        = User.GetRealName(),
                    CreatedAt        = DateTime.Now,
                };

                toInsert.Add(emp);
                existingNos.Add(empNo);
                successList.Add($"{empNo} {realName}");
            }
            catch (Exception ex)
            {
                errorList.Add($"{rowTag}：解析异常 - {ex.Message}");
            }
        }

        // ── 批量保存 ─────────────────────────────────────────
        if (toInsert.Any())
        {
            await _db.Employees.AddRangeAsync(toInsert);
            await _db.SaveChangesAsync();
            await _logSvc.LogAsync("批量导入员工",
                $"成功导入 {toInsert.Count} 条，跳过 {skipList.Count} 条，错误 {errorList.Count} 条",
                "INSERT", 0);
        }

        return Json(ApiResult<object>.Ok(new
        {
            SuccessCount = successList.Count,
            SkipCount    = skipList.Count,
            ErrorCount   = errorList.Count,
            SuccessList  = successList,
            SkipList     = skipList,
            ErrorList    = errorList,
        }, $"导入完成：成功 {successList.Count} 条，跳过 {skipList.Count} 条，错误 {errorList.Count} 条"));
    }

    // ── 生成工号 ─────────────────────────────────────────────
    private async Task<string> GenerateEmpNoAsync()
    {
        var year = DateTime.Now.Year.ToString();
        var count = await _db.Employees
            .Where(e => e.EmpNo.StartsWith($"EMP{year}"))
            .CountAsync();
        return $"EMP{year}{(count + 1):D4}";
    }

    // ── 工具方法 ─────────────────────────────────────────────
    private static string GetStr(IDictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var val) && val != null)
            return val.ToString()?.Trim() ?? "";
        return "";
    }

    private static string? GetStrOrNull(IDictionary<string, object> row, string key)
    {
        var s = GetStr(row, key);
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static DateTime? ParseDateNull(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (double.TryParse(s, out var oaDate) && oaDate > 1000)
            return DateTime.FromOADate(oaDate);
        return DateTime.TryParse(s, out var dt) ? dt : null;
    }
}
