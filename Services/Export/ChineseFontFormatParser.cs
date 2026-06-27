namespace EnterpriseMS.Services.Export;

/// <summary>
/// 把模块一AI解析出的格式要求（自由文本，例如"宋体小四"）解析成实际可用的字体名+磅值。
/// 这里特意做成"尽力而为、解析不出就回退默认值"，因为来源是AI抽取的自由文本，
/// 格式千变万化（"小四号宋体" vs "宋体，小四"等），不值得为了完美覆盖所有写法而过度设计正则，
/// 解析失败时静默回退比抛异常更合适——一份格式稍有偏差的Word文档，好过导出直接失败。
/// </summary>
public static class ChineseFontFormatParser
{
    private const string DefaultFontName = "宋体";
    private const int DefaultHalfPointSize = 24; // 小四 = 12pt = 24 half-points

    private static readonly Dictionary<string, int> SizeNameToHalfPoints = new()
    {
        ["八号"] = 10, ["七号"] = 11, ["小六"] = 13, ["六号"] = 15,
        ["小五"] = 18, ["五号"] = 21, ["小四"] = 24, ["四号"] = 28,
        ["小三"] = 30, ["三号"] = 32, ["小二"] = 36, ["二号"] = 44,
        ["小一"] = 48, ["一号"] = 52, ["小初"] = 72, ["初号"] = 84,
    };

    private static readonly string[] KnownFontNames =
    {
        "仿宋_GB2312", "仿宋", "黑体", "楷体_GB2312", "楷体", "宋体", "微软雅黑", "新宋体", "华文中宋", "Times New Roman", "Arial"
    };

    public static (string FontName, int HalfPointSize) Parse(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (DefaultFontName, DefaultHalfPointSize);

        var fontName = KnownFontNames.FirstOrDefault(f => description.Contains(f)) ?? DefaultFontName;
        var sizeEntry = SizeNameToHalfPoints.FirstOrDefault(kv => description.Contains(kv.Key));
        var size = sizeEntry.Key != null ? sizeEntry.Value : DefaultHalfPointSize;

        return (fontName, size);
    }
}
