namespace EnterpriseMS.Services.DTOs.Project;

public class ProjectListDto
{
    public long     Id              { get; set; }
    public string   ProjNo          { get; set; } = "";
    public string   ProjName        { get; set; } = "";
    public string?  DeptName        { get; set; }
    public string   BizType         { get; set; } = "";
    public string   OwnerName       { get; set; } = "";
    public string?  OwnerPhone      { get; set; }
    public string?  OwnerContact    { get; set; }
    public string?  ProcurementType { get; set; }
    public decimal  ContractAmount  { get; set; }
    public bool     IsJointVenture  { get; set; }
    public decimal? OurRatio        { get; set; }
    public string?  ProjectLeaderName { get; set; }
    public DateTime? SignDate       { get; set; }
    public DateTime? PlanEndDate    { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public int      ProgressStatus  { get; set; }
    public string   ProgressText    { get; set; } = "";
    public decimal  ActualAmount    { get; set; }
    public int      MilestoneDone   { get; set; }
    public int      MilestoneTotal  { get; set; }
    public DateTime? StatusUpdatedAt { get; set; }
}

/// <summary>投标建项时用于按名称选择关联项目的精简条目。</summary>
public class ProjectSelectItemDto
{
    public long   Id      { get; set; }
    public string ProjNo  { get; set; } = "";
    public string ProjName { get; set; } = "";
}

public class ProjectDetailDto : ProjectListDto
{
    public long?    DeptId          { get; set; }
    public long?    ProjectLeaderId { get; set; }
    public string?  ProjectCategory { get; set; }
    public string?  ProjectOverview { get; set; }
    public decimal? LimitPrice      { get; set; }
    public string?  BuildingScale   { get; set; }
    public string?  CooperationUnit { get; set; }
    public DateTime? ActualEndDate  { get; set; }
    public decimal  TotalReceived   { get; set; }
    public string?  Remark          { get; set; }
    public List<ProjectMemberDto>     Members     { get; set; } = new();
    public List<ProjectMilestoneDto>  Milestones  { get; set; } = new();
    public List<ProjectAcceptanceDto> Acceptances { get; set; } = new();
    public List<ProjectLogDto>        Logs        { get; set; } = new();
    public List<ProjectContractDto>   Contracts   { get; set; } = new();
    public List<ProjectInvoiceDto>    Invoices    { get; set; } = new();
    public List<ProjectFileDto>       Files       { get; set; } = new();
}

public class ProjectMemberDto
{
    public long    Id           { get; set; }
    public long    EmployeeId   { get; set; }
    public string  EmployeeName { get; set; } = "";
    public string? DeptName     { get; set; }
    public string  Role         { get; set; } = "";
    public string? DutyDesc     { get; set; }
    public decimal Ratio        { get; set; }
    public DateTime JoinDate    { get; set; }
    public int     Status       { get; set; }
    // 计算产值（运行时，不存库）
    public decimal ContractValue { get; set; }
    public decimal ReceivedValue { get; set; }
}

public class ProjectMilestoneDto
{
    public long     Id            { get; set; }
    public string   MilestoneName { get; set; } = "";
    public string?  MilestoneType { get; set; }
    public DateTime PlanDate      { get; set; }
    public DateTime? ActualDate   { get; set; }
    public string?  OwnerName     { get; set; }
    public decimal? AcceptAmount  { get; set; }
    public int      Status        { get; set; }
    public bool     IsOverdue     { get; set; }
    public string?  Remark        { get; set; }
    public int      Sort          { get; set; }
}

public class ProjectAcceptanceDto
{
    public long     Id           { get; set; }
    public string   AcceptBatch  { get; set; } = "";
    public DateTime AcceptDate   { get; set; }
    public decimal  AcceptAmount { get; set; }
    public string?  InvoiceNo    { get; set; }
    public string?  Remark       { get; set; }
}

public class ProjectLogDto
{
    public long     Id        { get; set; }
    public string   Title     { get; set; } = "";
    public string?  Content   { get; set; }
    public string   OperBy    { get; set; } = "";
    public DateTime OperAt    { get; set; }
}

public class ProjectContractDto
{
    public long     Id           { get; set; }
    public string   ContractNo   { get; set; } = "";
    public string   ContractType { get; set; } = "";
    public string?  ContractName { get; set; }
    public string   PartyA       { get; set; } = "";
    public string   PartyB       { get; set; } = "";
    public decimal  Amount       { get; set; }
    public DateTime? SignDate    { get; set; }
    public DateTime? StartDate   { get; set; }
    public DateTime? EndDate     { get; set; }
    public string?  FilePath     { get; set; }
    public string?  FileName     { get; set; }
    public int      Status       { get; set; }
    public string?  Remark       { get; set; }
}

public class ProjectInvoiceDto
{
    public long     Id              { get; set; }
    public long?    ContractId      { get; set; }
    public string   ReceiptName     { get; set; } = "";
    public string?  InvoiceNo       { get; set; }
    public string   InvoiceType     { get; set; } = "";
    public decimal  Amount          { get; set; }
    public decimal? TaxRate         { get; set; }
    public DateTime? InvoiceDate    { get; set; }
    public string?  Payer           { get; set; }
    public bool     IsReceived      { get; set; }
    public DateTime? ReceivedDate   { get; set; }
    public string?  InvoiceFile     { get; set; }
    public string?  InvoiceFileName { get; set; }
    public string?  PaymentFile     { get; set; }
    public string?  PaymentFileName { get; set; }
    public string?  Remark          { get; set; }
    public DateTime CreatedAt       { get; set; }
}

public class ProjectFileDto
{
    public long    Id           { get; set; }
    public string  FileCategory { get; set; } = "";
    public string  FileName     { get; set; } = "";
    public string  FilePath     { get; set; } = "";
    public long    FileSize     { get; set; }
    public string? FileExt      { get; set; }
    public string? Description  { get; set; }
    public string? Version      { get; set; }
    public string  UploadBy     { get; set; } = "";
    public DateTime CreatedAt   { get; set; }
    public string FileSizeText  => FileSize < 1024 ? $"{FileSize}B"
                                 : FileSize < 1048576 ? $"{FileSize/1024:N1}KB"
                                 : $"{FileSize/1048576:N1}MB";
}

// ── 操作 DTO ─────────────────────────────────────────────────
public class ProjectQueryDto
{
    public string? Keyword        { get; set; }
    public long?   DeptId         { get; set; }
    public int?    ProgressStatus { get; set; }
    public string? BizType        { get; set; }
    public int     Page           { get; set; } = 1;
    public int     Size           { get; set; } = 15;
}

public class CreateProjectDto
{
    public string   ProjNo           { get; set; } = "";
    public string   ProjName         { get; set; } = "";
    public long?    DeptId           { get; set; }
    public string   BizType          { get; set; } = "";
    public string   OwnerName        { get; set; } = "";
    public string?  OwnerContact     { get; set; }
    public string?  OwnerPhone       { get; set; }
    public string?  ProcurementType  { get; set; }
    public decimal? LimitPrice       { get; set; }
    public decimal  ContractAmount   { get; set; }
    public bool     IsJointVenture   { get; set; }
    public decimal? OurRatio         { get; set; }
    public DateTime? SignDate        { get; set; }
    public DateTime? PlanEndDate     { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? BidDeadline     { get; set; }
    public string?  BuildingScale    { get; set; }
    public string?  CooperationUnit  { get; set; }
    public string?  Remark           { get; set; }
    // 新增字段
    public string?  ProjectCategory  { get; set; }
    public long?    ProjectLeaderId  { get; set; }
    public string?  ProjectOverview  { get; set; }
    public bool     ContractSigned   { get; set; }
    public DateTime? ContractSignedDate { get; set; }
    public int      ProgressStatus  { get; set; } // 进度状态（原遗漏，需随表单保存）
    public List<CreateMilestoneDto> Milestones { get; set; } = new();
    public List<CreateMemberDto>    Members    { get; set; } = new();
}

/// <summary>一键转项目：仅需项目名称即可快速建项，其余字段留空后续补全。</summary>
public class QuickCreateProjectDto
{
    public string ProjName { get; set; } = "";
}

public class UpdateProjectDto : CreateProjectDto { public long Id { get; set; } }

public class ChangeStatusDto
{
    public long     Id         { get; set; }
    //public long ProjectId  { get; set; }
    public int      NewStatus  { get; set; }
    public DateTime? StatusDate { get; set; }
    public string?  Remark     { get; set; }
}

public class CreateMemberDto
{
    public long     EmployeeId { get; set; }
    public string   Role       { get; set; } = "参与人员";
    public string?  DutyDesc   { get; set; }
    public decimal  Ratio      { get; set; }
    public DateTime JoinDate   { get; set; } = DateTime.UtcNow.Date;
}

public class UpdateMemberDto
{
    public long    Id       { get; set; }
    public string  Role     { get; set; } = "";
    public string? DutyDesc { get; set; }
    public decimal Ratio    { get; set; }
}

public class CreateMilestoneDto
{
    public string   MilestoneName { get; set; } = "";
    public string?  MilestoneType { get; set; }
    public DateTime PlanDate      { get; set; }
    public long?    OwnerId       { get; set; }
    public decimal? AcceptAmount  { get; set; }
    public string?  Remark        { get; set; }
    public int      Sort          { get; set; }
}

public class UpdateMilestoneDto : CreateMilestoneDto { public long Id { get; set; } }

public class CreateAcceptanceDto
{
    public long     ProjectId    { get; set; }
    public string   AcceptBatch  { get; set; } = "";
    public DateTime AcceptDate   { get; set; }
    public decimal  AcceptAmount { get; set; }
    public string?  InvoiceNo    { get; set; }
    public string?  Remark       { get; set; }
}

public class CreateContractDto
{
    public long     ProjectId    { get; set; }
    public string   ContractNo   { get; set; } = "";
    public string   ContractType { get; set; } = "主合同";
    public string?  ContractName { get; set; }
    public string   PartyA       { get; set; } = "";
    public string   PartyB       { get; set; } = "";
    public decimal  Amount       { get; set; }
    public DateTime? SignDate    { get; set; }
    public DateTime? StartDate   { get; set; }
    public DateTime? EndDate     { get; set; }
    public string?  Remark       { get; set; }
}

public class CreateInvoiceDto
{
    public long     ProjectId    { get; set; }
    public long?    ContractId   { get; set; }
    public string   ReceiptName  { get; set; } = "";
    public string?  InvoiceNo    { get; set; }
    public string   InvoiceType  { get; set; } = "增值税专用发票";
    public decimal  Amount       { get; set; }
    public decimal? TaxRate      { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string?  Payer        { get; set; }
    public string?  Remark       { get; set; }
}

/// <summary>回款管理独立页列表 DTO（聚合项目名 / 合同名）</summary>
public class ReceiptListDto
{
    public long      Id           { get; set; }
    public long      ProjectId    { get; set; }
    public long?     ContractId   { get; set; }
    public string    ProjectName  { get; set; } = "";
    public string?   ContractName { get; set; }
    public string    ReceiptName  { get; set; } = "";
    public string?   InvoiceNo    { get; set; }
    public string    InvoiceType  { get; set; } = "";
    public decimal   Amount       { get; set; }
    public decimal?  TaxRate      { get; set; }
    public DateTime? InvoiceDate  { get; set; }
    public string?   Payer        { get; set; }
    public bool      IsReceived   { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string?   Remark       { get; set; }
    public DateTime  CreatedAt    { get; set; }
}

/// <summary>合同维度回款汇总（回款管理列表用，以合同为主）</summary>
public class ContractReceiptSummaryDto
{
    public long    ContractId       { get; set; }
    public long    ProjectId        { get; set; }
    public string  ProjectName      { get; set; } = "";
    public string  ContractName     { get; set; } = ""; // 合同名称（无名称则回退编号）
    public string  ContractNo       { get; set; } = "";
    public string  ContractType     { get; set; } = ""; // 主合同/补充协议/变更合同
    public decimal ContractAmount   { get; set; } // 合同总额
    public decimal ReceiptTotal     { get; set; } // 回款总额（所有回款记录金额合计）
    public decimal ReceivedTotal    { get; set; } // 已收金额
    public decimal UnreceivedTotal  { get; set; } // 未收金额 = 回款总额 - 已收金额
    public int     ReceiptCount     { get; set; } // 回款笔数
}

// ── #14 新增：编辑验收记录 DTO ─────────────────────────────
public class UpdateAcceptanceDto
{
    public long     Id           { get; set; }
    public string   AcceptBatch  { get; set; } = "";
    public DateTime AcceptDate   { get; set; }
    public decimal  AcceptAmount { get; set; }
    public string?  InvoiceNo    { get; set; }
    public string?  Remark       { get; set; }
}

// ── #13 新增：编辑合同 DTO ─────────────────────────────────
public class UpdateContractDto : CreateContractDto { public long Id { get; set; } }
