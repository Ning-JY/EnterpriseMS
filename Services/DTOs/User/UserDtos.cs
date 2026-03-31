using System.ComponentModel.DataAnnotations;

namespace EnterpriseMS.Services.DTOs.User;

public class UserListDto
{
    public long     Id            { get; set; }
    public string   Username      { get; set; } = "";
    public string   RealName      { get; set; } = "";
    public string?  Phone         { get; set; }
    public string?  Email         { get; set; }
    public string?  DeptName      { get; set; }
    public string?  PostName      { get; set; }
    public int      Status        { get; set; }
    public string   StatusText    => Status == 1 ? "正常" : "禁用";
    public DateTime? LastLoginTime { get; set; }
    public DateTime  CreatedAt    { get; set; }
    public List<string> RoleNames { get; set; } = new();
}

public class UserDetailDto : UserListDto
{
    public List<long>   RoleIds    { get; set; } = new();
    public long?        DeptId     { get; set; }
    public long?        PostId     { get; set; }
    public string?      Avatar     { get; set; }
    public string?      Remark     { get; set; }
    /// <summary>绑定的员工档案ID（可空，admin等系统账号不绑定）</summary>
    public long?        EmployeeId { get; set; }
    /// <summary>绑定员工姓名（仅展示用）</summary>
    public string?      EmployeeName { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50, ErrorMessage = "用户名最多50个字符")]
    public string  Username  { get; set; } = "";

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码最少6位")]
    [MaxLength(100)]
    public string  Password  { get; set; } = "";

    [Required(ErrorMessage = "姓名不能为空")]
    [MaxLength(50)]
    public string  RealName  { get; set; } = "";

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? Phone     { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email     { get; set; }

    public long?        DeptId     { get; set; }
    public long?        PostId     { get; set; }
    public List<long>   RoleIds    { get; set; } = new();
    public string?      Remark     { get; set; }
    /// <summary>绑定员工档案ID，null 表示解除绑定</summary>
    public long?        EmployeeId { get; set; }
}

public class UpdateUserDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "姓名不能为空")]
    [MaxLength(50)]
    public string  RealName  { get; set; } = "";

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? Phone     { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email     { get; set; }

    public long?        DeptId     { get; set; }
    public long?        PostId     { get; set; }
    public List<long>   RoleIds    { get; set; } = new();
    public string?      Remark     { get; set; }
    /// <summary>绑定员工档案ID，null 表示解除绑定</summary>
    public long?        EmployeeId { get; set; }
}

public class UserQueryDto
{
    public string? Keyword  { get; set; }
    public long?   DeptId   { get; set; }
    public int?    Status   { get; set; }
    public int     Page     { get; set; } = 1;
    public int     Size     { get; set; } = 10;
}

public class ResetPasswordDto
{
    public long   UserId     { get; set; }
    [Required, MinLength(6)]
    public string NewPassword { get; set; } = "";
}

public class ChangePasswordDto
{
    [Required] public string OldPassword { get; set; } = "";
    [Required, MinLength(6)] public string NewPassword { get; set; } = "";
}
