using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;

namespace EnterpriseMS.Domain.Entities.Project;

[Table("proj_project")]
public class Project : BaseEntity
{
    [Column("proj_no")]           public string   ProjNo          { get; set; } = "";
    [Column("proj_name")]         public string   ProjName        { get; set; } = "";
    [Column("dept_id")]           public long?    DeptId          { get; set; }
    [Column("biz_type")]          public string   BizType         { get; set; } = "";
    [Column("owner_name")]        public string   OwnerName       { get; set; } = "";
    [Column("owner_contact")]     public string?  OwnerContact    { get; set; }
    [Column("owner_phone")]       public string?  OwnerPhone      { get; set; }
    [Column("procurement_type")]  public string?  ProcurementType { get; set; }
    [Column("limit_price")]       public decimal? LimitPrice      { get; set; }
    [Column("contract_amount")]   public decimal  ContractAmount  { get; set; }
    [Column("is_joint_venture")]  public bool     IsJointVenture  { get; set; }
    [Column("our_ratio")]         public decimal? OurRatio        { get; set; }
    [Column("tech_leader_id")]    public long?    TechLeaderId    { get; set; }
    [Column("biz_leader_id")]     public long?    BizLeaderId     { get; set; }
    [Column("sign_date")]         public DateTime? SignDate        { get; set; }
    [Column("plan_end_date")]     public DateTime? PlanEndDate     { get; set; }
    [Column("actual_end_date")]   public DateTime? ActualEndDate   { get; set; }
    [Column("bid_deadline")]      public DateTime? BidDeadline     { get; set; }
    [Column("progress_status")]   public int      ProgressStatus  { get; set; } = 0;
    [Column("status_updated_at")] public DateTime? StatusUpdatedAt { get; set; } // 最后状态变更时间
    [Column("building_scale")]    public string?  BuildingScale   { get; set; }
    [Column("remark")]            public string?  Remark          { get; set; }

