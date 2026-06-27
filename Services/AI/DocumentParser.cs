using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using EnterpriseMS.Common;

namespace EnterpriseMS.Services.AI;

/// <summary>
/// 单个分块：对应招标文件中的一个章节（或在无法识别章节标题时，对应一段固定页数/段落区间）。
/// SourceHint 是该分块在原文中的定位描述，会被拼进喂给AI的文本里，
/// 这样AI在抽取信息时可以直接引用，而不是凭空编造页码。
/// </summary>
public class DocumentChunk
{
    public string SectionLabel { get; set; } = "";
    public string SourceHint { get; set; } = "";
    public string Text { get; set; } = "";
}

/// <summary>
/// 解析后的文档：保留分块结构，而不是早期版本里"整篇拼成一个字符串"的做法。
/// 分块的意义有两个：① 168页级别的文档不会因为超出单次上下文而被截断或忽略后半部分；
/// ② 每个分块自带 SourceHint，AI抽取结果时能标注出处，便于反向校验。
/// </summary>
public class ParsedDocument
{
    public List<DocumentChunk> Chunks { get; set; } = new();
    /// <summary>PDF有可靠页码；docx在OOXML层面没有真实分页（分页是Word渲染时才计算的），
    /// 只能退化为"章节/段落"定位，因此用该字段告知下游：这份文档的 SourceRef 是页码还是章节定位。</summary>
    public bool HasReliablePageNumbers { get; set; }

    /// <summary>把所有分块重新拼接为带标记的全文，主要用于小文档单次调用AI或调试查看。</summary>
    public string ToTaggedFullText() => string.Join("\n\n", Chunks.Select(c => $"[{c.SourceHint}]\n{c.Text}"));
}

public class DocumentParser
{
    private readonly ILogger<DocumentParser> _logger;

    /// <summary>每个分块的字符数上限（粗略对应约6000-8000 token，给系统提示词和返回JSON留足空间）。
    /// 没有现成的中文分词/计数工具，用字符数做保守估算。</summary>
    private const int MaxChunkChars = 12000;

    // 招标文件常见章节标题关键词，用于识别分块边界。命中任意一个即认为是新章节的起点。
    private static readonly Regex SectionHeadingPattern = new(
        @"^\s*(第[一二三四五六七八九十百]+[章节部分]\s*[、\.]?\s*)?" +
        @"(招标公告|投标邀请书|投标邀请函|投标人须知|资格审查|资格预审|评标办法|评标标准|评标方法|" +
        @"技术规格|技术规格书|技术要求|技术标准|合同条款|合同主要条款|合同书|" +
        @"投标文件格式|投标文件组成|开标|评标定标|附录)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public DocumentParser(ILogger<DocumentParser> logger)
    {
        _logger = logger;
    }

    public ParsedDocument Parse(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".pdf" => ParsePdf(fileStream),
            ".docx" or ".doc" => ParseWord(fileStream),
            _ => throw new BusinessException($"不支持的文件格式: {extension}")
        };
    }

    /// <summary>保留旧接口（返回拼好的纯文本），兼容暂不需要分块定位的调用方。</summary>
    public string ParseToFlatText(Stream fileStream, string fileName) => Parse(fileStream, fileName).ToTaggedFullText();

    private ParsedDocument ParsePdf(Stream fileStream)
    {
        try
        {
            using var document = PdfDocument.Open(fileStream);

            // 先按页提取文本，每页前插入 [P.n] 标记，页码是PDF天然具备、最可靠的定位信息。
            var pageTexts = new List<(int PageNumber, string Text)>();
            foreach (var page in document.GetPages())
            {
                pageTexts.Add((page.Number, page.Text ?? ""));
            }

            if (pageTexts.Count == 0)
                throw new BusinessException("PDF文件内容为空或无法提取文本（可能是纯扫描件，当前暂不支持OCR）");

            var chunks = ChunkByHeadingsWithPages(pageTexts);

            return new ParsedDocument { Chunks = chunks, HasReliablePageNumbers = true };
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PDF");
            throw new BusinessException("PDF文件解析失败，请确认文件未损坏且包含可提取的文本层（扫描件暂不支持）");
        }
    }

