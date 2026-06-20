namespace EnterpriseMS.Domain.Enums;

public enum BidProjectStatus
{
    Draft = 0,
    Analyzing = 1,
    Generating = 2,
    Reviewing = 3,
    Ready = 4,
    Submitted = 5,
    Won = 6,
    Lost = 7
}

public enum BidDocumentStatus
{
    Draft = 0,
    AiGenerated = 1,
    Editing = 2,
    Reviewed = 3,
    Finalized = 4
}

public enum BidChapterType
{
    Technical = 0,
    Commercial = 1,
    ProjectManagement = 2,
    Personnel = 3,
    QualityAssurance = 4,
    AfterSales = 5,
    Custom = 99
}
