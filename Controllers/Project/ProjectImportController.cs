using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Project;

[Authorize, Route("project")]
public class ProjectImportController : BaseAuthController
{
    private readonly IUnitOfWork    _uow;
    private readonly AppDbContext   _db;
    private readonly IDeptService   _deptSvc;
    private readonly IOperLogService _logSvc;

    public ProjectImportController(IUnitOfWork uow, AppDbContext db,
        IDeptService deptSvc, IOperLogService logSvc, IPermissionService permSvc)
        : base(permSvc)
    { _uow = uow; _db = db; _deptSvc = deptSvc; _logSvc = logSvc; }

    // ── 导入页面 ─────────────────────────────────────────────
    [HttpGet("import")]
    [HasPermission("proj:project:import")]
    public async Task<IActionResult> Import()
    {
        var depts = await _uow.Depts.Query()
            .Where(d => d.Status == 1).OrderBy(d => d.Sort).ToListAsync();
        ViewBag.Depts = depts;
        return View();
    }

    // ── 下载导入模板 ─────────────────────────────────────────
    [HttpGet("import/template")]
    [HasPermission("proj:project:import")]
    public IActionResult DownloadTemplate()
    {
        // 构造模板数据（表头说明行 + 示例数据行）
        var templateRows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["项目编号*"]     = "PROJ-2024-001",
                ["项目名称*"]     = "某某城市综合体可行性研究报告",
                ["业务类型*"]     = "可行性研究报告",
                ["承接部门"]      = "第一事业部",
                ["项目业主*"]     = "某某投资开发有限公司",
                ["业主联系人"]    = "张三",
                ["业主联系电话"]  = "13800138000",
                ["采购方式"]      = "竞争性磋商",
                ["限价金额(万元)"] = 50.00m,
                ["合同金额(万元)*"]= 45.00m,
                ["是否联合体"]    = "否",
                ["我方比例(%)"]   = "",
                ["签约日期"]      = "2024-03-01",
                ["计划完成日期"]  = "2024-09-30",
                ["进展状态"]      = "执行中",
                ["建设规模"]      = "总建筑面积约15万㎡",
                ["备注"]          = "",
            },
            new()
            {
                ["项目编号*"]     = "PROJ-2024-002",
                ["项目名称*"]     = "某某工程施工图预算编制",
                ["业务类型*"]     = "预算编制",
                ["承接部门"]      = "第二事业部",
                ["项目业主*"]     = "某某建设局",
                ["业主联系人"]    = "李四",
                ["业主联系电话"]  = "13900139000",
                ["采购方式"]      = "单一来源",
                ["限价金额(万元)"] = "",
                ["合同金额(万元)*"]= 8.50m,
                ["是否联合体"]    = "是",
                ["我方比例(%)"]   = 60,
                ["签约日期"]      = "2024-01-15",
                ["计划完成日期"]  = "2024-06-30",
                ["进展状态"]      = "成果提交",
                ["建设规模"]      = "",
                ["备注"]          = "联合体牵头单位",
            },
        };

        var ms = new MemoryStream();
        ms.SaveAs(templateRows);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "项目批量导入模板.xlsx");
    }

    // ── 执行导入 ─────────────────────────────────────────────
    [HttpPost("import/execute"), ValidateAntiForgeryToken]
    [HasPermission("proj:project:import")]
    public async Task<IActionResult> Execute(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(ApiResult<object>.Fail("请选择 Excel 文件"));

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx" && ext != ".xls")
            return Json(ApiResult<object>.Fail("仅支持 .xlsx / .xls 格式"));

        if (file.Length > 10 * 1024 * 1024)
            return Json(ApiResult<object>.Fail("文件不能超过 10MB"));

        // 读取部门列表（用于名称→ID 映射）
        var depts = await _uow.Depts.Query().Where(d => d.Status == 1).ToListAsync();
        var deptMap = depts.ToDictionary(
            d => d.DeptName.Trim(), d => d.Id, StringComparer.OrdinalIgnoreCase);

        // 现有项目编号集合（用于去重）
        var existingNos = await _db.Projects
            .Where(p => !p.IsDeleted)
            .Select(p => p.ProjNo)
            .ToHashSetAsync();

        var successList = new List<string>();
        var skipList    = new List<string>();
        var errorList   = new List<string>();

        using var stream = file.OpenReadStream();
        var rows = stream.Query(useHeaderRow: true).ToList();

        if (!rows.Any())
            return Json(ApiResult<object>.Fail("Excel 内容为空，请使用标准模板"));

        var toInsert = new List<EnterpriseMS.Domain.Entities.Project.Project>();
        int rowIndex = 2; // 从第2行（数据行）开始计数（第1行是表头）

        foreach (IDictionary<string, object> row in rows)
        {
            string rowTag = $"第{rowIndex}行";
            rowIndex++;

            try
            {
                // ── 必填字段校验 ────────────────────────────
                var projNo   = GetStr(row, "项目编号*");
                var projName = GetStr(row, "项目名称*");
                var bizType  = GetStr(row, "业务类型*");
                var ownerName= GetStr(row, "项目业主*");
                var amtStr   = GetStr(row, "合同金额(万元)*");

                if (string.IsNullOrWhiteSpace(projNo))
                { errorList.Add($"{rowTag}：项目编号不能为空"); continue; }
                if (string.IsNullOrWhiteSpace(projName))
                { errorList.Add($"{rowTag} [{projNo}]：项目名称不能为空"); continue; }
                if (string.IsNullOrWhiteSpace(bizType))
                { errorList.Add($"{rowTag} [{projNo}]：业务类型不能为空"); continue; }
                if (string.IsNullOrWhiteSpace(ownerName))
                { errorList.Add($"{rowTag} [{projNo}]：项目业主不能为空"); continue; }
                if (!decimal.TryParse(amtStr, out var contractAmount) || contractAmount < 0)
                { errorList.Add($"{rowTag} [{projNo}]：合同金额格式不正确，请填写数字"); continue; }

                // ── 重复检查 ─────────────────────────────────
                if (existingNos.Contains(projNo.Trim()))
                { skipList.Add($"{projNo}（已存在，跳过）"); continue; }

                // ── 可选字段解析 ─────────────────────────────
                // 部门
                long? deptId = null;
                var deptName = GetStr(row, "承接部门");
                if (!string.IsNullOrWhiteSpace(deptName))
                {
                    if (deptMap.TryGetValue(deptName.Trim(), out var did))
                        deptId = did;
                    else
                        errorList.Add($"{rowTag} [{projNo}]：部门 {deptName} 不存在，已忽略部门字段");
                }

                // 联合体
                var jointStr = GetStr(row, "是否联合体").Trim();
                var isJoint  = jointStr == "是" || jointStr == "1" || jointStr.ToLower() == "yes";
                decimal? ourRatio = null;
                if (isJoint)
                {
                    var ratioStr = GetStr(row, "我方比例(%)");
                    if (decimal.TryParse(ratioStr, out var r) && r > 0 && r <= 100)
                        ourRatio = r;
                }

                // 进展状态
                var statusStr = GetStr(row, "进展状态");
                var progress  = ParseProgressStatus(statusStr);

                // 构造实体
                var proj = new EnterpriseMS.Domain.Entities.Project.Project
                {
                    ProjNo          = projNo.Trim(),
                    ProjName        = projName.Trim(),
                    BizType         = bizType.Trim(),
                    DeptId          = deptId,
                    OwnerName       = ownerName.Trim(),
                    OwnerContact    = GetStrOrNull(row, "业主联系人"),
                    OwnerPhone      = GetStrOrNull(row, "业主联系电话"),
                    ProcurementType = GetStrOrNull(row, "采购方式"),
                    LimitPrice      = ParseDecimalNull(GetStr(row, "限价金额(万元)")),
                    ContractAmount  = contractAmount,
                    IsJointVenture  = isJoint,
                    OurRatio        = ourRatio,
                    SignDate        = ParseDateNull(GetStr(row, "签约日期")),
                    PlanEndDate     = ParseDateNull(GetStr(row, "计划完成日期")),
                    BuildingScale   = GetStrOrNull(row, "建设规模"),
                    Remark          = GetStrOrNull(row, "备注"),
                    ProgressStatus  = progress,
                    StatusUpdatedAt = DateTime.Now,
                    CreatedBy       = User.GetRealName(),
                };

                toInsert.Add(proj);
                existingNos.Add(projNo.Trim()); // 防止同一文件内重复
                successList.Add(projNo.Trim());
            }
            catch (Exception ex)
            {
                errorList.Add($"{rowTag}：解析异常 - {ex.Message}");
            }
        }

        // ── 批量保存 ─────────────────────────────────────────
        if (toInsert.Any())
        {
            // 使用雪花ID
            foreach (var p in toInsert)
                if (p.Id == 0) p.Id = SnowflakeId.Next();

            await _db.Projects.AddRangeAsync(toInsert);
            await _db.SaveChangesAsync();
            await _logSvc.LogAsync("批量导入项目",
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

    private static decimal? ParseDecimalNull(string s)
        => decimal.TryParse(s, out var v) ? v : null;

    private static DateTime? ParseDateNull(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        // 兼容 Excel 的 OA 日期数字和字符串日期
        if (double.TryParse(s, out var oaDate) && oaDate > 1000)
            return DateTime.FromOADate(oaDate);
        return DateTime.TryParse(s, out var dt) ? dt : null;
    }

    private static int ParseProgressStatus(string s) => s.Trim() switch
    {
        "前期商务"         => 0,
        "预计启动"         => 1,
        "标书制作中"       => 2,
        "投标/磋商中"      => 3,
        "已中标·签合同中" or "已中标签合同中" => 4,
        "已签回合同"       => 5,
        "执行中"           => 6,
        "成果提交"         => 7,
        "已完成"           => 8,
        "已终止"           => 9,
        _                  => 0, // 默认前期商务
    };
}
