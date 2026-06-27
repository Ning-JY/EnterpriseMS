namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 证书/职称类型对照表，用于把招标要求文本中提到的证书类型，精确匹配到员工证书库里的记录，
/// 取代"任意关键词在两边都出现就算匹配"的模糊判断。
///
/// 设计原因：原实现里 certKeywords/reqKeywords 是两组宽泛的通用词（"工程师""注册"等），
/// 只要证书名称和要求文本里各出现一个就判定匹配，会把"注册电气工程师"误判为满足"注册造价工程师"的要求——
/// 两者只是都含"注册""工程师"，证书类型完全不同。人员资格匹配错误可能直接导致废标，
/// 所以这里改成"两边必须命中同一个canonical证书类型"，且新增证书类型只需要在表里加一行，不需要改匹配代码。
/// </summary>
public static class CertificateTaxonomy
{
    public static readonly Dictionary<string, string[]> KnownCertificates = new()
    {
        ["一级建造师"] = new[] { "一级建造师", "建造师(一级)", "建造师（一级）", "一建" },
        ["二级建造师"] = new[] { "二级建造师", "建造师(二级)", "建造师（二级）", "二建" },
        ["注册建筑师"] = new[] { "注册建筑师", "一级注册建筑师", "二级注册建筑师" },
        ["注册结构工程师"] = new[] { "注册结构工程师", "结构工程师" },
        ["监理工程师"] = new[] { "监理工程师", "注册监理工程师" },
        ["造价工程师"] = new[] { "造价工程师", "注册造价工程师", "造价师" },
        ["咨询工程师"] = new[] { "咨询工程师", "注册咨询工程师" },
        ["城市规划师"] = new[] { "城市规划师", "注册城市规划师", "注册规划师" },
        ["岩土工程师"] = new[] { "岩土工程师", "注册岩土工程师" },
        ["安全工程师"] = new[] { "安全工程师", "注册安全工程师" },
        ["PMP项目管理"] = new[] { "PMP", "项目管理专业人士" },
        ["高级项目管理师"] = new[] { "高级项目管理师" },
        ["高级工程师"] = new[] { "高级工程师", "高工" },
        ["中级工程师"] = new[] { "中级工程师", "中级职称" },
        ["建筑工程师"] = new[] { "建筑工程师" },
        ["电气工程师"] = new[] { "电气工程师", "注册电气工程师" },
        ["给排水工程师"] = new[] { "给排水工程师", "注册公用设备工程师" },
    };

    /// <summary>
    /// 识别要求文本中提到了哪些已知证书类型。
    /// 识别不到任何已知类型时返回空列表——调用方应将该要求标记为"系统无法自动识别，需人工核对"，
    /// 而不是放宽规则去模糊匹配，宁可暴露给人工，也不要给出一个看似合理但可能错误的自动判断。
    /// </summary>
    public static List<string> ExtractRequiredCertTypes(string requirementText)
    {
        if (string.IsNullOrWhiteSpace(requirementText)) return new List<string>();
        var found = new List<string>();
        foreach (var (canonical, aliases) in KnownCertificates)
        {
            if (aliases.Any(a => requirementText.Contains(a, StringComparison.OrdinalIgnoreCase)))
                found.Add(canonical);
        }
        return found;
    }

    /// <summary>判断某条员工证书记录是否属于指定的canonical证书类型（按别名精确匹配，不做宽泛关键词重叠）。</summary>
    public static bool CertMatchesType(string certName, string canonicalType)
    {
        if (string.IsNullOrWhiteSpace(certName)) return false;
        if (!KnownCertificates.TryGetValue(canonicalType, out var aliases)) return false;
        return aliases.Any(a => certName.Contains(a, StringComparison.OrdinalIgnoreCase));
    }
}
