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
    public string? Remark    { get; set; }
    public string StatusText => Status == 1 ? "正常" : "禁用";
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleDto
{
    [Required, MaxLength(50)] public string RoleName  { get; set; } = "";
    [Required, MaxLength(50)] public string RoleCode  { get; set; } = "";
    public int     DataScope { get; set; } = 1;
    public int     Sort      { get; set; }
    public int     Status    { get; set; } = 1;
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

// 扁平列表（含上级菜单名，用于列表页）
public class MenuListDto
{
    public long   Id          { get; set; }
    public long   ParentId    { get; set; }
    public string MenuName    { get; set; } = "";
    public string ParentName  { get; set; } = "";
    public string MenuType    { get; set; } = "C";
    public string MenuTypeText => MenuType switch { "M" => "目录", "C" => "菜单", "F" => "按钮", _ => MenuType };
    public string? Perms      { get; set; }
    public string? Path       { get; set; }
    public string? Icon       { get; set; }
    public int    Sort        { get; set; }
    public int    Visible     { get; set; } = 1;
    public int    Status      { get; set; } = 1;
    public bool   HasChildren { get; set; }
    public int    Depth       { get; set; } = 0;
}

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
    public int     Status   { get; set; } = 1;
}

public class UpdateDeptDto : CreateDeptDto { public long Id { get; set; } }

// 扁平列表（含上级部门名，用于列表页）
public class DeptListDto
{
    public long   Id          { get; set; }
    public long   ParentId    { get; set; }
    public string DeptName    { get; set; } = "";
    public string ParentName  { get; set; } = "";
    public string? Leader     { get; set; }
    public string? Phone      { get; set; }
    public int    Sort        { get; set; }
    public int    Status      { get; set; }
    public bool   HasChildren { get; set; }
    public string StatusText => Status == 1 ? "正常" : "停用";
}

// ── Dict DTOs ──
public class DictTypeDto
{
    public long   Id       { get; set; }
    public string DictName { get; set; } = "";
    public string DictType { get; set; } = "";
    public int    Status   { get; set; }
    public string? Remark  { get; set; }
}

public class DictDataDto
{
    public long   Id         { get; set; }
    public string DictType   { get; set; } = "";
    public string DictLabel  { get; set; } = "";
    public string DictValue  { get; set; } = "";
    public int    Sort       { get; set; }
    public int    IsDefault  { get; set; }
    public int    Status     { get; set; } = 1;
}

public class CreateDictTypeDto
{
    [Required, MaxLength(50)] public string DictName { get; set; } = "";
    [Required, MaxLength(50)] public string DictType { get; set; } = "";
    public int     Status { get; set; } = 1;
    public string? Remark { get; set; }
}

public class UpdateDictTypeDto : CreateDictTypeDto { public long Id { get; set; } }

public class CreateDictDataDto
{
    [Required, MaxLength(50)] public string DictType  { get; set; } = "";
    [Required, MaxLength(100)] public string DictLabel { get; set; } = "";
    [Required, MaxLength(100)] public string DictValue { get; set; } = "";
    public int Sort       { get; set; }
    public int IsDefault  { get; set; }
    public int Status     { get; set; } = 1;
}

public class UpdateDictDataDto : CreateDictDataDto { public long Id { get; set; } }

// ── Config DTOs ──
public class SysConfigDto
{
    public string ConfigKey   { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public string? ConfigType { get; set; }
    public string? GroupName  { get; set; }
    public int    Sort        { get; set; }
}

// 扁平列表（系统参数，用于列表页）
public class ConfigListDto
{
    public long   Id          { get; set; }
    public string GroupName   { get; set; } = "";
    public string ConfigKey   { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public string ConfigType  { get; set; } = "text";
    public int    Sort        { get; set; }
}
