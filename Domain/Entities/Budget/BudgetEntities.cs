using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;

namespace EnterpriseMS.Domain.Entities.Budget;

[Table("budget_task")]
public class BudgetTask : BaseEntity
{
    [Column("task_no")]          public string   TaskNo         { get; set; } = "";
    [Column("task_name")]        public string   TaskName       { get; set; } = "";
    [Column("task_type")]        public int      TaskType       { get; set; }
    [Column("dept_id")]          public long?    DeptId         { get; set; }
    [Column("owner_name")]       public string   OwnerName      { get; set; } = "";
    [Column("owner_contact")]    public string?  OwnerContact   { get; set; }
    [Column("owner_phone")]      public string?  OwnerPhone     { get; set; }
    [Column("contractor_name")]  public string?  ContractorName { get; set; }
    [Column("building_scale")]   public string?  BuildingScale  { get; set; }
    [Column("submit_amount")]    public decimal? SubmitAmount   { get; set; }
    [Column("approved_amount")]  public decimal? ApprovedAmount { get; set; }
    [Column("reduction_rate")]   public decimal? ReductionRate  { get; set; }
    [Column("quota_basis")]      public string?  QuotaBasis     { get; set; }
    [Column("fee_standard")]     public string?  FeeStandard    { get; set; }
    [Column("tech_leader_id")]   public long?    TechLeaderId   { get; set; }
    [Column("biz_leader_id")]    public long?    BizLeaderId    { get; set; }
    [Column("plan_date")]        public DateTime? PlanDate      { get; set; }
    [Column("status")]           public int      Status         { get; set; } = 0;
    [Column("remark")]           public string?  Remark         { get; set; }
    public SysDept?  Dept       { get; set; }
    public Employee? TechLeader { get; set; }
    public Employee? BizLeader  { get; set; }
    public ICollection<BudgetSection>  Sections  { get; set; } = new List<BudgetSection>();
    public ICollection<ReviewOpinion>  Opinions  { get; set; } = new List<ReviewOpinion>();
}

[Table("budget_section")]
public class BudgetSection : BaseEntity
{
    [Column("task_id")]          public long     TaskId         { get; set; }
    [Column("section_no")]       public int      SectionNo      { get; set; }
    [Column("section_name")]     public string   SectionName    { get; set; } = "";
    [Column("category")]         public string   Category       { get; set; } = "";
    [Column("contract_amount")]  public decimal? ContractAmount { get; set; }
    [Column("submit_amount")]    public decimal? SubmitAmount   { get; set; }
    [Column("approved_amount")]  public decimal? ApprovedAmount { get; set; }
    [Column("status")]           public int      Status         { get; set; } = 0;
    public BudgetTask? Task { get; set; }
}

[Table("review_opinion")]
public class ReviewOpinion : BaseEntity
{
    [Column("task_id")]         public long     TaskId        { get; set; }
    [Column("opinion_no")]      public string   OpinionNo     { get; set; } = "";
    [Column("opinion_type")]    public int      OpinionType   { get; set; } = 0;
    [Column("category")]        public string?  Category      { get; set; }
    [Column("amount")]          public decimal  Amount        { get; set; }
    [Column("content")]         public string   Content       { get; set; } = "";
    [Column("basis")]           public string?  Basis         { get; set; }
    [Column("confirm_status")]  public int      ConfirmStatus { get; set; } = 0;
    public BudgetTask? Task { get; set; }
}
