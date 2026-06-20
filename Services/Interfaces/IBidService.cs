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

    Task<BidDocumentDto> GenerateChapterAsync(BidGenerateRequest request);
    IAsyncEnumerable<string> GenerateChapterStreamAsync(BidGenerateRequest request);
    Task<List<BidDocumentDto>> GenerateFullBidAsync(BidGenerateFullRequest request);

    Task<BidReviewResult> ReviewBidAsync(BidReviewRequest request);
    Task UpdateDocumentContentAsync(long documentId, string content);
    Task<BidDocumentDto?> GetDocumentAsync(long docId);
    Task<BidAssembleResult> AssembleBidDocumentAsync(long bidProjectId, string part);
    Task SaveStreamedDocumentAsync(long bidProjectId, string chapterName, int chapterType, string content);
    Task<PersonnelMatchResult> MatchPersonnelAsync(PersonnelMatchRequest request);
    Task<string> GeneratePersonnelSectionAsync(PersonnelMatchRequest request);
}
