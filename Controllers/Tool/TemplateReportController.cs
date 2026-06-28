using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Impl;
using EnterpriseMS.Services.Interfaces;

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

    [HttpGet("templateconfig")]
    public IActionResult TemplateConfig() => View();

    [HttpGet("templates")]
    public IActionResult GetTemplates()
    {
        try
        {
            var templates = _reportService.GetTemplates();
            return Json(ApiResult<List<TemplateInfoDto>>.Ok(templates));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpGet("template/{templateId}")]
    public IActionResult GetTemplate(string templateId)
    {
        try
        {
            var template = _reportService.GetTemplate(templateId);
            if (template == null)
                return Json(ApiResult<object>.Fail("模板不存在"));
            return Json(ApiResult<TemplateInfoDto>.Ok(template));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost("scan-placeholders")]
    public IActionResult ScanPlaceholders([FromBody] TemplateIdRequest request)
    {
        try
        {
            var placeholders = _reportService.ScanPlaceholders(request.TemplateId);
            return Json(ApiResult<List<TemplatePlaceholderDto>>.Ok(placeholders));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost("configure-template")]
    public IActionResult ConfigureTemplate([FromForm] ConfigureTemplateRequest request, IFormFile templateFile)
    {
        try
        {
            var templateId = _reportService.ConfigureTemplate(request, templateFile);
            return Json(ApiResult<object>.Ok(new { templateId }, "模板配置成功"));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost("preview")]
    public IActionResult Preview([FromBody] ReportFillRequest request)
    {
        try
        {
            var fieldValues = BuildFieldValues(request);
            var base64 = _reportService.FillTemplate(request.TemplateId, fieldValues);
            return Json(ApiResult<object>.Ok(new { base64 }));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
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
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    private Dictionary<string, string> BuildFieldValues(ReportFillRequest request)
    {
        var fieldValues = new Dictionary<string, string>();

        if (request.SupplementaryFields != null)
        {
            foreach (var kv in request.SupplementaryFields)
                fieldValues[kv.Key] = kv.Value;
        }

        if (request.ExcelColumns != null && request.ExcelRows != null)
        {
            foreach (var row in request.ExcelRows)
            {
                foreach (var col in request.ExcelColumns)
                {
                    if (row.TryGetValue(col.ColumnName, out var val))
                        fieldValues[col.FieldName] = val;
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