    private ParsedDocument ParseWord(Stream fileStream)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(fileStream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new BusinessException("Word文档内容为空");

            // docx 在 OOXML 层面没有真实的"页"概念（分页是 Word 渲染时按字体/页面大小动态计算的），
            // 因此这里退化为"段落序号 + 最近的章节标题"作为定位依据，且明确告知下游 HasReliablePageNumbers=false。
            var paragraphs = body.Elements<Paragraph>().ToList();
            var pseudoPages = new List<(int PageNumber, string Text)>();
            var sb = new StringBuilder();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var text = paragraphs[i].InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine($"[¶{i + 1}] {text}");
                }
                // 用每50个自然段模拟一个"页"分组，仅用于复用同一套分块算法，不代表真实页码。
                if ((i + 1) % 50 == 0 || i == paragraphs.Count - 1)
                {
                    if (sb.Length > 0)
                    {
                        pseudoPages.Add((pseudoPages.Count + 1, sb.ToString()));
                        sb.Clear();
                    }
                }
            }

            if (pseudoPages.Count == 0)
                throw new BusinessException("Word文档内容为空或无可识别段落");

            var chunks = ChunkByHeadingsWithPages(pseudoPages, sourceLabelPrefix: "¶区间");

            return new ParsedDocument { Chunks = chunks, HasReliablePageNumbers = false };
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Word document");
            throw new BusinessException("Word文档解析失败");
        }
    }

    /// <summary>
    /// 按章节标题切分页面集合；找不到任何标题命中时，退化为按固定页数（约每15"页"一块）切分，
    /// 保证超长文档不会被整篇丢给AI导致超出上下文或后半部分被模型忽略。
    /// </summary>
    private List<DocumentChunk> ChunkByHeadingsWithPages(
        List<(int PageNumber, string Text)> pages,
        string sourceLabelPrefix = "p")
    {
        // 第一步：定位每一页文本中第一处命中章节标题的位置，记录"该页起新章节"。
        var sectionStarts = new List<(int PageIndex, string Label)>();
        for (int i = 0; i < pages.Count; i++)
        {
            var match = SectionHeadingPattern.Match(pages[i].Text);
            if (match.Success)
            {
                var label = match.Value.Trim();
                // 避免同一标题因跨行重复匹配导致连续重复记录
                if (sectionStarts.Count == 0 || sectionStarts[^1].Label != label)
                    sectionStarts.Add((i, label));
            }
        }

        var chunks = new List<DocumentChunk>();

        if (sectionStarts.Count > 0)
        {
            for (int s = 0; s < sectionStarts.Count; s++)
            {
                var (startIdx, label) = sectionStarts[s];
                var endIdx = (s + 1 < sectionStarts.Count) ? sectionStarts[s + 1].PageIndex - 1 : pages.Count - 1;
                AppendChunksForRange(chunks, pages, startIdx, endIdx, label, sourceLabelPrefix);
            }

            // 第一个识别到的章节之前如果还有内容（如封面、目录），单独成块，标记为"前置内容"。
            if (sectionStarts[0].PageIndex > 0)
            {
                AppendChunksForRange(chunks, pages, 0, sectionStarts[0].PageIndex - 1, "前置内容（封面/目录等）", sourceLabelPrefix, insertAtFront: true);
            }
        }
        else
        {
            // 没有命中任何标题关键词：退化为固定页数切块，每块约15"页"，仍保留页码范围作为 SourceHint。
            const int pagesPerChunk = 15;
            for (int start = 0; start < pages.Count; start += pagesPerChunk)
            {
                var end = Math.Min(start + pagesPerChunk - 1, pages.Count - 1);
                AppendChunksForRange(chunks, pages, start, end, "未识别章节标题（按页数切分）", sourceLabelPrefix);
            }
        }

        return chunks;
    }

    /// <summary>把 [start, end] 页区间的文本收集起来，如果超过单块字符上限，再按字符数二次切分。</summary>
    private void AppendChunksForRange(
        List<DocumentChunk> chunks,
        List<(int PageNumber, string Text)> pages,
        int startIdx, int endIdx,
        string label, string sourceLabelPrefix,
        bool insertAtFront = false)
    {
        var combined = new StringBuilder();
        foreach (var idx in Enumerable.Range(startIdx, endIdx - startIdx + 1))
        {
            combined.AppendLine($"[{sourceLabelPrefix}.{pages[idx].PageNumber}]");
            combined.AppendLine(pages[idx].Text);
        }
        var text = combined.ToString();
        var pageRangeHint = startIdx == endIdx
            ? $"{label} {sourceLabelPrefix}.{pages[startIdx].PageNumber}"
            : $"{label} {sourceLabelPrefix}.{pages[startIdx].PageNumber}-{sourceLabelPrefix}.{pages[endIdx].PageNumber}";

        var subChunks = SplitByCharLimit(text, MaxChunkChars);
        var newChunks = subChunks.Select((t, i) => new DocumentChunk
        {
            SectionLabel = label,
            SourceHint = subChunks.Count > 1 ? $"{pageRangeHint}（第{i + 1}/{subChunks.Count}段）" : pageRangeHint,
            Text = t
        }).ToList();

        if (insertAtFront)
            chunks.InsertRange(0, newChunks);
        else
            chunks.AddRange(newChunks);
    }

    private List<string> SplitByCharLimit(string text, int maxChars)
    {
        if (text.Length <= maxChars) return new List<string> { text };

        var result = new List<string>();
        var lines = text.Split('\n');
        var current = new StringBuilder();
        foreach (var line in lines)
        {
            if (current.Length + line.Length + 1 > maxChars && current.Length > 0)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            current.AppendLine(line);
        }
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }
}
