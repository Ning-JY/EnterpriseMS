using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace EnterpriseMS.Services.Export;

/// <summary>
/// 把AI生成的Markdown文本（## 二级标题、### 三级标题、- 列表、&gt; 引用、**粗体**）
/// 转换成真正的Word段落/标题样式，而不是把"##"当作普通文字字面输出。
///
/// 范围说明：列表项用"•"前缀+悬挂缩进模拟，没有接入Word原生编号体系（NumberingDefinitionsPart）。
/// 这是有意简化——原生列表需要额外维护 numbering.xml 部件，对于标书正文这种以分节论述为主、
/// 列表层级不深的场景，视觉效果已经足够专业，没有必要为此引入更复杂、更难维护的实现。
/// </summary>
public static class MarkdownToOpenXmlConverter
{
    public static List<OpenXmlElement> Convert(string? markdown)
    {
        var elements = new List<OpenXmlElement>();
        if (string.IsNullOrWhiteSpace(markdown)) return elements;

        var lines = markdown.Replace("\r\n", "\n").Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.TrimStart();

            if (string.IsNullOrWhiteSpace(trimmed))
                continue; // 空行：靠段落自带的 spacing 控制间距，不额外插入空段落

            if (trimmed.StartsWith("### "))
            {
                elements.Add(BuildHeading(trimmed[4..].Trim(), "Heading3"));
            }
            else if (trimmed.StartsWith("## "))
            {
                elements.Add(BuildHeading(trimmed[3..].Trim(), "Heading2"));
            }
            else if (trimmed.StartsWith("# "))
            {
                elements.Add(BuildHeading(trimmed[2..].Trim(), "Heading1"));
            }
            else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("• "))
            {
                elements.Add(BuildBulletParagraph(trimmed[2..].Trim()));
            }
            else if (trimmed.StartsWith("> "))
            {
                elements.Add(BuildQuoteParagraph(trimmed[2..].Trim()));
            }
            else if (trimmed == "---" || trimmed == "***")
            {
                elements.Add(BuildDividerParagraph());
            }
            else
            {
                elements.Add(BuildBodyParagraph(trimmed));
            }
        }

        return elements;
    }

    private static Paragraph BuildHeading(string text, string styleId)
    {
        var p = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
        p.Append(BuildInlineRuns(text, bold: true));
        return p;
    }

    private static Paragraph BuildBulletParagraph(string text)
    {
        var p = new Paragraph(new ParagraphProperties(
            new Indentation { Left = "720", Hanging = "360" },
            new SpacingBetweenLines { After = "80" }));
        var bulletRun = new Run(new RunProperties(), new Text("• ") { Space = SpaceProcessingModeValues.Preserve });
        p.Append(bulletRun);
        foreach (var r in BuildInlineRuns(text)) p.Append(r);
        return p;
    }

    private static Paragraph BuildQuoteParagraph(string text)
    {
        var p = new Paragraph(new ParagraphProperties(
            new Indentation { Left = "480" },
            new SpacingBetweenLines { After = "120" }));
        var runProps = new RunProperties(new Italic(), new Color { Val = "595959" });
        p.Append(new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        return p;
    }

    private static Paragraph BuildDividerParagraph()
    {
        return new Paragraph(new ParagraphProperties(
            new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "BFBFBF" }),
            new SpacingBetweenLines { After = "120" }));
    }

    private static Paragraph BuildBodyParagraph(string text)
    {
        var p = new Paragraph(new ParagraphProperties(
            new SpacingBetweenLines { After = "120", Line = "360", LineRule = LineSpacingRuleValues.Auto },
            new Indentation { FirstLine = "480" })); // 首行缩进2字符，符合中文正式文档排版习惯
        foreach (var r in BuildInlineRuns(text)) p.Append(r);
        return p;
    }

    /// <summary>处理行内 **粗体** 标记，拆分成多个 Run；不支持嵌套或斜体内联（标书正文里基本不会用到，保持简单可靠）。</summary>
    private static List<Run> BuildInlineRuns(string text, bool bold = false)
    {
        var runs = new List<Run>();
        var segments = text.Split("**");
        for (int i = 0; i < segments.Length; i++)
        {
            if (string.IsNullOrEmpty(segments[i])) continue;
            var isBold = bold || (i % 2 == 1); // 奇数段位于一对**之间
            var props = isBold ? new RunProperties(new Bold()) : new RunProperties();
            runs.Add(new Run(props, new Text(segments[i]) { Space = SpaceProcessingModeValues.Preserve }));
        }
        if (runs.Count == 0)
            runs.Add(new Run(new RunProperties(), new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        return runs;
    }
}
