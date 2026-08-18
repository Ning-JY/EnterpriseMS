using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sys = System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Impl;
using EnterpriseMS.Services.Interfaces;
using MiniSoftware;

namespace EnterpriseMS.Controllers.Tool;

[AllowAnonymous]
[Route("templatereport")]
public class TemplateReportController : BaseAuthController
{
    private readonly IReportGeneratorService _reportService;
    private readonly IConfigService _configService;
    private readonly IEnumerable<ITemplateDataSource> _dataSources;

    public TemplateReportController(IPermissionService permSvc, IReportGeneratorService reportService, IConfigService configService, IEnumerable<ITemplateDataSource> dataSources)
        : base(permSvc)
    {
        _reportService = reportService;
        _configService = configService;
        _dataSources = dataSources;
    }

    [HttpGet("index")]
    public IActionResult Index() => View();

    [HttpGet("manage")]
    public IActionResult Manage() => View();

    [HttpGet("download/{templateId}")]
    public IActionResult DownloadTemplate(string templateId)
    {
        try
        {
            var (bytes, fileName) = _reportService.GetTemplateFile(templateId);
            if (bytes == null)
                return NotFound("模板文件不存在");
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpPost("delete")]
    public IActionResult DeleteTemplate([FromBody] DeleteTemplateRequest request)
    {
        try
        {
            var ok = _reportService.DeleteTemplate(request.TemplateId);
            return Json(ok
                ? ApiResult<object>.Ok(null, "模板已删除")
                : ApiResult<object>.Fail("模板不存在或已删除"));
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpGet("templateconfig")]
    public IActionResult TemplateConfig() => View();

    /// <summary>
    /// 模板列表（layui table 数据源）：返回 PagedResult&lt;object&gt;，
    /// 投影为扁平匿名对象，避免把 Fields 集合整包吐给前端。
    /// </summary>
    [HttpGet("list")]
    public IActionResult List(string? keyword, string? category, int page = 1, int size = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var all = _reportService.GetTemplates();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                all = all.Where(t =>
                        (t.Name ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                        (t.Description ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                all = all.Where(t => (t.Category ?? "") == category).ToList();
            }

            var items = all.Skip((page - 1) * size).Take(size)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Description,
                    t.FileName,
                    t.CreatedAt,
                    FieldCount = t.Fields?.Count ?? 0
                })
                .Cast<object>().ToList();

            return ApiOk(new PagedResult<object>
            {
                Items = items,
                Total = all.Count,
                Page = page,
                PageSize = size
            });
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpGet("templates")]
    public IActionResult GetTemplates()
    {
        try
        {
            var templates = _reportService.GetTemplates();
            return ApiOk(templates);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>模板分类下拉数据源（去重非空分类）。</summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        try
        {
            var cats = _reportService.GetTemplates()
                .Select(t => t.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            return ApiOk(cats);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>
    /// 字段来源可选项：供配置向导「绑定项目字段 / 系统配置」下拉使用。
    /// projectFields 来自 ReportFieldSources（与后端解析共用），configKeys 来自系统参数表。
    /// </summary>
    [HttpGet("sources")]
    public async Task<IActionResult> GetSources()
    {
        try
        {
            var projectFields = ReportFieldSources.ProjectFields
                .Select(f => new { key = f.Key, label = f.Label })
                .ToList();

            var configs = await _configService.GetAllAsync();
            var configKeys = configs
                .Select(c => new
                {
                    key = c.ConfigKey,
                    label = string.IsNullOrEmpty(c.ConfigValue)
                        ? c.ConfigKey
                        : $"{c.ConfigKey}（{c.ConfigValue}）"
                })
                .ToList();

            return ApiOk(new { projectFields, configKeys });
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpGet("template/{templateId}")]
    public IActionResult GetTemplate(string templateId)
    {
        try
        {
            var template = _reportService.GetTemplate(templateId);
            if (template == null)
                return ApiFail("模板不存在");
            return ApiOk(template);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>
    /// 通用数据源清单（替代旧 /sources）：返回所有已注册数据源及其字段白名单，供配置向导动态生成「字段来源」选项。
    /// </summary>
    [HttpGet("data-sources")]
    public IActionResult GetDataSources()
    {
        try
        {
            var sources = new List<object>();
            sources.Add(new { sourceId = "manual", displayName = "手动填写", fields = new object[0] });
            foreach (var p in _dataSources)
            {
                if (p.SourceId == "manual") continue;
                var schema = p.GetFieldSchema();
                sources.Add(new
                {
                    sourceId = p.SourceId,
                    displayName = p.DisplayName,
                    fields = schema.Select(kv => new { key = kv.Key, label = kv.Value }).ToList()
                });
            }
            return ApiOk(sources);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>
    /// 某数据源下的实例列表（如项目/员工/合同列表），供填充向导选择「数据上下文」实例。
    /// </summary>
    [HttpGet("data-contexts/{sourceId}")]
    public async Task<IActionResult> GetDataContexts(string sourceId)
    {
        try
        {
            var provider = _dataSources.FirstOrDefault(p => p.SourceId == sourceId);
            if (provider == null)
                return ApiFail("未知的数据源");
            var instances = await provider.ListInstancesAsync();
            return ApiOk(instances);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpPost("scan-placeholders")]
    public IActionResult ScanPlaceholders([FromBody] TemplateIdRequest request)
    {
        try
        {
            var placeholders = _reportService.ScanPlaceholders(request.TemplateId);
            return ApiOk(placeholders);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpPost("configure-template")]
    public IActionResult ConfigureTemplate([FromForm] ConfigureTemplateRequest request, IFormFile templateFile)
    {
        try
        {
            var templateId = _reportService.ConfigureTemplate(request, templateFile);
            return ApiOk(new { templateId }, "模板配置成功");
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            await MergeAutoFields(request, fieldValues);
            var base64 = _reportService.FillTemplate(request.TemplateId, fieldValues);
            return ApiOk(new { base64 });
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            await MergeAutoFields(request, fieldValues);
            var bytes = _reportService.GenerateDocument(request.TemplateId, fieldValues);
            var template = _reportService.GetTemplate(request.TemplateId);
            var fileName = $"{template?.Name ?? "报告"}.docx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>
    /// ad-hoc 渲染端点：造价小工具等客户端解析 Excel 后，把标量(SupplementaryFields)与明细行(ExcelRows→明细)提交，
    /// 由服务端用 OpenXML 展开表格行并替换标量占位符，返回生成的 docx。支持表格行循环。
    /// </summary>
    [HttpPost("generate-adhoc")]
    public IActionResult GenerateAdhoc([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            var bytes = _reportService.GenerateAdhocReport(request.TemplateId, fieldValues);
            var template = _reportService.GetTemplate(request.TemplateId);
            var fileName = $"{template?.Name ?? "报告"}.docx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>
    /// 实时字段预览：根据所选数据上下文实例，返回模板各字段的解析值——
    /// 自动带出字段显示真实值，手动字段返回默认值/提示，供填充向导实时预览（Phase 3）。
    /// </summary>
    [HttpGet("preview-fields")]
    public async Task<IActionResult> PreviewFields(string templateId, string? sourceId, string? instanceId)
    {
        try
        {
            var tpl = _reportService.GetTemplate(templateId);
            if (tpl == null) return ApiFail("模板不存在");

            var autoFields = new List<object>();
            var manualFields = new List<object>();

            Dictionary<string, string> resolved = new();
            if (!string.IsNullOrWhiteSpace(sourceId) && !string.IsNullOrWhiteSpace(instanceId))
                resolved = await _reportService.BuildReportFieldValuesAsync(sourceId, instanceId, tpl, null);

            foreach (var f in tpl.Fields)
            {
                if (f.Source == "manual")
                {
                    manualFields.Add(new
                    {
                        name = f.Name,
                        label = f.Label,
                        type = f.Type,
                        defaultValue = f.DefaultValue,
                        helpText = f.HelpText,
                        options = f.Options
                    });
                }
                else
                {
                    resolved.TryGetValue(f.Name, out var val);
                    autoFields.Add(new
                    {
                        name = f.Name,
                        label = f.Label,
                        source = f.Source,
                        binding = f.Binding,
                        configKey = f.ConfigKey,
                        value = val ?? ""
                    });
                }
            }

            return ApiOk(new { autoFields, manualFields, hasInstance = resolved.Count > 0 });
        }
        catch (Exception ex)
        {
            return ApiFail(FullErr(ex));
        }
    }

    /// <summary>把通用数据源解析出的绑定字段合并进填充字典（与 manual/excel/list/image 共存）。</summary>
    private async Task MergeAutoFields(ReportFillRequest request, Dictionary<string, object> fieldValues)
    {
        if (string.IsNullOrWhiteSpace(request.ContextSource) || string.IsNullOrWhiteSpace(request.InstanceId))
            return;
        var tpl = _reportService.GetTemplate(request.TemplateId);
        if (tpl == null) return;
        var auto = await _reportService.BuildReportFieldValuesAsync(request.ContextSource, request.InstanceId, tpl, request.SupplementaryFields);
        foreach (var kv in auto)
            fieldValues[kv.Key] = kv.Value;
    }

    private Dictionary<string, object> BuildFieldValues(ReportFillRequest request)
    {
        var fieldValues = new Dictionary<string, object>();

        if (request.SupplementaryFields != null)
        {
            foreach (var kv in request.SupplementaryFields)
                fieldValues[kv.Key] = kv.Value;
        }

        // Excel 行：聚合为列表字段"明细"，模板里用 {{明细.字段}} 在表格行循环
        if (request.ExcelRows != null && request.ExcelRows.Count > 0 && request.ExcelColumns != null && request.ExcelColumns.Count > 0)
        {
            var listRows = new List<Dictionary<string, object>>();
            foreach (var row in request.ExcelRows)
            {
                var dict = new Dictionary<string, object>();
                foreach (var col in request.ExcelColumns)
                {
                    if (row.TryGetValue(col.ColumnName, out var val))
                        dict[col.FieldName] = val;
                }
                listRows.Add(dict);
            }
            fieldValues["明细"] = listRows;
        }

        // 显式声明的列表字段
        if (request.ListFields != null)
        {
            foreach (var kv in request.ListFields)
                fieldValues[kv.Key] = kv.Value;
        }

        // 图片字段：MiniWordPicture 接收 Path，故 Base64 先落临时文件
        if (request.ImageFields != null)
        {
            foreach (var kv in request.ImageFields)
            {
                var img = kv.Value;
                string? imgPath = img.Path;
                if (string.IsNullOrEmpty(imgPath) && !string.IsNullOrEmpty(img.Base64))
                {
                    imgPath = Path.Combine(Path.GetTempPath(), $"ems_img_{Sys.Guid.NewGuid():N}.png");
                    Sys.IO.File.WriteAllBytes(imgPath, Convert.FromBase64String(img.Base64));
                }
                if (!string.IsNullOrEmpty(imgPath))
                {
                    fieldValues[kv.Key] = new MiniWordPicture { Path = imgPath, Width = img.Width, Height = img.Height };
                }
            }
        }

        // 业务计算：送审金额 + 审定金额 → 自动算 审减金额 / 审减率。
        // 手动填写或带上下文的模板通用；仅当目标字段为空时填充，避免覆盖用户显式填写的值。
        if (fieldValues.TryGetValue("送审金额", out var sObj) && fieldValues.TryGetValue("审定金额", out var dObj)
            && decimal.TryParse(sObj?.ToString(), out var sd) && decimal.TryParse(dObj?.ToString(), out var dd))
        {
            if (!fieldValues.ContainsKey("审减金额") || string.IsNullOrWhiteSpace(fieldValues["审减金额"]?.ToString()))
                fieldValues["审减金额"] = (sd - dd).ToString("F2");
            if (!fieldValues.ContainsKey("审减率") || string.IsNullOrWhiteSpace(fieldValues["审减率"]?.ToString()))
                fieldValues["审减率"] = (sd != 0 ? (sd - dd) / sd * 100 : 0).ToString("F2") + "%";
        }

        return fieldValues;
    }

    /// <summary>展开异常链（含 InnerException），便于前端/排查看到真实根因（如 MySQL 具体报错）。</summary>
    private static string FullErr(Exception ex)
    {
        if (ex == null) return "";
        var msg = ex.Message;
        var ie = ex.InnerException;
        while (ie != null)
        {
            msg += " | Inner: " + ie.Message;
            ie = ie.InnerException;
        }
        return msg;
    }
}

public class TemplateIdRequest
{
    public string TemplateId { get; set; } = "";
}
