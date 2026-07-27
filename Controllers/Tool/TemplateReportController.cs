using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Impl;
using EnterpriseMS.Services.Interfaces;
using MiniSoftware;
using System.IO;

namespace EnterpriseMS.Controllers.Tool;

[AllowAnonymous]
[Route("templatereport")]
public class TemplateReportController : BaseAuthController
{
    private readonly IReportGeneratorService _reportService;

    public TemplateReportController(IPermissionService permSvc, IReportGeneratorService reportService)
        : base(permSvc)
    {
        _reportService = reportService;
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
            return ApiFail(ex.Message);
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
            return ApiFail(ex.Message);
        }
    }

    [HttpGet("templateconfig")]
    public IActionResult TemplateConfig() => View();

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
            return ApiFail(ex.Message);
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
            return ApiFail(ex.Message);
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
            return ApiFail(ex.Message);
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
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("preview")]
    public IActionResult Preview([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            var base64 = _reportService.FillTemplate(request.TemplateId, fieldValues);
            return ApiOk(new { base64 });
        }
        catch (Exception ex)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("generate")]
    public IActionResult Generate([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            var bytes = _reportService.GenerateDocument(request.TemplateId, fieldValues);
            var template = _reportService.GetTemplate(request.TemplateId);
            var fileName = $"{template?.Name ?? "报告"}.docx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
        catch (Exception ex)
        {
            return ApiFail(ex.Message);
        }
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
                    imgPath = Path.Combine(Path.GetTempPath(), $"ems_img_{System.Guid.NewGuid():N}.png");
                    File.WriteAllBytes(imgPath, Convert.FromBase64String(img.Base64));
                }
                if (!string.IsNullOrEmpty(imgPath))
                {
                    fieldValues[kv.Key] = new MiniWordPicture { Path = imgPath, Width = img.Width, Height = img.Height };
                }
            }
        }

        return fieldValues;
    }
}

public class TemplateIdRequest
{
    public string TemplateId { get; set; } = "";
}
