using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using EnterpriseMS.Services.AI.Models;
using EnterpriseMS.Services.DTOs.Bid;

namespace EnterpriseMS.Services.Export;

public interface IWordExportService
{
    /// <summary>
    /// 生成投标文件Word文档。formatRule 来自模块一AI解析出的格式要求（字体、页数上限等），
    /// 找不到/解析不出时使用安全默认值，不阻断导出。
    /// 返回值附带 Warnings——格式相关的提示（如预估页数超限），但不会阻止文件生成：
    /// 用户始终能拿到文件，是否要据此调整内容由人工决定。
    /// </summary>
    (byte[] FileBytes, List<string> Warnings) BuildDocx(
        string projectName, string projectCode, string? tenderer,
        BidAssemblePart part, FormatRule? formatRule);

    /// <summary>
    /// 把已生成的docx字节转换为PDF。依赖服务器上安装的LibreOffice(soffice)命令行工具——
    /// 这是部署环境的外部依赖，不是.NET库能内置解决的，找不到该命令时返回 (null, 明确的错误说明)，
    /// 而不是抛出一个让人摸不着头脑的异常。
    /// </summary>
    Task<(byte[]? FileBytes, string? Error)> ConvertDocxToPdfAsync(byte[] docxBytes);
}

public class WordExportService : IWordExportService
{
    // 粗略估算：A4页面、小四字号、单倍行距下，一页中文正文大约能容纳的字数。
    // 仅用于"预估页数是否超限"的提示，不是精确计算——OpenXml层面无法在不渲染的情况下得到真实分页结果，
    // 这一点必须对用户讲清楚，不能让"预估"看起来像"精确值"。
    private const int ApproxCharsPerPage = 600;
    private readonly ILogger<WordExportService> _logger;

    public WordExportService(ILogger<WordExportService> logger)
    {
        _logger = logger;
    }

    public async Task<(byte[]? FileBytes, string? Error)> ConvertDocxToPdfAsync(byte[] docxBytes)
    {
        var workDir = Path.Combine(Path.GetTempPath(), "bid-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
        var docxPath = Path.Combine(workDir, "input.docx");
        var pdfPath = Path.Combine(workDir, "input.pdf");

        try
        {
            await File.WriteAllBytesAsync(docxPath, docxBytes);

            var sofficeCmd = OperatingSystem.IsWindows() ? "soffice.exe" : "soffice";
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = sofficeCmd,
                Arguments = $"--headless --convert-to pdf --outdir \"{workDir}\" \"{docxPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null)
                return (null, "无法启动 LibreOffice(soffice) 进程，请确认部署环境已安装 LibreOffice 并配置到系统 PATH 中。");

            var stderrTask = process.StandardError.ReadToEndAsync();
            var completed = await Task.Run(() => process.WaitForExit(60_000));
            if (!completed)
            {
                try { process.Kill(); } catch { /* 进程可能已自行退出，忽略 */ }
                return (null, "PDF转换超时（60秒），文档可能过大或LibreOffice进程异常，请改用Word格式下载，或联系管理员检查服务器环境。");
            }

            if (!File.Exists(pdfPath))
            {
                var stderr = await stderrTask;
                _logger.LogWarning("soffice转换未生成PDF文件，stderr: {Stderr}", stderr);
                return (null, "PDF转换失败，未生成输出文件。服务器可能未正确安装LibreOffice，请改用Word格式下载。");
            }

            var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
            return (pdfBytes, null);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // 找不到 soffice 命令本身（环境变量PATH里没有），这是最常见的部署环境缺失场景。
            return (null, "服务器未安装 LibreOffice（找不到 soffice 命令），无法转换为PDF。请联系管理员安装 LibreOffice，或改用Word格式下载。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF转换异常");
            return (null, $"PDF转换过程中发生异常：{ex.Message}");
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch { /* 临时文件清理失败不影响主流程 */ }
        }
    }

    public (byte[] FileBytes, List<string> Warnings) BuildDocx(
        string projectName, string projectCode, string? tenderer,
        BidAssemblePart part, FormatRule? formatRule)
    {
        var warnings = new List<string>();
        var (fontName, bodySize) = ChineseFontFormatParser.Parse(formatRule?.Font);

        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();
            mainPart.Document.Append(body);

            BuildStyles(mainPart, fontName, bodySize);

            var headerPart = mainPart.AddNewPart<HeaderPart>();
            BuildHeader(headerPart, projectCode);
            var footerPart = mainPart.AddNewPart<FooterPart>();
            BuildFooter(footerPart);

            // 封面
            foreach (var el in BuildCoverPage(projectName, tenderer, projectCode))
                body.Append(el);
            body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

            // 目录字段：Word打开后需要手动"更新域"（右键→更新字段，或F9）才会显示真实页码，
            // 这是Word字段机制本身的限制，OpenXml生成阶段无法预先渲染出页码。
            foreach (var el in BuildTocField())
                body.Append(el);
            body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

            // 章节正文
            var totalChars = 0;
            foreach (var chapter in part.Chapters)
            {
                var heading = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }));
                heading.Append(new Run(new Text(chapter.Name)));
                body.Append(heading);

