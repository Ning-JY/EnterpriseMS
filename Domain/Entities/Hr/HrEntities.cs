using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;

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
    [Column("education")]          public string?  Education        { get; set; }
    [Column("remark")]             public string?  Remark           { get; set; }
    // 人事档案扩充字段
    [Column("nationality")]        public string?  Nationality      { get; set; }
    [Column("birth_date")]         public DateTime? BirthDate       { get; set; }
    [Column("political_status")]   public string?  PoliticalStatus  { get; set; }
    [Column("native_place")]       public string?  NativePlace      { get; set; }
    [Column("address")]            public string?  Address          { get; set; }
    [Column("highest_degree")]     public string?  HighestDegree    { get; set; }
    [Column("graduate_school")]    public string?  GraduateSchool   { get; set; }
    [Column("major")]              public string?  Major            { get; set; }
    [Column("work_start_date")]    public DateTime? WorkStartDate   { get; set; }
    [Column("technical_title")]    public string?  TechnicalTitle   { get; set; }
    [Column("technical_level")]    public string?  TechnicalLevel   { get; set; }
    [Column("emergency_contact")]  public string?  EmergencyContact { get; set; }
    [Column("emergency_phone")]    public string?  EmergencyPhone   { get; set; }
    [Column("bank_account")]       public string?  BankAccount      { get; set; }
    [Column("bank_name")]          public string?  BankName         { get; set; }
    [Column("social_insurance_no")]public string?  SocialInsuranceNo{ get; set; }
    [Column("profile_photo")]      public string?  ProfilePhoto     { get; set; }
    public SysDept? Dept { get; set; }
    public ICollection<EmployeeContract>    Contracts      { get; set; } = new List<EmployeeContract>();
    public ICollection<EmployeeCertificate> Certificates   { get; set; } = new List<EmployeeCertificate>();
    public ICollection<EmployeeEducation>   EducationList  { get; set; } = new List<EmployeeEducation>();
    public ICollection<EmployeeWorkExp>     WorkExperiences{ get; set; } = new List<EmployeeWorkExp>();
}

[Table("hr_education")]
public class EmployeeEducation : BaseEntity
{
    [Column("employee_id")]  public long     EmployeeId  { get; set; }
    [Column("school_name")]  public string   SchoolName  { get; set; } = "";
    [Column("major")]        public string   Major       { get; set; } = "";
    [Column("degree")]       public string   Degree      { get; set; } = "";
    [Column("start_date")]   public DateTime? StartDate { get; set; }
    [Column("end_date")]     public DateTime? EndDate   { get; set; }
    [Column("is_full_time")] public bool     IsFullTime  { get; set; } = true;
    [Column("remark")]       public string?  Remark      { get; set; }
    public Employee? Employee { get; set; }
}

[Table("hr_work_experience")]
public class EmployeeWorkExp : BaseEntity
{
    [Column("employee_id")]   public long     EmployeeId   { get; set; }
    [Column("company_name")]  public string   CompanyName  { get; set; } = "";
    [Column("position")]      public string   Position     { get; set; } = "";
    [Column("start_date")]    public DateTime? StartDate  { get; set; }
    [Column("end_date")]      public DateTime? EndDate    { get; set; }
    [Column("remark")]        public string?  Remark       { get; set; }
    public Employee? Employee { get; set; }
}

[Table("hr_contract")]
public class EmployeeContract : BaseEntity, IFileEntity
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
public class EmployeeCertificate : BaseEntity, IFileEntity
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
