using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using EnterpriseMS.Common;

namespace EnterpriseMS.Services.AI;

public class DocumentParser
{
    private readonly ILogger<DocumentParser> _logger;

    public DocumentParser(ILogger<DocumentParser> logger)
    {
        _logger = logger;
    }

    public string ParsePdf(Stream fileStream)
    {
        try
        {
            using var document = PdfDocument.Open(fileStream);
            var text = string.Join("\n", document.GetPages().Select(p => p.Text));
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PDF");
            throw new BusinessException("PDF文件解析失败");
        }
    }

    public string ParseWord(Stream fileStream)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(fileStream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            return body?.InnerText ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Word document");
            throw new BusinessException("Word文档解析失败");
        }
    }

    public string Parse(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".pdf" => ParsePdf(fileStream),
            ".docx" or ".doc" => ParseWord(fileStream),
            _ => throw new BusinessException($"不支持的文件格式: {extension}")
        };
    }
}
