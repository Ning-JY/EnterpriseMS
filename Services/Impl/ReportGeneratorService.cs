using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Report;
using Microsoft.Extensions.Logging;

namespace EnterpriseMS.Services.Impl;

public interface IReportGeneratorService
{
    List<TemplateInfoDto> GetTemplates();
    TemplateInfoDto? GetTemplate(string templateId);
    List<TemplatePlaceholderDto> ScanPlaceholders(string templateId);
    string ConfigureTemplate(ConfigureTemplateRequest request, IFormFile templateFile);
    string FillTemplate(string templateId, Dictionary<string, string> fieldValues);
    byte[] GenerateDocument(string templateId, Dictionary<string, string> fieldValues);
    bool DeleteTemplate(string templateId);
    (byte[]? Bytes, string FileName) GetTemplateFile(string templateId);
}

public class ReportGeneratorService : IReportGeneratorService
{
    private readonly string _templateRoot;
    private readonly string _docRoot;
    private readonly ILogger<ReportGeneratorService> _logger;

    public ReportGeneratorService(IConfiguration config, ILogger<ReportGeneratorService> logger)
    {
        _templateRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
        _docRoot = Path.Combine(Directory.GetCurrentDirectory(), "doc", "模板文件");
        _logger = logger;

        if (!Directory.Exists(_templateRoot))
            Directory.CreateDirectory(_templateRoot);
    }

    public List<TemplateInfoDto> GetTemplates()
    {
        var manifestPath = Path.Combine(_templateRoot, "template-manifest.json");
        if (!File.Exists(manifestPath))
            return new List<TemplateInfoDto>();

        var json = File.ReadAllText(manifestPath);
        var manifest = System.Text.Json.JsonSerializer.Deserialize<ManifestWrapper>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return manifest?.Templates ?? new List<TemplateInfoDto>();
    }

    public TemplateInfoDto? GetTemplate(string templateId)
    {
        return GetTemplates().FirstOrDefault(t => t.Id == templateId);
    }

    public List<TemplatePlaceholderDto> ScanPlaceholders(string templateId)
    {
        var template = GetTemplate(templateId);
        if (template == null)
            throw new BusinessException("模板不存在");

        var filePath = GetTemplateFilePath(template.FileName);
        if (!File.Exists(filePath))
            throw new BusinessException($"模板文件不存在: {template.FileName}");

        var placeholders = new List<TemplatePlaceholderDto>();
        var pattern = new Regex(@"\{\{(.+?)\}\}");

        using var stream = new MemoryStream();
        using (var fileStream = File.OpenRead(filePath))
            fileStream.CopyTo(stream);

        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return placeholders;

        int paraIndex = 0;
        foreach (var para in body.Descendants<Paragraph>())
        {
            var text = para.InnerText;
            if (string.IsNullOrEmpty(text)) { paraIndex++; continue; }

            var matches = pattern.Matches(text);
            foreach (Match m in matches)
            {
                placeholders.Add(new TemplatePlaceholderDto
                {
                    Name = m.Groups[1].Value,
                    ParagraphIndex = paraIndex,
                    Context = "正文",
                    SurroundingText = GetSurroundingText(text, m.Index, m.Length)
                });
            }
            paraIndex++;
        }

        int tableIndex = 0;
        foreach (var table in body.Descendants<Table>())
        {
            foreach (var row in table.Descendants<TableRow>())
            {
                foreach (var cell in row.Descendants<TableCell>())
                {
                    var text = cell.InnerText;
                    if (string.IsNullOrEmpty(text)) continue;

                    var matches = pattern.Matches(text);
                    foreach (Match m in matches)
                    {
                        placeholders.Add(new TemplatePlaceholderDto
                        {
                            Name = m.Groups[1].Value,
                            ParagraphIndex = tableIndex,
                            Context = $"表格第{tableIndex + 1}个",
                            SurroundingText = GetSurroundingText(text, m.Index, m.Length)
                        });
                    }
                }
            }
            tableIndex++;
        }

        return placeholders.DistinctBy(p => p.Name).ToList();
    }

