using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

// ── PermissionService ────────────────────────────────────────
public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionCache _cache;
    private readonly AppDbContext _db;

    public PermissionService(IUnitOfWork uow, IPermissionCache cache, AppDbContext db)
    { _uow = uow; _cache = cache; _db = db; }

    public async Task<List<string>> GetUserPermissionsAsync(long userId)
    {
        var cached = await _cache.GetUserPermsAsync(userId);
        if (cached != null) return cached;

        var perms = await _db.SysUserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.SysRoleMenus, ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
            .Join(_db.SysMenus.Where(m => m.Status == 1 && !m.IsDeleted),
                  mid => mid, m => m.Id, (mid, m) => m.Perms)
            .Where(p => p != null)
            .Distinct()
            .ToListAsync();

        var result = perms.Where(p => !string.IsNullOrWhiteSpace(p)).Cast<string>().ToList();
        await _cache.SetUserPermsAsync(userId, result);
        return result;
    }

    public async Task<List<MenuTreeDto>> GetUserMenuTreeAsync(long userId)
    {
        // 超级管理员获取全部菜单
        // IgnoreQueryFilters 确保即使Role被软删除也能正确判断（理论上superadmin不会被删除）

        List<SysMenu> menus;

        // ── 匿名用户：只返回 Perms 为 null 的公开菜单 ──────────
        if (userId == 0)
        {
            menus = await _db.SysMenus
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1
                         && !m.IsDeleted && m.Perms == null)
                .OrderBy(m => m.Sort).ToListAsync();
            return BuildMenuTree(menus, 0);
        }

        var isAdmin = await _db.SysUserRoles
            .AnyAsync(ur => ur.UserId == userId &&
                _db.SysRoles.IgnoreQueryFilters()
                    .Any(r => r.Id == ur.RoleId && r.RoleCode == "superadmin" && !r.IsDeleted));

        if (isAdmin)
        {
            menus = await _db.SysMenus
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1 && !m.IsDeleted)
                .OrderBy(m => m.Sort).ToListAsync();
        }
        else
        {
            // 已登录用户：角色菜单 + 公开菜单（Perms==null）取并集
            var roleMenuIds = await _db.SysUserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_db.SysRoleMenus, ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
                .Distinct()
                .ToListAsync();

            menus = await _db.SysMenus
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1 && !m.IsDeleted
                         && (m.Perms == null || roleMenuIds.Contains(m.Id)))
                .OrderBy(m => m.Sort).ToListAsync();
        }
        return BuildMenuTree(menus, 0);
    }
    public async Task<(int DataScope, long? DeptId)> GetUserDataScopeAsync(long userId)
    {
        // 取该用户权限最大的角色（DataScope 值越小权限越大）
        var role = await _db.SysUserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.SysRoles.Where(r => r.Status == 1 && !r.IsDeleted),
                  ur => ur.RoleId, r => r.Id, (ur, r) => r)
            .OrderBy(r => r.DataScope)   // 取最高权限
            .FirstOrDefaultAsync();

        if (role == null) return (4, null);   // 无角色：仅本人

        // 同时返回用户所在部门（用于部门过滤）
        var user = await _db.SysUsers
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new { u.DeptId })
            .FirstOrDefaultAsync();

        return (role.DataScope, user?.DeptId);
    }

    private List<MenuTreeDto> BuildMenuTree(List<SysMenu> all, long parentId)
    {
        return all.Where(m => m.ParentId == parentId)
                  .Select(m => new MenuTreeDto
                  {
                      Id = m.Id, ParentId = m.ParentId, MenuName = m.MenuName,
                      MenuType = m.MenuType, Perms = m.Perms, Icon = m.Icon,
                      Path = m.Path, Sort = m.Sort, Visible = m.Visible, Status = m.Status,
                      Children = BuildMenuTree(all, m.Id),
                  }).ToList();
    }

    public async Task<bool> HasPermAsync(long userId, string perm)
    {
        var perms = await GetUserPermissionsAsync(userId);
        return perms.Contains(perm) || perms.Contains("*:*:*");
    }

    public async Task ClearUserCacheAsync(long userId)
    {
        await _cache.RemoveUserPermsAsync(userId);
        await _cache.RemoveUserMenuIdsAsync(userId);
    }

    public async Task ClearRoleUsersCacheAsync(long roleId)
    {
        var userIds = await _db.SysUserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId).ToListAsync();
        foreach (var uid in userIds)
            await ClearUserCacheAsync(uid);
    }
}

