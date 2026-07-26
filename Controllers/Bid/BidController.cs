using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.AI.Models;
using EnterpriseMS.Services.DTOs.Bid;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Bid;

[Authorize]
public class BidController : BaseAuthController
{
    private readonly IBidService _bidService;
    private readonly ILogger<BidController> _logger;

    public BidController(IBidService bidService, IPermissionService permSvc, ILogger<BidController> logger)
        : base(permSvc)
    {
        _bidService = bidService;
        _logger = logger;
    }

    [HasPermission("bid:project:list")]
    public async Task<IActionResult> Index(BidListQuery query)
    {
        var result = await _bidService.GetPagedAsync(query);
        ViewBag.Query = query;
        return View(result);
    }

    [HasPermission("bid:project:list")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [HasPermission("bid:project:add")]
    public async Task<IActionResult> Create(BidProjectCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var id = await _bidService.CreateAsync(dto, User.GetUsername());
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HasPermission("bid:project:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var bid = await _bidService.GetDetailAsync(id);
        if (bid == null) return NotFound();
        return View(bid);
    }

    [HasPermission("bid:project:list")]
    public async Task<IActionResult> Edit(long id)
    {
        var bid = await _bidService.GetDetailAsync(id);
        if (bid == null) return NotFound();
        return View(bid);
    }

    [HttpPost]
    [HasPermission("bid:project:edit")]
    public async Task<IActionResult> Edit(long id, BidProjectUpdateDto dto)
    {
        await _bidService.UpdateAsync(id, dto, User.GetUsername());
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [HasPermission("bid:project:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        await _bidService.DeleteAsync(id, User.GetUsername());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [HasPermission("bid:project:analyze")]
    public async Task<IActionResult> Analyze(BidAnalyzeRequest request)
    {
        try
        {
            var result = await _bidService.AnalyzeBidDocumentAsync(request);
            await _bidService.SaveAnalysisResultAsync(request.BidProjectId, result);
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing bid document");
            return ApiFail(ex.Message);
        }
    }

    /// <summary>"人工确认招标要素表"卡点。仍有待确认条目时会被拒绝，前端按 message 提示用户先处理。</summary>
    [HttpPost]
    [HasPermission("bid:project:confirm")]
    public async Task<IActionResult> ConfirmElements([FromBody] ConfirmElementsRequest request)
    {
        try
        {
            await _bidService.ConfirmElementsAsync(request.BidProjectId, User.GetUsername());
            return ApiOk("招标要素表已确认");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming bid elements");
            return ApiFail(ex.Message);
        }
    }

    /// <summary>人工核对单条"待确认"要求：补充出处定位、修正是否为否决项，提交后清除待确认标记。</summary>
    [HttpPost]
    public async Task<IActionResult> ResolveRequirement([FromBody] ResolveRequirementRequest request)
    {
        try
        {
            await _bidService.ResolveRequirementReviewAsync(
                request.RequirementId, request.IsVeto, request.SourceRef, User.GetUsername());
            return ApiOk("操作成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving requirement review");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateChapter([FromBody] BidGenerateRequest request)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            var fullContent = "";
            await foreach (var chunk in _bidService.GenerateChapterStreamAsync(request))
            {
                fullContent += chunk;
                var data = JsonSerializer.Serialize(new { content = chunk });
                await Response.WriteAsync($"data: {data}\n\n");
                await Response.Body.FlushAsync();
            }

            // 流式生成完成后自动保存文档
            if (!string.IsNullOrEmpty(fullContent))
            {
                await _bidService.SaveStreamedDocumentAsync(request.BidProjectId, request.ChapterName,
                    request.ChapterType, fullContent);
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chapter");
            var error = JsonSerializer.Serialize(new { error = ex.Message });
            await Response.WriteAsync($"data: {error}\n\n");
            await Response.Body.FlushAsync();
        }

        return new EmptyResult();
    }

    [HttpPost]
    public async Task<IActionResult> GenerateChapterSync([FromBody] BidGenerateRequest request)
    {
        try
        {
            var result = await _bidService.GenerateChapterAsync(request);
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chapter");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateFull([FromBody] BidGenerateFullRequest request)
    {
        try
        {
            var results = await _bidService.GenerateFullBidAsync(request);
            return ApiOk(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating full bid");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Assemble([FromBody] BidAssembleRequest request)
    {
        try
        {
            var result = await _bidService.AssembleBidDocumentAsync(request.BidProjectId, request.Part);
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling bid document");
            return ApiFail(ex.Message);
        }
    }

    /// <summary>导出真正的.docx文件。用GET+querystring而不是走JSON接口，方便前端直接用fetch+blob下载。</summary>
    [HttpGet]
    [HasPermission("bid:project:export")]
    public async Task<IActionResult> ExportWord(long bidProjectId, string part = "all")
    {
        try
        {
            var result = await _bidService.ExportWordAsync(bidProjectId, part);
            if (result.Warnings.Any())
                Response.Headers.Append("X-Export-Warnings", Uri.EscapeDataString(string.Join(" | ", result.Warnings)));
            return File(result.FileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.FileName);
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting bid document to Word");
            return ApiFail(ex.Message);
        }
    }

    [HttpGet]
    [HasPermission("bid:project:export")]
    public async Task<IActionResult> ExportPdf(long bidProjectId, string part = "all")
    {
        try
        {
            var result = await _bidService.ExportPdfAsync(bidProjectId, part);
            if (result.Warnings.Any())
                Response.Headers.Append("X-Export-Warnings", Uri.EscapeDataString(string.Join(" | ", result.Warnings)));
            return File(result.FileBytes, "application/pdf", result.FileName);
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting bid document to PDF");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Review([FromBody] BidReviewRequest request)
    {
        try
        {
            var result = await _bidService.ReviewBidAsync(request);
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing bid");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateContent(long documentId, [FromBody] UpdateContentRequest request)
    {
        try
        {
            await _bidService.UpdateDocumentContentAsync(documentId, request.Content);
            return ApiOk("更新成功");
        }
        catch (Exception ex)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalysisData(long bidProjectId)
    {
        var bid = await _bidService.GetDetailAsync(bidProjectId);
        if (bid == null) return NotFound();
        return Json(bid.Requirements);
    }

    [HttpGet]
    public async Task<IActionResult> GetDocument(long docId)
    {
        try
        {
            var doc = await _bidService.GetDocumentAsync(docId);
            if (doc == null) return ApiFail("文档不存在");
            return ApiOk(doc);
        }
        catch (Exception ex)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    [HasPermission("bid:project:match")]
    public async Task<IActionResult> MatchPersonnel([FromBody] PersonnelMatchRequest request)
    {
        try
        {
            var result = await _bidService.MatchPersonnelAsync(request);
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error matching personnel");
            return ApiFail(ex.Message);
        }
    }

    [HttpPost]
    [HasPermission("bid:project:match")]
    public async Task<IActionResult> GeneratePersonnelSection([FromBody] PersonnelMatchRequest request)
    {
        try
        {
            var content = await _bidService.GeneratePersonnelSectionAsync(request);

            // 保存到文档
            await _bidService.SaveStreamedDocumentAsync(request.BidProjectId, "人员配置", 3, content);

            return ApiOk(new { content, saved = true }, "人员配置文档已生成并保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personnel section");
            return ApiFail(ex.Message);
        }
    }
}

public class UpdateContentRequest
{
    public string Content { get; set; } = "";
}

public class ConfirmElementsRequest
{
    public long BidProjectId { get; set; }
}

public class ResolveRequirementRequest
{
    public long RequirementId { get; set; }
    public bool IsVeto { get; set; }
    public string? SourceRef { get; set; }
}
