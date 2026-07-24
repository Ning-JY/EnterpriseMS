using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionCache _cache;

    public PermissionService(IUnitOfWork uow, IPermissionCache cache)
    { _uow = uow; _cache = cache; }

    public async Task<List<string>> GetUserPermissionsAsync(long userId)
    {
        var cached = await _cache.GetUserPermsAsync(userId);
        if (cached != null) return cached;

        var perms = await _uow.UserRoles.Query()
            .Where(ur => ur.UserId == userId)
            .Join(_uow.RoleMenus.Query(), ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
            .Join(_uow.Menus.Query().Where(m => m.Status == 1 && !m.IsDeleted),
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
        List<SysMenu> menus;

        // ── 匿名用户：只返回 Perms 为 null 的公开菜单 ──────────
        if (userId == 0)
        {
            menus = await _uow.Menus.Query()
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1
                         && !m.IsDeleted && m.Perms == null)
                .OrderBy(m => m.Sort).ToListAsync();
            return BuildMenuTree(menus, 0);
        }
        var roleQuery = _uow.Roles.Query().IgnoreQueryFilters();
        var isAdmin = await _uow.UserRoles.Query()
            .AnyAsync(ur =>
                ur.UserId == userId &&
                roleQuery.Any(r =>
                    r.Id == ur.RoleId &&
                    r.RoleCode == "superadmin" &&
                    !r.IsDeleted
                )
            );
        if (isAdmin)
        {
            menus = await _uow.Menus.Query()
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1 && !m.IsDeleted)
                .OrderBy(m => m.Sort).ToListAsync();
        }
        else
        {
            var roleMenuIds = await _uow.UserRoles.Query()
                .Where(ur => ur.UserId == userId)
                .Join(_uow.RoleMenus.Query(), ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
                .Distinct()
                .ToListAsync();

            menus = await _uow.Menus.Query()
                .Where(m => m.MenuType != "F" && m.Visible == 1 && m.Status == 1 && !m.IsDeleted
                         && (m.Perms == null || roleMenuIds.Contains(m.Id)))
                .OrderBy(m => m.Sort).ToListAsync();
        }
        return BuildMenuTree(menus, 0);
    }

    public async Task<(int DataScope, long? DeptId)> GetUserDataScopeAsync(long userId)
    {
        var role = await _uow.UserRoles.Query()
            .Where(ur => ur.UserId == userId)
            .Join(_uow.Roles.Query().Where(r => r.Status == 1 && !r.IsDeleted),
                  ur => ur.RoleId, r => r.Id, (ur, r) => r)
            .OrderBy(r => r.DataScope)
            .FirstOrDefaultAsync();

        if (role == null) return (4, null);

        var user = await _uow.Users.Query()
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
        var userIds = await _uow.UserRoles.Query()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId).ToListAsync();
        foreach (var uid in userIds)
            await ClearUserCacheAsync(uid);
    }
}