// ── RoleService ──────────────────────────────────────────────
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IPermissionService _permSvc;

    public RoleService(IUnitOfWork uow, IMapper mapper,
        AppDbContext db, IPermissionService permSvc)
    { _uow = uow; _mapper = mapper; _db = db; _permSvc = permSvc; }

    public async Task<PagedResult<RoleListDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.Roles.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(r => r.RoleName.Contains(keyword) || r.RoleCode.Contains(keyword));
        var total = await q.CountAsync();
        var list  = await q.OrderBy(r => r.Sort).Skip((page-1)*size).Take(size).ToListAsync();
        return new PagedResult<RoleListDto>
        { Items = _mapper.Map<List<RoleListDto>>(list), Total = total, Page = page, PageSize = size };
    }

    public async Task<List<RoleListDto>> GetAllActiveAsync()
    {
        var list = await _uow.Roles.GetListAsync(r => r.Status == 1);
        return _mapper.Map<List<RoleListDto>>(list);
    }

    public async Task<RoleListDto?> GetByIdAsync(long id)
    {
        var r = await _uow.Roles.GetByIdAsync(id);
        return r == null ? null : _mapper.Map<RoleListDto>(r);
    }

    public async Task<long> CreateAsync(CreateRoleDto dto, string operBy)
    {
        if (await _uow.Roles.AnyAsync(r => r.RoleCode == dto.RoleCode))
            throw new BusinessException("角色编码已存在");
        var role = new SysRole
        {
            RoleName = dto.RoleName, RoleCode = dto.RoleCode,
            DataScope = dto.DataScope, Sort = dto.Sort,
            Remark = dto.Remark, CreatedBy = operBy,
        };
        await _uow.Roles.AddAsync(role);
        await _uow.SaveChangesAsync();
        if (dto.MenuIds.Any()) await AssignMenusAsync(role.Id, dto.MenuIds);
        return role.Id;
    }

    public async Task UpdateAsync(UpdateRoleDto dto, string operBy)
    {
        var role = await _uow.Roles.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("角色不存在");
        role.RoleName  = dto.RoleName;
        role.DataScope = dto.DataScope;
        role.Sort      = dto.Sort;
        role.Remark    = dto.Remark;
        role.UpdatedBy = operBy;
        _uow.Roles.Update(role);
        await AssignMenusAsync(dto.Id, dto.MenuIds);
        await _uow.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(dto.Id);
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var role = await _uow.Roles.GetByIdAsync(id)
            ?? throw new NotFoundException("角色不存在");
        if (role.RoleCode == "superadmin")
            throw new BusinessException("超级管理员角色不可删除");
        _uow.Roles.SoftDelete(role);
        await _uow.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(id);
    }

    public async Task SetStatusAsync(long id, int status)
    {
        var role = await _uow.Roles.GetByIdAsync(id)
            ?? throw new NotFoundException("角色不存在");
        role.Status = status;
        _uow.Roles.Update(role);
        await _uow.SaveChangesAsync();
    }

    public async Task AssignMenusAsync(long roleId, List<long> menuIds)
    {
        var old = await _db.SysRoleMenus.Where(rm => rm.RoleId == roleId).ToListAsync();
        _db.SysRoleMenus.RemoveRange(old);
        var news = menuIds.Distinct().Select(mid => new SysRoleMenu { RoleId = roleId, MenuId = mid });
        await _db.SysRoleMenus.AddRangeAsync(news);
        await _db.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(roleId);
    }

    public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
        => await _db.SysRoleMenus.Where(rm => rm.RoleId == roleId)
                                  .Select(rm => rm.MenuId).ToListAsync();
}

// ── MenuService ──────────────────────────────────────────────
public class MenuService : IMenuService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public MenuService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<List<MenuTreeDto>> GetTreeAsync(bool onlyVisible = false)
    {
        var q = _uow.Menus.Query();
        if (onlyVisible) q = q.Where(m => m.Visible == 1 && m.Status == 1);
        var all = await q.OrderBy(m => m.Sort).ToListAsync();
        return BuildTree(_mapper.Map<List<MenuTreeDto>>(all), 0);
    }

    private List<MenuTreeDto> BuildTree(List<MenuTreeDto> all, long parentId)
        => all.Where(m => m.ParentId == parentId)
              .Select(m => { m.Children = BuildTree(all, m.Id); return m; }).ToList();

    public async Task<MenuTreeDto?> GetByIdAsync(long id)
    {
        var m = await _uow.Menus.GetByIdAsync(id);
        return m == null ? null : _mapper.Map<MenuTreeDto>(m);
    }

    public async Task<long> CreateAsync(CreateMenuDto dto, string operBy)
    {
        var menu = _mapper.Map<SysMenu>(dto);
        menu.CreatedBy = operBy;
        await _uow.Menus.AddAsync(menu);
        await _uow.SaveChangesAsync();
        return menu.Id;
    }

    public async Task UpdateAsync(UpdateMenuDto dto, string operBy)
    {
        var menu = await _uow.Menus.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("菜单不存在");
        menu.MenuName  = dto.MenuName;
        menu.ParentId  = dto.ParentId;
        menu.MenuType  = dto.MenuType;
        menu.Perms     = dto.Perms;
        menu.Icon      = dto.Icon;
        menu.Path      = dto.Path;
        menu.Component = dto.Component;
        menu.Sort      = dto.Sort;
        menu.Visible   = dto.Visible;
        menu.UpdatedBy = operBy;
        _uow.Menus.Update(menu);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (await _uow.Menus.AnyAsync(m => m.ParentId == id))
            throw new BusinessException("存在子菜单，不可删除");
        var menu = await _uow.Menus.GetByIdAsync(id)
            ?? throw new NotFoundException("菜单不存在");
        _uow.Menus.SoftDelete(menu);
        await _uow.SaveChangesAsync();
    }
}

