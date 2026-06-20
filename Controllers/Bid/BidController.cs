using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
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

    public async Task<IActionResult> Index(BidListQuery query)
    {
        var result = await _bidService.GetPagedAsync(query);
        ViewBag.Query = query;
        return View(result);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(BidProjectCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var id = await _bidService.CreateAsync(dto, User.GetUsername());
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Detail(long id)
    {
        var bid = await _bidService.GetDetailAsync(id);
        if (bid == null) return NotFound();
        return View(bid);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var bid = await _bidService.GetDetailAsync(id);
        if (bid == null) return NotFound();
        return View(bid);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, BidProjectUpdateDto dto)
    {
        await _bidService.UpdateAsync(id, dto, User.GetUsername());
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        await _bidService.DeleteAsync(id, User.GetUsername());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(BidAnalyzeRequest request)
    {
        try
        {
            var result = await _bidService.AnalyzeBidDocumentAsync(request);
            await _bidService.SaveAnalysisResultAsync(request.BidProjectId, result);
            return Json(ApiResult<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing bid document");
            return Json(ApiResult<object>.Fail(ex.Message));
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
            return Json(ApiResult<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chapter");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateFull([FromBody] BidGenerateFullRequest request)
    {
        try
        {
            var results = await _bidService.GenerateFullBidAsync(request);
            return Json(ApiResult<object>.Ok(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating full bid");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Assemble([FromBody] BidAssembleRequest request)
    {
        try
        {
            var result = await _bidService.AssembleBidDocumentAsync(request.BidProjectId, request.Part);
            return Json(ApiResult<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling bid document");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Review([FromBody] BidReviewRequest request)
    {
        try
        {
            var result = await _bidService.ReviewBidAsync(request);
            return Json(ApiResult<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing bid");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateContent(long documentId, [FromBody] UpdateContentRequest request)
    {
        try
        {
            await _bidService.UpdateDocumentContentAsync(documentId, request.Content);
            return Json(ApiResult.Ok());
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
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
            if (doc == null) return Json(ApiResult<object>.Fail("文档不存在"));
            return Json(ApiResult<object>.Ok(doc));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> MatchPersonnel([FromBody] PersonnelMatchRequest request)
    {
        try
        {
            var result = await _bidService.MatchPersonnelAsync(request);
            return Json(ApiResult<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error matching personnel");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> GeneratePersonnelSection([FromBody] PersonnelMatchRequest request)
    {
        try
        {
            var content = await _bidService.GeneratePersonnelSectionAsync(request);

            // 保存到文档
            await _bidService.SaveStreamedDocumentAsync(request.BidProjectId, "人员配置", 3, content);

            return Json(ApiResult<object>.Ok(new { content, saved = true }, "人员配置文档已生成并保存"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personnel section");
            return Json(ApiResult<object>.Fail(ex.Message));
        }
    }
}

public class UpdateContentRequest
{
    public string Content { get; set; } = "";
}
