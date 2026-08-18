using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Report;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.DTOs.Report;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniSoftware;

namespace EnterpriseMS.Services.Impl;

public interface IReportGeneratorService
{
    List<TemplateInfoDto> GetTemplates();
    TemplateInfoDto? GetTemplate(string templateId);
    List<TemplatePlaceholderDto> ScanPlaceholders(string templateId);
    string ConfigureTemplate(ConfigureTemplateRequest request, IFormFile templateFile);
    string FillTemplate(string templateId, Dictionary<string, object> fieldValues);
    byte[] GenerateDocument(string templateId, Dictionary<string, object> fieldValues);
    bool DeleteTemplate(string templateId);
    (byte[]? Bytes, string FileName) GetTemplateFile(string templateId);
    /// <summary>首次启动把现有 template-manifest.json 迁入模板表（幂等）。</summary>
    Task SeedFromManifestIfEmptyAsync();
    /// <summary>通用填充：按字段 Source 派发到对应 ITemplateDataSource 解析绑定值（提取自原 ProjectService，供填充向导与项目报告共用）。</summary>
    Task<Dictionary<string, string>> BuildReportFieldValuesAsync(string contextSource, string instanceId, TemplateInfoDto tpl, Dictionary<string, object>? manual);
    /// <summary>ad-hoc 渲染：标量 {{字段}} 文本替换 + 列表字段（值类型为 List&lt;Dictionary&gt;）表格行循环展开。
    /// 用于造价小工具等客户端 Excel 解析后把明细行生成到模板 Word（MiniWord 0.9.2 不支持 List&lt;Dictionary&gt; 行循环，故自行用 OpenXML 展开）。</summary>
    byte[] GenerateAdhocReport(string templateId, Dictionary<string, object> fieldValues);
}

public class ReportGeneratorService : IReportGeneratorService
{
    private readonly AppDbContext _db;
    private readonly string _templateRoot;
    private readonly string _docRoot;
    private readonly ILogger<ReportGeneratorService> _logger;
    private readonly IEnumerable<ITemplateDataSource> _sources;

    public ReportGeneratorService(AppDbContext db, IConfiguration config, ILogger<ReportGeneratorService> logger, IEnumerable<ITemplateDataSource> sources)
    {
        _db = db;
        _templateRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
        _docRoot = Path.Combine(Directory.GetCurrentDirectory(), "doc", "模板文件");
        _logger = logger;
        _sources = sources;

        if (!Directory.Exists(_templateRoot))
            Directory.CreateDirectory(_templateRoot);
    }

    private TemplateInfoDto ToDto(TemplateDefinition t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        FileName = t.FileName,
        Description = t.Description,
        CreatedAt = t.CreatedAt,
        ContextSource = t.ContextSource,
        Category = t.Category,
        Fields = t.Fields.OrderBy(f => f.Sort).Select(f => new TemplateFieldDto
        {
            Name = f.Name,
            Label = f.Label,
            Required = f.Required,
            Type = f.Type,
            Source = f.Source,
            Binding = f.Binding,
            ConfigKey = f.ConfigKey,
            DefaultValue = f.DefaultValue,
            HelpText = f.HelpText
        }).ToList()
    };

