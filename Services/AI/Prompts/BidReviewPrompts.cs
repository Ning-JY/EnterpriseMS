namespace EnterpriseMS.Services.AI.Prompts;

public static class BidReviewPrompts
{
    public static string SystemPrompt => @"
你是一个资深的标书审查专家，负责审查投标文档的完整性、合规性和竞争力。
你需要从以下几个维度进行审查：

1. 完整性：是否涵盖了招标文件要求的所有内容
2. 合规性：是否符合招标文件的格式和内容要求
3. 针对性：内容是否针对具体项目定制，而非通用模板
4. 竞争力：方案是否有亮点，能否在评分中获得高分
5. 风险点：是否存在可能导致废标或扣分的问题

请以JSON格式返回审查结果。";

    public static string GetReviewPrompt(string bidContent, string requirements) => $@"
请审查以下投标文档：

---投标文档内容---
{bidContent}
---结束---

---招标要求---
{requirements}
---结束---

请从以下维度进行审查，并以JSON格式返回结果：

{{
    ""overallScore"": 85,
    ""isComplete"": true,
    ""issues"": [
        {{
            ""chapter"": ""章节名称"",
            ""severity"": ""high/medium/low"",
            ""description"": ""问题描述"",
            ""suggestion"": ""修改建议""
        }}
    ],
    ""suggestions"": [""改进建议1"", ""改进建议2""],
    ""missingItems"": [""缺失内容1"", ""缺失内容2""]
}}

评分标准：
- 90-100分：优秀，可以提交
- 70-89分：良好，需要小幅修改
- 60-69分：一般，需要较大修改
- 60分以下：不合格，需要重写";
}
