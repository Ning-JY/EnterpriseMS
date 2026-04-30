using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;

namespace EnterpriseMS.Domain.Entities.Hr;

[Table("hr_employee")]
public class Employee : BaseEntity
{
    [Column("emp_no")]             public string   EmpNo            { get; set; } = "";
    [Column("real_name")]          public string   RealName         { get; set; } = "";
    [Column("gender")]             public int      Gender           { get; set; } = 1;
    [Column("id_card")]            public string?  IdCard           { get; set; }
    [Column("phone")]              public string?  Phone            { get; set; }
    [Column("email")]              public string?  Email            { get; set; }
    [Column("dept_id")]            public long?    DeptId           { get; set; }
    [Column("post_id")]            public long?    PostId           { get; set; }
    [Column("status")]             public int      Status           { get; set; } = 0;
    [Column("entry_date")]         public DateTime? EntryDate       { get; set; }
    [Column("probation_end_date")] public DateTime? ProbationEndDate{ get; set; }
    [Column("formal_date")]        public DateTime? FormalDate      { get; set; }
    [Column("leave_date")]         public DateTime? LeaveDate       { get; set; }
    [Column("remark")]             public string?  Remark           { get; set; }
    // ── 扩展字段 ──────────────────────────────────────────
    [Column("avatar")]             public string?  Avatar           { get; set; }
    [Column("birth_date")]         public DateTime? BirthDate       { get; set; }
    [Column("native_place")]       public string?  NativePlace      { get; set; }
    [Column("education")]          public string?  Education        { get; set; }
    [Column("major")]              public string?  Major            { get; set; }
    [Column("graduate_school")]    public string?  GraduateSchool   { get; set; }
    [Column("emergency_contact")]  public string?  EmergencyContact { get; set; }
    [Column("emergency_phone")]    public string?  EmergencyPhone   { get; set; }
    [Column("address")]            public string?  Address          { get; set; }
    [Column("bank_account")]       public string?  BankAccount      { get; set; }
    [Column("bank_name")]          public string?  BankName         { get; set; }
    public SysDept? Dept { get; set; }
    public ICollection<EmployeeContract>    Contracts    { get; set; } = new List<EmployeeContract>();
    public ICollection<EmployeeCertificate> Certificates { get; set; } = new List<EmployeeCertificate>();
}

[Table("hr_contract")]
public class EmployeeContract : BaseEntity
{
    [Column("employee_id")]   public long     EmployeeId   { get; set; }
    [Column("contract_no")]   public string   ContractNo   { get; set; } = "";
    [Column("contract_type")] public string   ContractType { get; set; } = "";
    [Column("start_date")]    public DateTime StartDate    { get; set; }
    [Column("end_date")]      public DateTime EndDate      { get; set; }
    [Column("sign_date")]     public DateTime? SignDate    { get; set; }
    [Column("status")]        public int      Status       { get; set; } = 0;
    [Column("file_path")]     public string?  FilePath     { get; set; }
    [Column("file_name")]     public string?  FileName     { get; set; }
    [Column("remark")]        public string?  Remark       { get; set; }
    public Employee? Employee { get; set; }
}

[Table("hr_certificate")]
public class EmployeeCertificate : BaseEntity
{
    [Column("employee_id")]  public long     EmployeeId  { get; set; }
    [Column("cert_name")]    public string   CertName    { get; set; } = "";
    [Column("cert_type")]    public string   CertType    { get; set; } = "";
    [Column("cert_no")]      public string?  CertNo      { get; set; }
    [Column("issue_org")]    public string?  IssueOrg    { get; set; }
    [Column("issue_date")]   public DateTime? IssueDate  { get; set; }
    [Column("expire_date")]  public DateTime? ExpireDate { get; set; }
    [Column("file_path")]    public string?  FilePath    { get; set; }
    [Column("file_name")]    public string?  FileName    { get; set; }
    [Column("status")]       public int      Status      { get; set; } = 0;
    [Column("remark")]       public string?  Remark      { get; set; }
    public Employee? Employee { get; set; }
}
