using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using EnterpriseMS.Services.AI.Models;
using EnterpriseMS.Services.AI.Prompts;

#pragma warning disable SKEXP0010

namespace EnterpriseMS.Services.AI;

public class OpenAIService : IAIService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OpenAIService> _logger;
    private static readonly string AiConfigPath = Path.Combine("App_Data", "ai-config.json");

    public OpenAIService(IConfiguration config, ILogger<OpenAIService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private (string apiKey, string baseUrl, string model) GetAiSettings()
    {
        try
        {
            if (File.Exists(AiConfigPath))
            {
                var json = File.ReadAllText(AiConfigPath);
                var dto = JsonSerializer.Deserialize<AiConfigDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto != null && !string.IsNullOrWhiteSpace(dto.ApiKey))
                    return (dto.ApiKey, dto.BaseUrl, dto.Model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read AI config file, falling back to appsettings");
        }

        return (
            _config["AI:ApiKey"] ?? "",
            _config["AI:BaseUrl"] ?? "https://api.openai.com/v1",
            _config["AI:Model"] ?? "gpt-4o"
        );
    }

    private Kernel? BuildKernel(string apiKey, string baseUrl, string model)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, endpoint: new Uri(baseUrl));
        return builder.Build();
    }

    public async Task<BidAnalysisResult> AnalyzeBidDocumentAsync(string documentContent)
    {
        var (apiKey, baseUrl, model) = GetAiSettings();
        var kernel = BuildKernel(apiKey, baseUrl, model);

        if (kernel == null)
        {
            _logger.LogWarning("AI API key not configured, using demo mode");
            return GenerateDemoAnalysis(documentContent);
        }

        try
        {
            var prompt = BidAnalysisPrompts.GetAnalysisPrompt(documentContent);

            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage(BidAnalysisPrompts.SystemPrompt);
            history.AddUserMessage(prompt);

            var response = await chat.GetChatMessageContentAsync(history);
            var json = response.Content ?? "{}";

            return JsonSerializer.Deserialize<BidAnalysisResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new BidAnalysisResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing bid document, falling back to demo mode");
            return GenerateDemoAnalysis(documentContent);
        }
    }

    public async Task<string> GenerateChapterAsync(BidChapterRequest request)
    {
        var (apiKey, baseUrl, model) = GetAiSettings();
        var kernel = BuildKernel(apiKey, baseUrl, model);

        if (kernel == null)
        {
            _logger.LogWarning("AI API key not configured, using demo mode");
            return GenerateDemoChapter(request);
        }

        try
        {
            var prompt = BidGeneratePrompts.GetChapterPrompt(request);

            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage(BidGeneratePrompts.SystemPrompt);
            history.AddUserMessage(prompt);

            var response = await chat.GetChatMessageContentAsync(history);
            return response.Content ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chapter {Chapter}", request.ChapterName);
            return GenerateDemoChapter(request);
        }
    }

    public async IAsyncEnumerable<string> GenerateChapterStreamAsync(BidChapterRequest request)
    {
        var (apiKey, baseUrl, model) = GetAiSettings();
        var kernel = BuildKernel(apiKey, baseUrl, model);

        if (kernel == null)
        {
            var demoContent = GenerateDemoChapter(request);
            var chars = demoContent.ToCharArray();
            foreach (var ch in chars)
            {
                yield return ch.ToString();
                await Task.Delay(10);
            }
            yield break;
        }

        var prompt = BidGeneratePrompts.GetChapterPrompt(request);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(BidGeneratePrompts.SystemPrompt);
        history.AddUserMessage(prompt);

        await foreach (var chunk in chat.GetStreamingChatMessageContentsAsync(history))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
                yield return chunk.Content;
        }
    }

    public async Task<BidReviewResult> ReviewBidDocumentAsync(string bidContent, string requirements)
    {
        var (apiKey, baseUrl, model) = GetAiSettings();
        var kernel = BuildKernel(apiKey, baseUrl, model);

        if (kernel == null)
        {
            _logger.LogWarning("AI API key not configured, using demo mode");
            return GenerateDemoReview();
        }

        try
        {
            var prompt = BidReviewPrompts.GetReviewPrompt(bidContent, requirements);

            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage(BidReviewPrompts.SystemPrompt);
            history.AddUserMessage(prompt);

            var response = await chat.GetChatMessageContentAsync(history);
            var json = response.Content ?? "{}";

            return JsonSerializer.Deserialize<BidReviewResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new BidReviewResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing bid document");
            return GenerateDemoReview();
        }
    }

    private BidAnalysisResult GenerateDemoAnalysis(string content)
    {
        var rng = new Random();
        var hasKeyword = content.Contains("招标") || content.Contains("投标") || content.Contains("磋商");

        return new BidAnalysisResult
        {
            ProjectName = hasKeyword ? ExtractProjectName(content) : "示例项目名称",
            ProjectCode = $"BID-{DateTime.UtcNow:yyyyMMdd}-{rng.Next(1000, 9999)}",
            Tenderer = "XX市建设局",
            Budget = Math.Round((decimal)(rng.Next(50, 500) + rng.NextDouble()), 2),
            Deadline = DateTime.UtcNow.AddDays(rng.Next(30, 90)),
            Qualifications = new List<QualificationItem>
            {
                new() { Content = "具有工程咨询甲级资质，未提供资质证明的不予资格审查通过", IsVeto = true, SourceRef = "p.14（演示数据）" },
                new() { Content = "近三年无不良信用记录", IsVeto = false, SourceRef = "p.15（演示数据）" },
                new() { Content = "项目负责人需具有高级职称", IsVeto = false, SourceRef = "p.16（演示数据）" }
            },
            TechnicalRequirements = new List<RequirementItem>
            {
                new() { Content = "技术方案需涵盖项目全过程", SourceRef = "p.20（演示数据）" },
                new() { Content = "提交成果需符合国家相关规范", SourceRef = "p.21（演示数据）" },
                new() { Content = "项目团队不少于5人", SourceRef = "p.22（演示数据）" }
            },
            CommercialRequirements = new List<RequirementItem>
            {
                new() { Content = "投标有效期90天", SourceRef = "p.25（演示数据）" },
                new() { Content = "履约保证金为合同金额的10%", SourceRef = "p.26（演示数据）" },
                new() { Content = "付款方式：3-3-3-1", SourceRef = "p.26（演示数据）" }
            },
            ScoringCriteria = new List<ScoringCriterion>
            {
                new() { Item = "技术方案", MaxScore = 40, Description = "方案的科学性、可行性、创新性", SourceRef = "p.29（演示数据）" },
                new() { Item = "商务报价", MaxScore = 30, Description = "价格合理性", SourceRef = "p.29（演示数据）" },
                new() { Item = "企业业绩", MaxScore = 20, Description = "类似项目经验", SourceRef = "p.30（演示数据）" },
                new() { Item = "人员配置", MaxScore = 10, Description = "团队资质和经验", SourceRef = "p.30（演示数据）" }
            },
            BidDocuments = new List<string>
            {
                "投标函及投标函附录",
                "法定代表人身份证明",
                "技术方案",
                "商务报价",
                "企业资质证书",
                "项目业绩证明"
            },
            SpecialNotes = new List<string>
            {
                "投标文件需密封递交",
                "逾期递交概不接受",
                "演示模式：请配置 AI:ApiKey 启用真实AI解析"
            },
            FormatRule = new FormatRule
            {
                Font = "宋体小四（演示数据）",
                PageLimit = 80,
                Binding = "胶装一正三副（演示数据）",
                SourceRef = "p.33（演示数据）"
            },
            NeedsReview = new List<string>
            {
                "演示模式生成，未做真实出处校验，请配置 AI:ApiKey 后重新解析"
            }
        };
    }

    private string ExtractProjectName(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Take(20))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 5 && trimmed.Length < 50 &&
                (trimmed.Contains("项目") || trimmed.Contains("工程") || trimmed.Contains("规划") ||
                 trimmed.Contains("咨询") || trimmed.Contains("设计")))
            {
                return trimmed;
            }
        }
        return "招标文件项目";
    }

    private string GenerateDemoChapter(BidChapterRequest request)
    {
        return request.ChapterName switch
        {
            "技术方案" => $@"## 一、项目理解与分析

本项目为{request.ProjectName}，我公司对该领域具有丰富的项目经验和深厚的技术积累。

## 二、技术路线

### 2.1 总体思路
采用'调研分析-方案编制-专家评审-成果提交'的标准化工作流程。

### 2.2 技术方法
- 文献研究法：系统梳理相关政策文件和技术规范
- 实地调研法：深入了解项目实际情况
- 专家咨询法：邀请行业专家进行技术指导

### 2.3 质量控制
建立三级质量审核体系，确保成果质量。

## 三、项目组织

### 3.1 项目团队
拟投入项目负责人1名，技术骨干4名，辅助人员2名。

### 3.2 进度安排
- 第一阶段：调研分析（15天）
- 第二阶段：方案编制（20天）
- 第三阶段：评审修改（10天）

## 四、服务承诺

我公司将严格按照合同约定，按时高质量完成各项任务。

---
> ⚠️ 演示模式生成，配置 AI:ApiKey 后将使用真实AI生成",

            "商务方案" => $@"## 一、公司概况

我公司是具有甲级资质的综合性咨询机构，注册资本XXX万元。

## 二、类似业绩

| 序号 | 项目名称 | 合同金额 | 完成时间 |
|------|---------|---------|---------|
| 1 | XX市XX项目 | XX万元 | 2024年 |
| 2 | YY市YY项目 | YY万元 | 2023年 |

## 三、报价说明

本报价包含完成本项目所需的全部费用，包括但不限于：
- 人工费
- 差旅费
- 资料费
- 管理费及利润

## 四、售后服务

- 免费提供技术咨询服务6个月
- 成果修改响应时间不超过3个工作日

---
> ⚠️ 演示模式生成，配置 AI:ApiKey 后将使用真实AI生成",

            _ => $@"## {request.ChapterName}

### 概述

针对{request.ProjectName}项目，我公司将以专业的技术能力和丰富的项目经验，为业主提供优质服务。

### 具体内容

本章节将详细阐述{request.ChapterName}的具体内容和实施方案，确保满足招标文件的各项要求。

### 保障措施

建立完善的质量保障体系，确保项目顺利实施。

---
> ⚠️ 演示模式生成，配置 AI:ApiKey 后将使用真实AI生成"
        };
    }

    private BidReviewResult GenerateDemoReview()
    {
        return new BidReviewResult
        {
            OverallScore = 78,
            IsComplete = false,
            Issues = new List<BidReviewIssue>
            {
                new() { Chapter = "技术方案", Severity = "medium", Description = "技术路线描述较为笼统，建议增加具体实施步骤", Suggestion = "补充各阶段的详细工作内容和交付物" },
                new() { Chapter = "商务方案", Severity = "low", Description = "类似业绩数量偏少", Suggestion = "建议补充更多类似项目经验" }
            },
            Suggestions = new List<string>
            {
                "建议增加项目进度甘特图",
                "技术方案中补充质量控制措施的具体指标",
                "商务报价部分建议增加费用明细表"
            },
            MissingItems = new List<string>
            {
                "项目进度计划表",
                "质量保证措施详细说明",
                "风险分析及应对方案"
            }
        };
    }
}

public class AiConfigDto
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4o";
}