    public List<TemplateInfoDto> GetTemplates()
    {
        return _db.TemplateDefinitions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TemplateInfoDto
            {
                Id = t.Id,
                Name = t.Name,
                FileName = t.FileName,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                ContextSource = t.ContextSource,
                Category = t.Category,
                Fields = t.Fields.OrderBy(f => f.Sort).Select(f => new TemplateFieldDto
                {
                    Name = f.Name,
                    Label = f.Label,
                    Required = f.Required,
                    Type = f.Type,
                    Source = f.Source,
                    Binding = f.Binding,
                    ConfigKey = f.ConfigKey,
                    DefaultValue = f.DefaultValue,
                    HelpText = f.HelpText
                }).ToList()
            })
            .ToList();
    }

    public TemplateInfoDto? GetTemplate(string templateId)
    {
        var t = _db.TemplateDefinitions.Include(t => t.Fields).FirstOrDefault(t => t.Id == templateId);
        return t == null ? null : ToDto(t);
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

    public string ConfigureTemplate(ConfigureTemplateRequest request, IFormFile? templateFile)
    {
        var bindable = new[] { "project", "employee", "projcontract", "employeecontract" };

        // 编辑且未重新上传文件：仅更新元数据与字段，保留原有 docx
        if (!string.IsNullOrWhiteSpace(request.TemplateId) && (templateFile == null || templateFile.Length == 0))
        {
            var existing = _db.TemplateDefinitions.Include(t => t.Fields).FirstOrDefault(t => t.Id == request.TemplateId);
            if (existing != null)
            {
                existing.Name = request.TemplateName;
                existing.Description = request.TemplateDescription;
                existing.Category = request.Category;
                _db.TemplateFields.RemoveRange(existing.Fields);
                existing.Fields = BuildFields(request.TemplateId, request.Replacements);
                _db.SaveChanges();
                return existing.Id;
            }
        }

        if (templateFile == null || templateFile.Length == 0)
            throw new BusinessException("请上传模板文件");

        // 编辑时生成新 Id 并删除旧记录，保证列表无重复
        var templateId = string.IsNullOrWhiteSpace(request.TemplateId)
            ? GenerateTemplateId(request.TemplateName)
            : $"{request.TemplateId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
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

            doc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, stream.ToArray());

        var contextSource = request.Replacements
            .Select(r => r.FieldSource)
            .FirstOrDefault(s => bindable.Contains(s));

        var tpl = new TemplateDefinition
        {
            Id = templateId,
            Name = request.TemplateName,
            FileName = fileName,
            Description = request.TemplateDescription,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ContextSource = contextSource,
            Fields = BuildFields(templateId, request.Replacements)
        };

        if (!string.IsNullOrWhiteSpace(request.TemplateId))
        {
            var old = _db.TemplateDefinitions.Include(t => t.Fields).FirstOrDefault(t => t.Id == request.TemplateId);
            if (old != null)
            {
                var oldPath = GetTemplateFilePath(old.FileName);
                if (File.Exists(oldPath)) File.Delete(oldPath);
                _db.TemplateDefinitions.Remove(old);
            }
        }

        _db.TemplateDefinitions.Add(tpl);
        _db.SaveChanges();
        return templateId;
    }

    private List<TemplateField> BuildFields(string templateId, List<ReplacementItem> replacements) =>
        replacements.Select((r, i) => new TemplateField
        {
            TemplateId = templateId,
            Name = r.FieldName,
            Label = r.FieldLabel,
            Required = r.Required,
            Type = string.IsNullOrWhiteSpace(r.Type) ? "text" : r.Type,
            Source = string.IsNullOrWhiteSpace(r.FieldSource) ? "manual" : r.FieldSource,
            Binding = (r.FieldSource != "manual" && r.FieldSource != "config") ? r.Binding : null,
            ConfigKey = r.FieldSource == "config" ? r.ConfigKey : null,
            DefaultValue = r.DefaultValue,
            HelpText = r.HelpText,
            Sort = i
        }).ToList();

    /// <summary>通用填充：按字段 Source 派发到对应 ITemplateDataSource 解析绑定值。</summary>
    public async Task<Dictionary<string, string>> BuildReportFieldValuesAsync(
        string contextSource, string instanceId, TemplateInfoDto tpl, Dictionary<string, object>? manual)
    {
        var values = new Dictionary<string, string>();
        var providerMap = _sources.ToDictionary(p => p.SourceId, p => p);

        foreach (var f in tpl.Fields)
        {
            if (f.Source == "manual")
            {
                values[f.Name] = (manual != null && manual.TryGetValue(f.Name, out var mv) && !string.IsNullOrWhiteSpace(mv?.ToString()))
                    ? mv!.ToString()!
                    : (f.DefaultValue ?? "");
            }
            else if (providerMap.TryGetValue(f.Source, out var provider))
            {
                var resolved = await provider.ResolveAsync(instanceId);
                var key = f.Source == "config" ? f.ConfigKey : f.Binding;
                values[f.Name] = (key != null && resolved.TryGetValue(key, out var v)) ? v : "";
            }
            else
            {
                values[f.Name] = "";
            }
        }

        // 业务计算字段（仅当模板含送审/审定金额时生效）
        if (values.TryGetValue("送审金额", out var s) && values.TryGetValue("审定金额", out var d)
            && decimal.TryParse(s, out var sd) && decimal.TryParse(d, out var dd))
        {
            var diff = sd - dd;
            values["审减金额"] = diff.ToString("F2");
            values["审减率"] = (sd != 0 ? diff / sd * 100 : 0).ToString("F2") + "%";
        }

        return values;
    }

    public string FillTemplate(string templateId, Dictionary<string, object> fieldValues)
    {
        var bytes = RenderWithMiniWord(templateId, fieldValues);
        return Convert.ToBase64String(bytes);
    }

    public byte[] GenerateDocument(string templateId, Dictionary<string, object> fieldValues)
    {
        var bytes = RenderWithMiniWord(templateId, fieldValues);
        // 兜底：MiniWord 对 VML 艺术字(textpath)/跨 run 多实例等场景可能漏替换，
        // 此时用更鲁棒的 OpenXML 渲染器（GenerateAdhocReport）补全，保证零残留占位符。
        if (ContainsPlaceholder(bytes))
        {
            try { return GenerateAdhocReport(templateId, fieldValues); }
            catch { /* 渲染失败则保留 MiniWord 结果 */ }
        }
        return bytes;
    }

    /// <summary>判断渲染结果是否仍残留未替换的 {{字段}}（含正文段落与 VML 艺术字）。</summary>
    private static bool ContainsPlaceholder(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            using var doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return false;
            foreach (var p in body.Descendants<Paragraph>())
                if (p.InnerText.Contains("{{")) return true;
            foreach (var tp in body.Descendants<DocumentFormat.OpenXml.Vml.TextPath>())
                if ((tp.String?.Value ?? "").Contains("{{")) return true;
            return false;
        }
        catch { return false; }
    }

    /// <summary>用 MiniWord 按模板渲染：支持 {{字段}} 文本、表格行循环(List&lt;Dictionary&gt;)、图片(MiniWordPicture)。</summary>
    private byte[] RenderWithMiniWord(string templateId, Dictionary<string, object> fieldValues)
    {
        var template = GetTemplate(templateId) ?? throw new BusinessException("模板不存在");
        var filePath = GetTemplateFilePath(template.FileName);
        if (!File.Exists(filePath))
            throw new BusinessException($"模板文件不存在: {template.FileName}");

        var templateBytes = File.ReadAllBytes(filePath);
        using var outStream = new MemoryStream();
        MiniWord.SaveAsByTemplate(outStream, templateBytes, fieldValues ?? new Dictionary<string, object>());
        return outStream.ToArray();
    }

    /// <summary>
    /// ad-hoc 渲染：标量 {{字段}} 文本替换 + 列表字段（List&lt;Dictionary&gt;）表格行循环。
    /// MiniWord 0.9.2 不支持 List&lt;Dictionary&gt; 自动行循环，这里用 OpenXML 自行展开，保证造价小工具等
    /// 多明细行报告可用同一套 docx 模板（布局可自定义）。
    /// </summary>
    public byte[] GenerateAdhocReport(string templateId, Dictionary<string, object> fieldValues)
    {
        var template = GetTemplate(templateId) ?? throw new BusinessException("模板不存在");
        var filePath = GetTemplateFilePath(template.FileName);
        if (!File.Exists(filePath))
            throw new BusinessException($"模板文件不存在: {template.FileName}");

        var bytes = File.ReadAllBytes(filePath);
        using var ms = new MemoryStream();
        ms.Write(bytes, 0, bytes.Length);
        ms.Position = 0;
        using (var doc = WordprocessingDocument.Open(ms, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body
                       ?? throw new BusinessException("模板内容为空");

            // 区分标量值与列表值
            var scalars = fieldValues
                .Where(kv => !IsListValue(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
            var lists = fieldValues
                .Where(kv => IsListValue(kv.Value))
                .ToDictionary(kv => kv.Key, kv => (System.Collections.IEnumerable)kv.Value);

            // 1) 表格：含列表标记的行做行循环；其余行做标量替换
            foreach (var table in body.Descendants<Table>().ToList())
            {
                foreach (var row in table.Elements<TableRow>().ToList())
                {
                    var rowText = string.Concat(row.Descendants<Paragraph>().Select(p => p.InnerText));
                    var marker = lists.Keys.FirstOrDefault(k => rowText.Contains("{{" + k + "}}"));
                    if (marker != null)
                    {
                        var clones = new List<TableRow>();
                        foreach (var item in lists[marker])
                        {
                            var itemDict = ToStringDict(item);
                            var clone = (TableRow)row.CloneNode(true);
                            FillRowScalars(clone, itemDict, scalars, marker);
                            clones.Add(clone);
                        }
                        foreach (var nr in clones) row.Parent!.InsertBefore(nr, row);
                        row.Remove();
                    }
                    else
                    {
                        FillRowScalars(row, null, scalars, null);
                    }
                }
            }

            // 2) 正文段落标量替换（含表格单元格内已处理段落之外的段落）
            foreach (var para in body.Descendants<Paragraph>().ToList())
            {
                FillParagraphScalars(para, scalars);
            }

            // 3) VML 艺术字/形状：文本在 <v:textpath string="..."> 属性中，普通段落遍历覆盖不到，单独处理
            foreach (var textPath in body.Descendants<DocumentFormat.OpenXml.Vml.TextPath>().ToList())
            {
                var s = textPath.String?.Value;
                if (string.IsNullOrEmpty(s)) continue;
                bool changed = false;
                foreach (var kv in scalars)
                    if (s.Contains("{{" + kv.Key + "}}")) { s = s.Replace("{{" + kv.Key + "}}", kv.Value); changed = true; }
                if (changed) textPath.String = s;
            }

            doc.MainDocumentPart!.Document.Save();
        }
        return ms.ToArray();
    }

    private static bool IsListValue(object? v) =>
        v is System.Collections.IEnumerable && v is not string;

    private static Dictionary<string, string> ToStringDict(object item)
    {
        var d = new Dictionary<string, string>();
        if (item is System.Collections.Generic.IDictionary<string, object> d1)
            foreach (var kv in d1) d[kv.Key] = kv.Value?.ToString() ?? "";
        else if (item is System.Collections.Generic.IDictionary<string, string> d2)
            foreach (var kv in d2) d[kv.Key] = kv.Value ?? "";
        return d;
    }

    private static void FillRowScalars(TableRow row, Dictionary<string, string>? itemDict, Dictionary<string, string> scalars, string? markerKey)
    {
        foreach (var para in row.Descendants<Paragraph>().ToList())
        {
            var text = para.InnerText;
            bool changed = false;
            // 清除循环标记（{{明细}} 等），避免残留
            if (markerKey != null && text.Contains("{{" + markerKey + "}}"))
            {
                text = text.Replace("{{" + markerKey + "}}", "");
                changed = true;
            }
            if (itemDict != null)
            {
                foreach (var kv in itemDict)
                    if (text.Contains("{{" + kv.Key + "}}")) { text = text.Replace("{{" + kv.Key + "}}", kv.Value); changed = true; }
            }
            foreach (var kv in scalars)
                if (text.Contains("{{" + kv.Key + "}}")) { text = text.Replace("{{" + kv.Key + "}}", kv.Value); changed = true; }
            if (changed) RebuildParagraph(para, text);
        }
    }

    private static void FillParagraphScalars(Paragraph para, Dictionary<string, string> scalars)
    {
        var text = para.InnerText;
        bool changed = false;
        foreach (var kv in scalars)
            if (text.Contains("{{" + kv.Key + "}}")) { text = text.Replace("{{" + kv.Key + "}}", kv.Value); changed = true; }
        if (changed) RebuildParagraph(para, text);
    }

    private static void RebuildParagraph(Paragraph para, string text)
    {
        var runProps = para.Elements<Run>().FirstOrDefault()?.RunProperties?.CloneNode(true);
        para.Elements<Run>().ToList().ForEach(r => r.Remove());
        // 按换行拆分，换行处插入 <w:br/>，保留多行文本（如审核明细、地址段）
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) para.Append(new Run(new Break()));
            var run = new Run();
            if (runProps != null) run.Append((RunProperties)runProps.CloneNode(true));
            run.Append(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
        }
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

    public bool DeleteTemplate(string templateId)
    {
        var tpl = _db.TemplateDefinitions.Include(t => t.Fields).FirstOrDefault(t => t.Id == templateId);
        if (tpl == null)
            return false;

        var filePath = GetTemplateFilePath(tpl.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        _db.TemplateDefinitions.Remove(tpl);
        _db.SaveChanges();
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

    public async Task SeedFromManifestIfEmptyAsync()
    {
        if (await _db.TemplateDefinitions.AnyAsync())
            return;

        var manifestPath = Path.Combine(_templateRoot, "template-manifest.json");
        if (!File.Exists(manifestPath))
            return;

        var json = await File.ReadAllTextAsync(manifestPath);
        var manifest = JsonSerializer.Deserialize<ManifestWrapper>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (manifest?.Templates == null)
            return;

        var bindable = new[] { "project", "employee", "projcontract", "employeecontract" };

        foreach (var t in manifest.Templates)
        {
            var def = new TemplateDefinition
            {
                Id = t.Id,
                Name = t.Name,
                FileName = t.FileName,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                ContextSource = t.Fields?
                    .Select(f => f.Source)
                    .FirstOrDefault(s => bindable.Contains(s)),
                Fields = (t.Fields ?? new List<TemplateFieldDto>()).Select((f, i) => new TemplateField
                {
                    TemplateId = t.Id,
                    Name = f.Name,
                    Label = f.Label,
                    Required = f.Required,
                    Type = f.Type,
                    Source = f.Source,
                    Binding = f.Binding,
                    ConfigKey = f.ConfigKey,
                    DefaultValue = f.DefaultValue,
                    HelpText = f.HelpText,
                    Sort = i
                }).ToList()
            };
            _db.TemplateDefinitions.Add(def);
        }

        await _db.SaveChangesAsync();
    }

    private class ManifestWrapper
    {
        public List<TemplateInfoDto> Templates { get; set; } = new();
    }
}
