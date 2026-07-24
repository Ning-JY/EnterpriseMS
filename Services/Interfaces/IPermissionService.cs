using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IPermissionService
{
    Task<List<string>>    GetUserPermissionsAsync(long userId);
    Task<List<MenuTreeDto>> GetUserMenuTreeAsync(long userId);
    Task<(int DataScope, long? DeptId)> GetUserDataScopeAsync(long userId);
    Task<bool>            HasPermAsync(long userId, string perm);
    Task                  ClearUserCacheAsync(long userId);
    Task                  ClearRoleUsersCacheAsync(long roleId);
}