    public SysDept?  Dept       { get; set; }
    public Employee? TechLeader { get; set; }
    public Employee? BizLeader  { get; set; }
    public ICollection<ProjectMember>     Members     { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectMilestone>  Milestones  { get; set; } = new List<ProjectMilestone>();
    public ICollection<ProjectAcceptance> Acceptances { get; set; } = new List<ProjectAcceptance>();
    public ICollection<ProjectOperLog>    OperLogs    { get; set; } = new List<ProjectOperLog>();
    public ICollection<ProjectContract>   Contracts   { get; set; } = new List<ProjectContract>();
    public ICollection<ProjectInvoice>    Invoices    { get; set; } = new List<ProjectInvoice>();
    public ICollection<ProjectFile>       Files       { get; set; } = new List<ProjectFile>();

    [NotMapped] public decimal ActualContractAmount =>
        IsJointVenture && OurRatio.HasValue ? ContractAmount * OurRatio.Value / 100 : ContractAmount;
}

[Table("proj_member")]
public class ProjectMember : BaseEntity
{
    [Column("project_id")]  public long     ProjectId  { get; set; }
    [Column("employee_id")] public long     EmployeeId { get; set; }
    [Column("role")]        public string   Role       { get; set; } = "参与人员";
    [Column("duty_desc")]   public string?  DutyDesc   { get; set; }
    [Column("ratio")]       public decimal  Ratio      { get; set; }
    [Column("join_date")]   public DateTime JoinDate   { get; set; }
    [Column("leave_date")]  public DateTime? LeaveDate { get; set; }
    [Column("status")]      public int      Status     { get; set; } = 0; // 0参与中 1已退出
    public Project?  Project  { get; set; }
    public Employee? Employee { get; set; }
}

[Table("proj_milestone")]
public class ProjectMilestone : BaseEntity
{
    [Column("project_id")]     public long     ProjectId     { get; set; }
    [Column("milestone_name")] public string   MilestoneName { get; set; } = "";
    [Column("milestone_type")] public string?  MilestoneType { get; set; }
    [Column("plan_date")]      public DateTime PlanDate      { get; set; }
    [Column("actual_date")]    public DateTime? ActualDate   { get; set; }
    [Column("owner_id")]       public long?    OwnerId       { get; set; }
    [Column("accept_amount")]  public decimal? AcceptAmount  { get; set; }
    [Column("status")]         public int      Status        { get; set; } = 0; // 0待开始 1进行中 2已完成
    [Column("is_overdue")]     public bool     IsOverdue     { get; set; }
    [Column("remark")]         public string?  Remark        { get; set; }
    [Column("sort")]           public int      Sort          { get; set; }
    public Project?  Project  { get; set; }
    public Employee? Owner    { get; set; }
}

[Table("proj_acceptance")]
public class ProjectAcceptance : BaseEntity
{
    [Column("project_id")]    public long     ProjectId    { get; set; }
    [Column("accept_batch")]  public string   AcceptBatch  { get; set; } = "";
    [Column("accept_date")]   public DateTime AcceptDate   { get; set; }
    [Column("accept_amount")] public decimal  AcceptAmount { get; set; }
    [Column("invoice_no")]    public string?  InvoiceNo    { get; set; }
    [Column("remark")]        public string?  Remark       { get; set; }
    public Project? Project { get; set; }
}

/// <summary>项目操作日志（只追加不修改）</summary>
[Table("proj_oper_log")]
public class ProjectOperLog : BaseEntity
{
    [Column("project_id")] public long     ProjectId { get; set; }
    [Column("title")]      public string   Title     { get; set; } = "";
    [Column("content")]    public string?  Content   { get; set; }
    [Column("remark")]     public string?  Remark    { get; set; }
    [Column("oper_by")]    public string   OperBy    { get; set; } = "";
    [Column("oper_at")]    public DateTime OperAt    { get; set; }
    public Project? Project { get; set; }
}

/// <summary>项目合同（一个项目可有多份合同：主合同/补充协议/变更合同）</summary>
[Table("proj_contract")]
public class ProjectContract : BaseEntity
{
    [Column("project_id")]      public long     ProjectId     { get; set; }
    [Column("contract_no")]     public string   ContractNo    { get; set; } = "";
    [Column("contract_type")]   public string   ContractType  { get; set; } = "主合同"; // 主合同/补充协议/变更合同
    [Column("contract_name")]   public string?  ContractName  { get; set; }
    [Column("party_a")]         public string   PartyA        { get; set; } = ""; // 甲方
    [Column("party_b")]         public string   PartyB        { get; set; } = ""; // 乙方
    [Column("amount")]          public decimal  Amount        { get; set; }
    [Column("sign_date")]       public DateTime? SignDate     { get; set; }
    [Column("start_date")]      public DateTime? StartDate    { get; set; }
    [Column("end_date")]        public DateTime? EndDate      { get; set; }
    [Column("file_path")]       public string?  FilePath      { get; set; } // 合同扫描件
    [Column("file_name")]       public string?  FileName      { get; set; }
    [Column("status")]          public int      Status        { get; set; } = 1; // 0无效 1生效
    [Column("remark")]          public string?  Remark        { get; set; }
    public Project? Project { get; set; }
}

/// <summary>项目发票</summary>
/// <summary>
/// 回款管理（原发票+验收合并）
/// 记录每笔回款的开票信息、收款状态及附件
/// </summary>
[Table("proj_invoice")]
public class ProjectInvoice : BaseEntity
{
    [Column("project_id")]        public long     ProjectId        { get; set; }
    [Column("receipt_name")]      public string   ReceiptName      { get; set; } = ""; // 回款批次名称
    [Column("invoice_no")]        public string?  InvoiceNo        { get; set; }
    [Column("invoice_type")]      public string   InvoiceType      { get; set; } = "增值税专用发票";
    [Column("amount")]            public decimal  Amount           { get; set; }
    [Column("tax_rate")]          public decimal? TaxRate          { get; set; }
    [Column("invoice_date")]      public DateTime? InvoiceDate     { get; set; }
    [Column("payer")]             public string?  Payer            { get; set; }
    [Column("is_received")]       public bool     IsReceived       { get; set; } = false;
    [Column("received_date")]     public DateTime? ReceivedDate    { get; set; }
    [Column("invoice_file")]      public string?  InvoiceFile      { get; set; } // 发票文件路径
    [Column("invoice_file_name")] public string?  InvoiceFileName  { get; set; }
    [Column("payment_file")]      public string?  PaymentFile      { get; set; } // 付款申请文件
    [Column("payment_file_name")] public string?  PaymentFileName  { get; set; }
    [Column("remark")]            public string?  Remark           { get; set; }
    public Project? Project { get; set; }
}

/// <summary>项目文件（资料收集/过程文件/成果文件）</summary>
[Table("proj_file")]
public class ProjectFile : BaseEntity
{
    [Column("project_id")]   public long     ProjectId   { get; set; }
    [Column("file_category")]public string   FileCategory{ get; set; } = ""; // 资料收集/过程文件/成果文件
    [Column("file_name")]    public string   FileName    { get; set; } = "";
    [Column("file_path")]    public string   FilePath    { get; set; } = "";
    [Column("file_size")]    public long     FileSize    { get; set; }
    [Column("file_ext")]     public string?  FileExt     { get; set; }
    [Column("description")]  public string?  Description { get; set; }
    [Column("version")]      public string?  Version     { get; set; } // 版本号（成果文件用）
    [Column("upload_by")]    public string   UploadBy    { get; set; } = "";
    public Project? Project { get; set; }
}