// ── DeptService ──────────────────────────────────────────────
public class DeptService : IDeptService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DeptService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<List<DeptTreeDto>> GetTreeAsync()
    {
        var all = await _uow.Depts.Query().OrderBy(d => d.Sort).ToListAsync();
        return BuildTree(_mapper.Map<List<DeptTreeDto>>(all), 0);
    }

    private List<DeptTreeDto> BuildTree(List<DeptTreeDto> all, long parentId)
        => all.Where(d => d.ParentId == parentId)
              .Select(d => { d.Children = BuildTree(all, d.Id); return d; }).ToList();

    public async Task<DeptTreeDto?> GetByIdAsync(long id)
    {
        var d = await _uow.Depts.GetByIdAsync(id);
        return d == null ? null : _mapper.Map<DeptTreeDto>(d);
    }

    public async Task<List<DeptTreeDto>> GetChildrenAsync(long parentId)
    {
        var list = await _uow.Depts.GetListAsync(d => d.ParentId == parentId);
        return _mapper.Map<List<DeptTreeDto>>(list);
    }

    public async Task<long> CreateAsync(CreateDeptDto dto, string operBy)
    {
        var parent = await _uow.Depts.GetByIdAsync(dto.ParentId);
        var ancestors = parent == null ? "0" : parent.Ancestors + "," + parent.Id;
        var dept = new SysDept
        {
            DeptName  = dto.DeptName, ParentId = dto.ParentId,
            Ancestors = ancestors, Leader = dto.Leader,
            Phone = dto.Phone, Sort = dto.Sort, CreatedBy = operBy,
        };
        await _uow.Depts.AddAsync(dept);
        await _uow.SaveChangesAsync();
        return dept.Id;
    }

    public async Task UpdateAsync(UpdateDeptDto dto, string operBy)
    {
        var dept = await _uow.Depts.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("部门不存在");
        dept.DeptName  = dto.DeptName;
        dept.Leader    = dto.Leader;
        dept.Phone     = dto.Phone;
        dept.Sort      = dto.Sort;
        dept.UpdatedBy = operBy;
        _uow.Depts.Update(dept);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (await _uow.Depts.AnyAsync(d => d.ParentId == id))
            throw new BusinessException("存在子部门，不可删除");
        if (await _uow.Users.AnyAsync(u => u.DeptId == id))
            throw new BusinessException("部门下存在用户，不可删除");
        var dept = await _uow.Depts.GetByIdAsync(id)
            ?? throw new NotFoundException("部门不存在");
        _uow.Depts.SoftDelete(dept);
        await _uow.SaveChangesAsync();
    }
}

// ── DictService ──────────────────────────────────────────────
public class DictService : IDictService
{
    private readonly IUnitOfWork _uow;
    public DictService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<DictDataDto>> GetDataByTypeAsync(string dictType)
    {
        var list = await _uow.DictDatas.GetListAsync(
            d => d.DictType == dictType && d.Status == 1);
        return list.OrderBy(d => d.Sort)
                   .Select(d => new DictDataDto
                   { Id = d.Id, DictType = d.DictType, DictLabel = d.DictLabel,
                     DictValue = d.DictValue, Sort = d.Sort, IsDefault = d.IsDefault })
                   .ToList();
    }

    public async Task<List<DictTypeDto>> GetAllTypesAsync()
    {
        var list = await _uow.DictTypes.GetListAsync(d => d.Status == 1);
        return list.Select(d => new DictTypeDto
            { Id = d.Id, DictName = d.DictName, DictType = d.DictType, Status = d.Status })
            .ToList();
    }
}

// ── OperLogService ───────────────────────────────────────────
public class OperLogService : IOperLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpCtx;

    public OperLogService(AppDbContext db, IHttpContextAccessor httpCtx)
    { _db = db; _httpCtx = httpCtx; }

    public async Task LogAsync(string title, string? content = null,
        string? businessType = null, long? businessId = null)
    {
        var ctx  = _httpCtx.HttpContext;
        var log  = new SysOperLog
        {
            Title        = title,
            Content      = content,
            BusinessType = businessType,
            BusinessId   = businessId,
            OperName     = ctx?.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            OperUrl      = ctx?.Request.Path,
            OperIp       = ctx?.Connection.RemoteIpAddress?.ToString(),
            Status       = 1,
            OperTime     = DateTime.Now,
        };
        log.Id = EnterpriseMS.Common.SnowflakeId.Next();
        await _db.SysOperLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }
}
