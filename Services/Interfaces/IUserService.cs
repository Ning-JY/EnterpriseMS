using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Services.DTOs.User;

namespace EnterpriseMS.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListDto>> GetPagedAsync(UserQueryDto query);
    Task<UserDetailDto?>          GetDetailAsync(long id);
    Task<SysUser?>                GetByUsernameAsync(string username);
    Task<List<string>>            GetRoleCodesAsync(long userId);
    Task<long>                    CreateAsync(CreateUserDto dto, string operBy);
    Task                          UpdateAsync(UpdateUserDto dto, string operBy);
    Task                          DeleteAsync(long id, string operBy);
    Task                          SetStatusAsync(long id, int status, string operBy);
    Task                          ResetPasswordAsync(long id, string newPwd, string operBy);
    Task                          ChangePasswordAsync(long id, string oldPwd, string newPwd);
    Task                          AssignRolesAsync(long userId, List<long> roleIds);
    Task                          UpdateLastLoginAsync(long id, string? ip = null);
    Task<bool>                    ValidatePasswordAsync(string username, string password);
    Task<List<UserListDto>>       GetAllActiveAsync();
}
