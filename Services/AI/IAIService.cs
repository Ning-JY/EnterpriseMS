using EnterpriseMS.Services.AI.Models;

namespace EnterpriseMS.Services.AI;

public interface IAIService
{
    Task<BidAnalysisResult> AnalyzeBidDocumentAsync(string documentContent);
    Task<string> GenerateChapterAsync(BidChapterRequest request);
    IAsyncEnumerable<string> GenerateChapterStreamAsync(BidChapterRequest request);
    Task<BidReviewResult> ReviewBidDocumentAsync(string bidContent, string requirements);
}