    public string ConfigureTemplate(ConfigureTemplateRequest request, IFormFile templateFile)
    {
        if (templateFile == null || templateFile.Length == 0)
            throw new BusinessException("请上传模板文件");

        var templateId = GenerateTemplateId(request.TemplateName);
        var fileName = $"{templateId}.docx";
        var filePath = Path.Combine(_templateRoot, fileName);

        using var stream = new MemoryStream();
        templateFile.CopyTo(stream);
        stream.Position = 0;

        using (var doc = WordprocessingDocument.Open(stream, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new BusinessException("无法读取模板文件内容");

            foreach (var replacement in request.Replacements)
            {
                if (string.IsNullOrWhiteSpace(replacement.OldText) || string.IsNullOrWhiteSpace(replacement.FieldName))
                    continue;

                var placeholder = $"{{{{{replacement.FieldName}}}}}";
                ReplaceTextInBody(body, replacement.OldText, placeholder);
            }

            doc.MainDocumentPart.Document.Save();
        }

        File.WriteAllBytes(filePath, stream.ToArray());

        UpdateManifest(request, fileName, templateId);

        return templateId;
    }

    public string FillTemplate(string templateId, Dictionary<string, string> fieldValues)
    {
        var template = GetTemplate(templateId);
        if (template == null)
            throw new BusinessException("模板不存在");

        var filePath = GetTemplateFilePath(template.FileName);
        if (!File.Exists(filePath))
            throw new BusinessException($"模板文件不存在: {template.FileName}");

        using var stream = new MemoryStream();
        using (var fileStream = File.OpenRead(filePath))
            fileStream.CopyTo(stream);

        using (var doc = WordprocessingDocument.Open(stream, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new BusinessException("无法读取模板文件内容");

            FillPlaceholdersInBody(body, fieldValues);

            foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
            {
                if (headerPart.Header?.InnerText != null)
                    FillPlaceholdersInElement(headerPart.Header, fieldValues);
            }

            foreach (var footerPart in doc.MainDocumentPart.FooterParts)
            {
                if (footerPart.Footer?.InnerText != null)
                    FillPlaceholdersInElement(footerPart.Footer, fieldValues);
            }

            doc.MainDocumentPart.Document.Save();
        }

        return Convert.ToBase64String(stream.ToArray());
    }

    public byte[] GenerateDocument(string templateId, Dictionary<string, string> fieldValues)
    {
        var template = GetTemplate(templateId);
        if (template == null)
            throw new BusinessException("模板不存在");

        var filePath = GetTemplateFilePath(template.FileName);
        if (!File.Exists(filePath))
            throw new BusinessException($"模板文件不存在: {template.FileName}");

        using var stream = new MemoryStream();
        using (var fileStream = File.OpenRead(filePath))
            fileStream.CopyTo(stream);

        using (var doc = WordprocessingDocument.Open(stream, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new BusinessException("无法读取模板文件内容");

            FillPlaceholdersInBody(body, fieldValues);

            foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
            {
                if (headerPart.Header?.InnerText != null)
                    FillPlaceholdersInElement(headerPart.Header, fieldValues);
            }

            foreach (var footerPart in doc.MainDocumentPart.FooterParts)
            {
                if (footerPart.Footer?.InnerText != null)
                    FillPlaceholdersInElement(footerPart.Footer, fieldValues);
            }

            doc.MainDocumentPart.Document.Save();
        }

        return stream.ToArray();
    }

    private void ReplaceTextInBody(Body body, string oldText, string newText)
    {
        foreach (var para in body.Descendants<Paragraph>())
        {
            var fullText = para.InnerText;
            if (!fullText.Contains(oldText)) continue;

            var runs = para.Elements<Run>().ToList();
            if (runs.Count == 0) continue;

            var combinedText = string.Concat(runs.Select(r => r.InnerText));
            var replaceIndex = combinedText.IndexOf(oldText, StringComparison.Ordinal);
            if (replaceIndex < 0) continue;

            var runProps = runs[0].RunProperties?.CloneNode(true) as RunProperties;

            foreach (var run in runs) run.Remove();

            var beforeText = combinedText.Substring(0, replaceIndex);
            var afterText = combinedText.Substring(replaceIndex + oldText.Length);

            if (!string.IsNullOrEmpty(beforeText))
            {
                var beforeRun = new Run();
                if (runProps != null) beforeRun.Append(runProps.CloneNode(true));
                beforeRun.Append(new Text(beforeText) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(beforeRun);
            }

            var placeholderRun = new Run();
            if (runProps != null) placeholderRun.Append(runProps.CloneNode(true));
            placeholderRun.Append(new Text(newText) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(placeholderRun);

            if (!string.IsNullOrEmpty(afterText))
            {
                var afterRun = new Run();
                if (runProps != null) afterRun.Append(runProps.CloneNode(true));
                afterRun.Append(new Text(afterText) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(afterRun);
            }
        }
    }

    private void FillPlaceholdersInBody(Body body, Dictionary<string, string> fieldValues)
    {
        var pattern = new Regex(@"\{\{(.+?)\}\}");

        foreach (var para in body.Descendants<Paragraph>())
        {
            FillPlaceholdersInParagraph(para, fieldValues, pattern);
        }

        foreach (var table in body.Descendants<Table>())
        {
            foreach (var row in table.Descendants<TableRow>())
            {
                foreach (var cell in row.Descendants<TableCell>())
                {
                    foreach (var para in cell.Descendants<Paragraph>())
                    {
                        FillPlaceholdersInParagraph(para, fieldValues, pattern);
                    }
                }
            }
        }
    }

    private void FillPlaceholdersInParagraph(Paragraph para, Dictionary<string, string> fieldValues, Regex pattern)
    {
        var runs = para.Elements<Run>().ToList();
        if (runs.Count == 0) return;

        var combinedText = string.Concat(runs.Select(r => r.InnerText));
        if (!pattern.IsMatch(combinedText)) return;

        var runProps = runs[0].RunProperties?.CloneNode(true) as RunProperties;
        foreach (var run in runs) run.Remove();

        var result = pattern.Replace(combinedText, match =>
        {
            var key = match.Groups[1].Value;
            return fieldValues.TryGetValue(key, out var val) ? val : match.Value;
        });

        var newRun = new Run();
        if (runProps != null) newRun.Append(runProps.CloneNode(true));
        newRun.Append(new Text(result) { Space = SpaceProcessingModeValues.Preserve });
        para.Append(newRun);
    }

    private void FillPlaceholdersInElement(OpenXmlCompositeElement element, Dictionary<string, string> fieldValues)
    {
        var pattern = new Regex(@"\{\{(.+?)\}\}");

        foreach (var para in element.Descendants<Paragraph>())
        {
            FillPlaceholdersInParagraph(para, fieldValues, pattern);
        }
    }

    private string GetSurroundingText(string text, int index, int length)
    {
        var start = Math.Max(0, index - 10);
        var end = Math.Min(text.Length, index + length + 10);
        var prefix = start > 0 ? "..." : "";
        var suffix = end < text.Length ? "..." : "";
        return $"{prefix}{text.Substring(start, end - start)}{suffix}";
    }

    private string GetTemplateFilePath(string fileName)
    {
        var path = Path.Combine(_templateRoot, fileName);
        if (File.Exists(path)) return path;

        path = Path.Combine(_docRoot, fileName);
        if (File.Exists(path)) return path;

        return Path.Combine(_templateRoot, fileName);
    }

    private string GenerateTemplateId(string name)
    {
        var sanitized = Regex.Replace(name, @"[^\w\u4e00-\u9fa5]", "-").ToLower();
        return $"{sanitized}-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private void UpdateManifest(ConfigureTemplateRequest request, string fileName, string templateId)
    {
        var manifestPath = Path.Combine(_templateRoot, "template-manifest.json");
        var manifest = new ManifestWrapper { Templates = new List<TemplateInfoDto>() };

        if (File.Exists(manifestPath))
        {
            var json = File.ReadAllText(manifestPath);
            manifest = System.Text.Json.JsonSerializer.Deserialize<ManifestWrapper>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? manifest;
        }

        var newTemplate = new TemplateInfoDto
        {
            Id = templateId,
            Name = request.TemplateName,
            FileName = fileName,
            Description = request.TemplateDescription,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Fields = request.Replacements.Select(r => new TemplateFieldDto
            {
                Name = r.FieldName,
                Label = r.FieldLabel,
                Required = true,
                Type = "text",
                Source = "manual"
            }).ToList()
        };

        manifest.Templates.Add(newTemplate);

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        var updatedJson = System.Text.Json.JsonSerializer.Serialize(manifest, options);
        File.WriteAllText(manifestPath, updatedJson, Encoding.UTF8);
    }

    private class ManifestWrapper
    {
        public List<TemplateInfoDto> Templates { get; set; } = new();
    }

    public bool DeleteTemplate(string templateId)
    {
        var manifestPath = Path.Combine(_templateRoot, "template-manifest.json");
        if (!File.Exists(manifestPath))
            return false;

        var json = File.ReadAllText(manifestPath);
        var manifest = System.Text.Json.JsonSerializer.Deserialize<ManifestWrapper>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (manifest?.Templates == null)
            return false;

        var tpl = manifest.Templates.FirstOrDefault(t => t.Id == templateId);
        if (tpl == null)
            return false;

        // 删除磁盘上的模板文件（先在 wwwroot/templates，再到 doc/模板文件）
        var filePath = GetTemplateFilePath(tpl.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        manifest.Templates.Remove(tpl);

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(manifestPath,
            System.Text.Json.JsonSerializer.Serialize(manifest, options), Encoding.UTF8);
        return true;
    }

    public (byte[]? Bytes, string FileName) GetTemplateFile(string templateId)
    {
        var tpl = GetTemplate(templateId);
        if (tpl == null)
            return (null, "");

        var filePath = GetTemplateFilePath(tpl.FileName);
        if (!File.Exists(filePath))
            return (null, $"{tpl.Name}.docx");

        return (File.ReadAllBytes(filePath), $"{tpl.Name}.docx");
    }
}
