using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IRoleService
{
    Task<PagedResult<RoleListDto>> GetPagedAsync(string? keyword, int page, int size);
    Task<List<RoleListDto>>        GetAllActiveAsync();
    Task<RoleListDto?>             GetByIdAsync(long id);
    Task<long>                     CreateAsync(CreateRoleDto dto, string operBy);
    Task                           UpdateAsync(UpdateRoleDto dto, string operBy);
    Task                           DeleteAsync(long id, string operBy);
    Task                           SetStatusAsync(long id, int status);
    Task                           AssignMenusAsync(long roleId, List<long> menuIds);
    Task<List<long>>               GetRoleMenuIdsAsync(long roleId);
}
