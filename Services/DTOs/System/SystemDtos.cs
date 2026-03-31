using System.ComponentModel.DataAnnotations;

namespace EnterpriseMS.Services.DTOs.System;

// ── Role DTOs ──
public class RoleListDto
{
    public long   Id        { get; set; }
    public string RoleName  { get; set; } = "";
    public string RoleCode  { get; set; } = "";
    public int    DataScope { get; set; }
    public int    Sort      { get; set; }
    public int    Status    { get; set; }
    public string StatusText => Status == 1 ? "正常" : "禁用";
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleDto
{
    [Required, MaxLength(50)] public string RoleName  { get; set; } = "";
    [Required, MaxLength(50)] public string RoleCode  { get; set; } = "";
    public int     DataScope { get; set; } = 1;
    public int     Sort      { get; set; }
    public string? Remark    { get; set; }
    public List<long> MenuIds { get; set; } = new();
}

public class UpdateRoleDto : CreateRoleDto { public long Id { get; set; } }

// ── Menu DTOs ──
public class MenuTreeDto
{
    public long   Id       { get; set; }
    public long   ParentId { get; set; }
    public string MenuName { get; set; } = "";
    public string MenuType { get; set; } = "C";
    public string? Perms   { get; set; }
    public string? Icon    { get; set; }
    public string? Path    { get; set; }
    public int    Sort     { get; set; }
    public int    Visible  { get; set; }
    public int    Status   { get; set; }
    public List<MenuTreeDto> Children { get; set; } = new();
}

public class CreateMenuDto
{
    [Required, MaxLength(50)] public string  MenuName  { get; set; } = "";
    public long    ParentId  { get; set; }
    public string  MenuType  { get; set; } = "C";
    public string? Perms     { get; set; }
    public string? Icon      { get; set; }
    public string? Path      { get; set; }
    public string? Component { get; set; }
    public int     Sort      { get; set; }
    public int     Visible   { get; set; } = 1;
}

public class UpdateMenuDto : CreateMenuDto { public long Id { get; set; } }

// ── Dept DTOs ──
public class DeptTreeDto
{
    public long   Id        { get; set; }
    public long   ParentId  { get; set; }
    public string DeptName  { get; set; } = "";
    public string? Leader   { get; set; }
    public string? Phone    { get; set; }
    public int    Sort      { get; set; }
    public int    Status    { get; set; }
    public List<DeptTreeDto> Children { get; set; } = new();
}

public class CreateDeptDto
{
    [Required, MaxLength(50)] public string  DeptName { get; set; } = "";
    public long    ParentId { get; set; }
    public string? Leader   { get; set; }
    public string? Phone    { get; set; }
    public int     Sort     { get; set; }
}

public class UpdateDeptDto : CreateDeptDto { public long Id { get; set; } }

// ── Dict DTOs ──
public class DictTypeDto
{
    public long   Id       { get; set; }
    public string DictName { get; set; } = "";
    public string DictType { get; set; } = "";
    public int    Status   { get; set; }
}

public class DictDataDto
{
    public long   Id         { get; set; }
    public string DictType   { get; set; } = "";
    public string DictLabel  { get; set; } = "";
    public string DictValue  { get; set; } = "";
    public int    Sort       { get; set; }
    public int    IsDefault  { get; set; }
}