                foreach (var el in MarkdownToOpenXmlConverter.Convert(chapter.Content))
                    body.Append(el);

                body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
                totalChars += chapter.Content?.Length ?? 0;
            }

            // 页眉/页脚/页码格式通过 SectionProperties 关联到正文最后一节
            var sectionProps = new SectionProperties(
                new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) },
                new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) },
                new PageSize { Width = 11906, Height = 16838 }, // A4
                new PageMargin { Top = 1440, Right = 1440, Bottom = 1440, Left = 1440, Header = 720, Footer = 720 }
            );
            body.Append(sectionProps);

            // 格式合规提示：预估页数是否超出招标文件要求的页数上限——
            // 这是导出后的最后一道提示，但不阻断导出，因为用户随时需要先拿到文件再决定是否精简内容。
            if (formatRule?.PageLimit is int limit && limit > 0)
            {
                var estimatedPages = Math.Max(1, (int)Math.Ceiling(totalChars / (double)ApproxCharsPerPage));
                if (estimatedPages > limit)
                {
                    warnings.Add($"预估正文约 {estimatedPages} 页（按每页约{ApproxCharsPerPage}字粗略估算），" +
                                 $"超出招标文件要求的页数上限（{limit}页），建议精简内容；该数字仅为估算，并非Word实际分页结果。");
                }
            }
            if (string.IsNullOrWhiteSpace(formatRule?.Font))
            {
                warnings.Add($"未能从招标要素表中识别出明确的字体要求，已使用默认格式（{fontName}、小四），请人工核对招标文件原文的格式要求。");
            }

            mainPart.Document.Save();
        }

        return (stream.ToArray(), warnings);
    }

    private void BuildStyles(MainDocumentPart mainPart, string fontName, int bodySize)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        styles.Append(new DocDefaults(
            new RunPropertiesDefault(new RunPropertiesBaseStyle(
                new RunFonts { Ascii = fontName, EastAsia = fontName, HighAnsi = fontName },
                new FontSize { Val = bodySize.ToString() }))));

        styles.Append(BuildStyle("Normal", "正文", null, fontName, bodySize, bold: false, isDefault: true));
        styles.Append(BuildStyle("Heading1", "标题 1", "Normal", fontName, bodySize + 8, bold: true));
        styles.Append(BuildStyle("Heading2", "标题 2", "Normal", fontName, bodySize + 4, bold: true));
        styles.Append(BuildStyle("Heading3", "标题 3", "Normal", fontName, bodySize + 2, bold: true));
        styles.Append(BuildStyle("Title", "标题", "Normal", fontName, bodySize + 16, bold: true));

        stylesPart.Styles = styles;
    }

    private Style BuildStyle(string id, string name, string? basedOn, string fontName, int size, bool bold, bool isDefault = false)
    {
        var style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = id,
            Default = isDefault ? OnOffValue.FromBoolean(true) : null
        };
        style.Append(new StyleName { Val = name });
        if (basedOn != null) style.Append(new BasedOn { Val = basedOn });

        // CT_Style 的子元素顺序要求 pPr 必须出现在 rPr 之前，顺序写反会导致Word无法打开文档（OOXML schema校验失败）。
        if (id.StartsWith("Heading"))
        {
            style.Append(new StyleParagraphProperties(
                new SpacingBetweenLines { Before = "240", After = "120" },
                new KeepNext()));
        }

        var runProps = new StyleRunProperties(
            new RunFonts { Ascii = fontName, EastAsia = fontName, HighAnsi = fontName },
            new FontSize { Val = size.ToString() });
        if (bold) runProps.Append(new Bold());
        style.Append(runProps);

        return style;
    }

    private void BuildHeader(HeaderPart headerPart, string projectCode)
    {
        var p = new Paragraph(new ParagraphProperties(
            new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "BFBFBF" })));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }, new Color { Val = "808080" }),
            new Text($"项目编号：{projectCode}") { Space = SpaceProcessingModeValues.Preserve }));
        headerPart.Header = new Header(p);
    }

    private void BuildFooter(FooterPart footerPart)
    {
        var p = new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.Begin }));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }), new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.Separate }));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }), new Text("1")));
        p.Append(new Run(new RunProperties(new FontSize { Val = "18" }), new FieldChar { FieldCharType = FieldCharValues.End }));
        footerPart.Footer = new Footer(p);
    }

    private IEnumerable<OpenXmlElement> BuildCoverPage(string projectName, string? tenderer, string projectCode)
    {
        var elements = new List<OpenXmlElement>();

        var spacer = new Paragraph(new ParagraphProperties(new SpacingBetweenLines { Before = "2400" }));
        elements.Add(spacer);

        var title = new Paragraph(new ParagraphProperties(
            new ParagraphStyleId { Val = "Title" }, new Justification { Val = JustificationValues.Center }));
        title.Append(new Run(new Text(projectName)));
        elements.Add(title);

        var subtitle = new Paragraph(new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { Before = "240", After = "960" }));
        subtitle.Append(new Run(new RunProperties(new FontSize { Val = "32" }), new Text("投 标 文 件")));
        elements.Add(subtitle);

        if (!string.IsNullOrWhiteSpace(tenderer))
            elements.Add(BuildCoverInfoLine($"招  标  人：{tenderer}"));
        elements.Add(BuildCoverInfoLine($"项目编号：{projectCode}"));
        elements.Add(BuildCoverInfoLine($"编制日期：{DateTime.Now:yyyy 年 MM 月 dd 日}"));

        return elements;
    }

    private Paragraph BuildCoverInfoLine(string text)
    {
        var p = new Paragraph(new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "160" }));
        p.Append(new Run(new RunProperties(new FontSize { Val = "24" }), new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        return p;
    }

    private IEnumerable<OpenXmlElement> BuildTocField()
    {
        var heading = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }));
        heading.Append(new Run(new Text("目录")));

        var fieldPara = new Paragraph();
        fieldPara.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }));
        fieldPara.Append(new Run(new FieldCode(@" TOC \o ""1-3"" \h \z \u ") { Space = SpaceProcessingModeValues.Preserve }));
        fieldPara.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }));
        fieldPara.Append(new Run(new Text("右键点击此处选择\"更新字段\"以生成目录") { Space = SpaceProcessingModeValues.Preserve }));
        fieldPara.Append(new Run(new FieldChar { FieldCharType = FieldCharValues.End }));

        return new List<OpenXmlElement> { heading, fieldPara };
    }
}
