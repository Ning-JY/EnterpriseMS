using System.ComponentModel.DataAnnotations;
using EnterpriseMS.Domain.Enums;

namespace EnterpriseMS.Services.DTOs.Hr;

// ── 下拉选项 ───────────────────────────────────────────────
public class EmployeeSimpleDto
{
    public long    Id       { get; set; }
    public string  RealName { get; set; } = "";
    public string? DeptName { get; set; }
    /// <summary>显示名：姓名（部门），用于下拉选项</summary>
    public string  Display  => string.IsNullOrEmpty(DeptName)
                               ? RealName : $"{RealName}（{DeptName}）";
}

// ── 列表展示 ───────────────────────────────────────────────
public class EmployeeListDto
{
    public long      Id        { get; set; }
    public string    EmpNo     { get; set; } = "";
    public string    RealName  { get; set; } = "";
    public int       Gender    { get; set; }
    public string    GenderText => Gender == 1 ? "男" : "女";
    public string?   Phone     { get; set; }
    public string?   Email     { get; set; }
    public long?     DeptId    { get; set; }
    public string?   DeptName  { get; set; }
    public int       Status    { get; set; }
    public string    StatusText => Status switch
    {
        0 => "试用期",
        1 => "在职",
        2 => "离职",
        _ => ""
    };
    public DateTime? EntryDate  { get; set; }
    public DateTime? FormalDate { get; set; }
    public DateTime? LeaveDate  { get; set; }
    public string?   Avatar     { get; set; }
}

// ── 详情展示 ───────────────────────────────────────────────
public class EmployeeDetailDto : EmployeeListDto
{
    public string?   IdCard            { get; set; }
    public long?     PostId            { get; set; }
    public DateTime? ProbationEndDate  { get; set; }
    public string?   Remark            { get; set; }
    // ── 扩展字段 ──────────────────────────────────────
    public DateTime? BirthDate         { get; set; }
    public string?   NativePlace       { get; set; }
    public string?   Education         { get; set; }
    public string?   Major             { get; set; }
    public string?   GraduateSchool    { get; set; }
    public string?   EmergencyContact  { get; set; }
    public string?   EmergencyPhone    { get; set; }
    public string?   Address           { get; set; }
    public string?   BankAccount       { get; set; }
    public string?   BankName          { get; set; }
    public List<EmployeeContractDto>    Contracts    { get; set; } = new();
    public List<EmployeeCertificateDto> Certificates { get; set; } = new();
}

// ── 创建 ───────────────────────────────────────────────────
public class CreateEmployeeDto
{
    [Required(ErrorMessage = "姓名不能为空")]
    [MaxLength(50, ErrorMessage = "姓名不能超过50个字符")]
    public string RealName { get; set; } = "";

    [Required(ErrorMessage = "性别不能为空")]
    public int Gender { get; set; } = 1;

    [Phone(ErrorMessage = "手机号格式不正确")]
    [MaxLength(20, ErrorMessage = "手机号不能超过20个字符")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [MaxLength(100, ErrorMessage = "邮箱不能超过100个字符")]
    public string? Email { get; set; }

    [MaxLength(18, ErrorMessage = "身份证号不能超过18个字符")]
    public string? IdCard { get; set; }

    public long? DeptId { get; set; }
    public long? PostId { get; set; }

    [Required(ErrorMessage = "入职日期不能为空")]
    public DateTime? EntryDate { get; set; }

    public DateTime? ProbationEndDate { get; set; }

    [MaxLength(500, ErrorMessage = "备注不能超过500个字符")]
    public string? Remark { get; set; }

    // ── 扩展字段 ──────────────────────────────────────
    public DateTime? BirthDate { get; set; }
    [MaxLength(50)]  public string? NativePlace { get; set; }
    [MaxLength(20)]  public string? Education { get; set; }
    [MaxLength(50)]  public string? Major { get; set; }
    [MaxLength(100)] public string? GraduateSchool { get; set; }
    [MaxLength(50)]  public string? EmergencyContact { get; set; }
    [MaxLength(20)]  public string? EmergencyPhone { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(30)]  public string? BankAccount { get; set; }
    [MaxLength(100)] public string? BankName { get; set; }
}

// ── 更新 ───────────────────────────────────────────────────
public class UpdateEmployeeDto : CreateEmployeeDto
{
    [Required(ErrorMessage = "员工ID不能为空")]
    public long Id { get; set; }
}

// ── 查询条件 ───────────────────────────────────────────────
public class EmployeeQueryDto
{
    public int    Page    { get; set; } = 1;
    public int    Size    { get; set; } = 10;
    public string? Keyword { get; set; }
    public long?  DeptId  { get; set; }
    public int?   Status  { get; set; }
}

// ── 合同子 DTO ─────────────────────────────────────────────
public class EmployeeContractDto
{
    public long      Id          { get; set; }
    public string    ContractNo  { get; set; } = "";
    public string    ContractType { get; set; } = "";
    public DateTime  StartDate   { get; set; }
    public DateTime  EndDate     { get; set; }
    public DateTime? SignDate    { get; set; }
    public int       Status      { get; set; }
    public string    StatusText  => Status switch
    {
        0 => "履行中",
        1 => "已终止",
        2 => "已到期",
        _ => ""
    };
    public string?   FilePath    { get; set; }
    public string?   FileName    { get; set; }
    public string?   Remark      { get; set; }
    /// <summary>是否即将到期（30天内）</summary>
    public bool IsExpiringSoon => Status == 0 && EndDate <= DateTime.Now.AddDays(30) && EndDate > DateTime.Now;
    /// <summary>是否已过期</summary>
    public bool IsExpired => EndDate < DateTime.Now && Status == 0;
}

// ── 证书子 DTO ─────────────────────────────────────────────
public class EmployeeCertificateDto
{
    public long      Id         { get; set; }
    public string    CertName   { get; set; } = "";
    public string    CertType   { get; set; } = "";
    public string?   CertNo     { get; set; }
    public string?   IssueOrg   { get; set; }
    public DateTime? IssueDate  { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string?   FilePath   { get; set; }
    public string?   FileName   { get; set; }
    public int       Status     { get; set; }
    public string    StatusText => Status switch
    {
        0 => "有效",
        1 => "已过期",
        _ => ""
    };
    public string?   Remark     { get; set; }
    /// <summary>是否即将到期（90天内）</summary>
    public bool IsExpiringSoon => Status == 0 && ExpireDate.HasValue
        && ExpireDate.Value <= DateTime.Now.AddDays(90) && ExpireDate.Value > DateTime.Now;
}
