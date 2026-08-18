using EnterpriseMS.Services.AI.Models;

namespace EnterpriseMS.Services.AI;

public interface IAIService
{
    Task<BidAnalysisResult> AnalyzeBidDocumentAsync(string documentContent);
    Task<string> GenerateChapterAsync(BidChapterRequest request);
    IAsyncEnumerable<string> GenerateChapterStreamAsync(BidChapterRequest request);
    Task<BidReviewResult> ReviewBidDocumentAsync(string bidContent, string requirements);

    // ── 运行时 AI 配置（Debug 页在线配置 / 测试）──
    Task<AiConfigDto> GetConfigAsync();
    Task SaveConfigAsync(AiConfigDto cfg);
    Task<string> TestConnectionAsync(string apiKey, string baseUrl, string model);
}
