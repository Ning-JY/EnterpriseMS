namespace EnterpriseMS.Services.DTOs.Bid;

public class PersonnelMatchRequest
{
    public long BidProjectId { get; set; }
    public int MaxPersonnel { get; set; } = 10;
}

public class PersonnelMatchResult
{
    public List<MatchedPersonnel> MatchedPersonnel { get; set; } = new();
    public List<string> UnmatchedRequirements { get; set; } = new();
    /// <summary>系统未能从要求文本中识别出已知证书类型，无法用规则自动判断是否满足，
    /// 必须由人工直接核对候选人简历——不应被悄悄忽略，也不应靠模糊关键词强行匹配。</summary>
    public List<string> UnrecognizedRequirements { get; set; } = new();
    public string Summary { get; set; } = "";
}

public class MatchedPersonnel
{
    public long EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string? DeptName { get; set; }
    public string? PostName { get; set; }
    public string? Education { get; set; }
    public int WorkYears { get; set; }
    public List<MatchedCertificate> Certificates { get; set; } = new();
    public List<string> MatchedRequirements { get; set; } = new();
    /// <summary>逐条对应 MatchedRequirements 的判定依据（用的哪张证书、有效期到什么时候），
    /// 保证结论可追溯到具体规则，而不是一个不可解释的分数。</summary>
    public List<string> MatchBasis { get; set; } = new();
    public int MatchScore { get; set; }
    /// <summary>在建项目关键岗位重叠提示——这是"社保唯一性冲突"的代理信号，
    /// 系统没有真实社保数据可查验，命中后必须交由人工和HR确认，不能据此直接采用或直接排除候选人。</summary>
    public List<string> ConflictWarnings { get; set; } = new();
    public bool HasConflict => ConflictWarnings.Count > 0;
}

public class MatchedCertificate
{
    public string CertName { get; set; } = "";
    public string? CertNo { get; set; }
    public string? IssueOrg { get; set; }
    public DateTime? ExpireDate { get; set; }
    /// <summary>Valid 有效 / ExpiringSoon 投标截止日前到期 / Expired 已过期 / Unknown 未登记有效期。
    /// 由 ExpireDate 与投标截止日的确定性比较得出，不经过AI判断。</summary>
    public string ValidityStatus { get; set; } = "Valid";
}
