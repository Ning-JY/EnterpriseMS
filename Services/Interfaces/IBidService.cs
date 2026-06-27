using EnterpriseMS.Common;
using EnterpriseMS.Services.AI.Models;
using EnterpriseMS.Services.DTOs.Bid;

namespace EnterpriseMS.Services.Interfaces;

public interface IBidService
{
    Task<PagedResult<BidProjectDto>> GetPagedAsync(BidListQuery query);
    Task<BidProjectDto?> GetDetailAsync(long id);
    Task<long> CreateAsync(BidProjectCreateDto dto, string operBy);
    Task UpdateAsync(long id, BidProjectUpdateDto dto, string operBy);
    Task DeleteAsync(long id, string operBy);

    Task<BidAnalysisResult> AnalyzeBidDocumentAsync(BidAnalyzeRequest request);
    Task SaveAnalysisResultAsync(long bidProjectId, BidAnalysisResult result);
    /// <summary>人工确认招标要素表，解析阶段卡点。仍存在 NeedsReview 条目时会拒绝确认。</summary>
    Task ConfirmElementsAsync(long bidProjectId, string operBy);
    /// <summary>人工核对单条"待确认"要求，补充出处或调整否决项标记后清除待确认状态。</summary>
    Task ResolveRequirementReviewAsync(long requirementId, bool isVeto, string? sourceRef, string operBy);

    Task<BidDocumentDto> GenerateChapterAsync(BidGenerateRequest request);
    IAsyncEnumerable<string> GenerateChapterStreamAsync(BidGenerateRequest request);
    Task<List<BidDocumentDto>> GenerateFullBidAsync(BidGenerateFullRequest request);

    Task<BidReviewResult> ReviewBidAsync(BidReviewRequest request);
    Task UpdateDocumentContentAsync(long documentId, string content);
    Task<BidDocumentDto?> GetDocumentAsync(long docId);
    Task<BidAssembleResult> AssembleBidDocumentAsync(long bidProjectId, string part);
    /// <summary>生成真正的.docx文件（封面、目录字段、按FormatRule套用字体、页眉页码），而不是纯文本拼接。</summary>
    Task<BidExportResult> ExportWordAsync(long bidProjectId, string part);
    /// <summary>导出PDF，依赖服务器LibreOffice环境；环境不可用时抛出包含明确原因的异常。</summary>
    Task<BidExportResult> ExportPdfAsync(long bidProjectId, string part);
    Task SaveStreamedDocumentAsync(long bidProjectId, string chapterName, int chapterType, string content);
    Task<PersonnelMatchResult> MatchPersonnelAsync(PersonnelMatchRequest request);
    Task<string> GeneratePersonnelSectionAsync(PersonnelMatchRequest request);
}
