using System.ComponentModel.DataAnnotations.Schema;
using EnterpriseMS.Domain.Base;

namespace EnterpriseMS.Domain.Entities.System;

[Table("sys_user")]
public class SysUser : BaseEntity
{
    [Column("username")]       public string   Username      { get; set; } = "";
    [Column("password_hash")]  public string   PasswordHash  { get; set; } = "";
    [Column("real_name")]      public string   RealName      { get; set; } = "";
    [Column("phone")]          public string?  Phone         { get; set; }
    [Column("email")]          public string?  Email         { get; set; }
    [Column("avatar")]         public string?  Avatar        { get; set; }
    [Column("dept_id")]        public long?    DeptId        { get; set; }
    [Column("post_id")]        public long?    PostId        { get; set; }
    [Column("status")]         public int      Status        { get; set; } = 1;
    [Column("last_login_time")]public DateTime? LastLoginTime { get; set; }
    [Column("remark")]         public string?  Remark        { get; set; }
    /// <summary>关联员工档案（一对一）</summary>
    [Column("employee_id")]    public long?    EmployeeId    { get; set; }
    public SysDept?                    Dept      { get; set; }
    public ICollection<SysUserRole>    UserRoles { get; set; } = new List<SysUserRole>();
}

[Table("sys_role")]
public class SysRole : BaseEntity
{
    [Column("role_name")]  public string  RoleName  { get; set; } = "";
    [Column("role_code")]  public string  RoleCode  { get; set; } = "";
    [Column("data_scope")] public int     DataScope { get; set; } = 1;
    [Column("sort")]       public int     Sort      { get; set; }
    [Column("status")]     public int     Status    { get; set; } = 1;
    [Column("remark")]     public string? Remark    { get; set; }
    public ICollection<SysUserRole>  UserRoles  { get; set; } = new List<SysUserRole>();
    public ICollection<SysRoleMenu>  RoleMenus  { get; set; } = new List<SysRoleMenu>();
}

[Table("sys_menu")]
public class SysMenu : BaseEntity
{
    [Column("menu_name")]  public string  MenuName  { get; set; } = "";
    [Column("parent_id")]  public long    ParentId  { get; set; }
    [Column("menu_type")]  public string  MenuType  { get; set; } = "C"; // M目录 C菜单 F按钮
    [Column("perms")]      public string? Perms     { get; set; }
    [Column("icon")]       public string? Icon      { get; set; }
    [Column("path")]       public string? Path      { get; set; }
    [Column("component")]  public string? Component { get; set; }
    [Column("sort")]       public int     Sort      { get; set; }
    [Column("visible")]    public int     Visible   { get; set; } = 1;
    [Column("status")]     public int     Status    { get; set; } = 1;
    public ICollection<SysRoleMenu> RoleMenus { get; set; } = new List<SysRoleMenu>();
}

[Table("sys_dept")]
public class SysDept : BaseEntity
{
    [Column("dept_name")]  public string  DeptName  { get; set; } = "";
    [Column("parent_id")]  public long    ParentId  { get; set; }
    [Column("ancestors")]  public string  Ancestors { get; set; } = "";
    [Column("leader")]     public string? Leader    { get; set; }
    [Column("phone")]      public string? Phone     { get; set; }
    [Column("sort")]       public int     Sort      { get; set; }
    [Column("status")]     public int     Status    { get; set; } = 1;
    public ICollection<SysUser> Users { get; set; } = new List<SysUser>();
}

[Table("sys_post")]
public class SysPost : BaseEntity
{
    [Column("post_name")] public string  PostName { get; set; } = "";
    [Column("post_code")] public string  PostCode { get; set; } = "";
    [Column("sort")]      public int     Sort     { get; set; }
    [Column("status")]    public int     Status   { get; set; } = 1;
}

[Table("sys_user_role")]
public class SysUserRole
{
    [Column("user_id")] public long UserId { get; set; }
    [Column("role_id")] public long RoleId { get; set; }
    public SysUser? User { get; set; }
    public SysRole? Role { get; set; }
}

[Table("sys_role_menu")]
public class SysRoleMenu
{
    [Column("role_id")] public long RoleId { get; set; }
    [Column("menu_id")] public long MenuId { get; set; }
    public SysRole? Role { get; set; }
    public SysMenu? Menu { get; set; }
}

[Table("sys_dict_type")]
public class SysDictType : BaseEntity
{
    [Column("dict_name")] public string  DictName { get; set; } = "";
    [Column("dict_type")] public string  DictType { get; set; } = "";
    [Column("status")]    public int     Status   { get; set; } = 1;    //1:全部数据，2：本部门数据，3：本部门及子部门数据，4：仅本人数据
    [Column("remark")]    public string? Remark   { get; set; }
    public ICollection<SysDictData> DictDatas { get; set; } = new List<SysDictData>();
}

[Table("sys_dict_data")]
public class SysDictData : BaseEntity
{
    [Column("dict_type")]   public string  DictType   { get; set; } = "";
    [Column("dict_label")]  public string  DictLabel  { get; set; } = "";
    [Column("dict_value")]  public string  DictValue  { get; set; } = "";
    [Column("sort")]        public int     Sort       { get; set; }
    [Column("is_default")]  public int     IsDefault  { get; set; }
    [Column("status")]      public int     Status     { get; set; } = 1;
}

[Table("sys_oper_log")]
public class SysOperLog
{
    public long      Id           { get; set; }
    [Column("title")]        public string   Title       { get; set; } = "";
    [Column("business_type")]public string?  BusinessType{ get; set; }
    [Column("method")]       public string?  Method      { get; set; }
    [Column("oper_url")]     public string?  OperUrl     { get; set; }
    [Column("oper_ip")]      public string?  OperIp      { get; set; }
    [Column("oper_name")]    public string?  OperName    { get; set; }
    [Column("content")]      public string?  Content     { get; set; }
    [Column("status")]       public int      Status      { get; set; } = 1;
    [Column("error_msg")]    public string?  ErrorMsg    { get; set; }
    [Column("oper_time")]    public DateTime OperTime    { get; set; }
    [Column("business_id")]  public long?    BusinessId  { get; set; }
}

[Table("sys_login_log")]
public class SysLoginLog
{
    public long     Id         { get; set; }
    [Column("username")]   public string   Username  { get; set; } = "";
    [Column("ip")]         public string?  Ip        { get; set; }
    [Column("browser")]    public string?  Browser   { get; set; }
    [Column("os")]         public string?  Os        { get; set; }
    [Column("status")]     public int      Status    { get; set; }
    [Column("msg")]        public string?  Msg       { get; set; }
    [Column("login_time")] public DateTime LoginTime { get; set; }
}
