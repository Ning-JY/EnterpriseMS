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
    public int MatchScore { get; set; }
}

public class MatchedCertificate
{
    public string CertName { get; set; } = "";
    public string? CertNo { get; set; }
    public string? IssueOrg { get; set; }
    public DateTime? ExpireDate { get; set; }
}
