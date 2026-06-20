using EnterpriseMS.Services.AI.Models;

namespace EnterpriseMS.Services.AI.Prompts;

public static class BidGeneratePrompts
{
    public static string SystemPrompt => @"
你是一个资深的投标文档撰写专家，具有10年以上的标书编写经验。
你需要根据招标要求和项目信息，撰写专业、完整、有竞争力的标书内容。

写作原则：
1. 语言专业、正式、准确，使用行业标准术语
2. 内容完整，覆盖所有评分要点
3. 突出公司优势和项目经验
4. 方案具有可操作性和针对性
5. 结构清晰，逻辑严密
6. 适当引用相关标准和规范

禁止事项：
1. 不要编造虚假的项目经验或资质
2. 不要使用过于夸张的表述
3. 不要遗漏招标文件中的关键要求
4. 不要照搬模板，要根据具体项目定制";

    public static string GetChapterPrompt(BidChapterRequest request) => $@"
请为以下项目撰写标书的「{request.ChapterName}」章节：

---项目信息---
项目名称：{request.ProjectName}
项目类型：{request.ProjectType ?? "未指定"}
项目概况：{request.ProjectDescription ?? "未提供"}
---结束---

---招标要求---
{request.Requirements ?? "未提供具体要求"}
---结束---

{(request.ScoringCriteria.Any() ? $"---评分标准---\n{string.Join("\n", request.ScoringCriteria)}\n---结束---" : "")}

{(string.IsNullOrEmpty(request.CompanyInfo) ? "" : $"---公司信息---\n{request.CompanyInfo}\n---结束---")}

{(string.IsNullOrEmpty(request.TemplateContent) ? "" : $"---参考模板---\n{request.TemplateContent}\n---结束---")}

{(string.IsNullOrEmpty(request.ReferenceContent) ? "" : $"---参考内容---\n{request.ReferenceContent}\n---结束---")}

{(string.IsNullOrEmpty(request.CustomRequirements) ? "" : $"---自定义要求（请优先满足）---\n{request.CustomRequirements}\n---结束---")}

请撰写该章节的完整内容，要求：
1. 内容详实，字数约{request.TargetWordCount}字
2. 结构清晰，使用二级标题分节论述
3. 重点突出评分标准中的得分点
4. 体现专业性和可操作性
5. 直接输出正文内容，不要输出章节标题（标题已由系统添加）
6. 优先满足自定义要求中的特殊需求";
}
