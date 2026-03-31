using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 用户服务 ─────────────────────────────────────────────────
public interface IUserService
{
    Task<PagedResult<UserListDto>> GetPagedAsync(UserQueryDto query);
    Task<UserDetailDto?>          GetDetailAsync(long id);
    Task<SysUser?>                GetByUsernameAsync(string username);
    Task<long>                    CreateAsync(CreateUserDto dto, string operBy);
    Task                          UpdateAsync(UpdateUserDto dto, string operBy);
    Task                          DeleteAsync(long id, string operBy);
    Task                          SetStatusAsync(long id, int status, string operBy);
    Task                          ResetPasswordAsync(long id, string newPwd, string operBy);
    Task                          ChangePasswordAsync(long id, string oldPwd, string newPwd);
    Task                          AssignRolesAsync(long userId, List<long> roleIds);
    Task                          UpdateLastLoginAsync(long id);
    Task<bool>                    ValidatePasswordAsync(string username, string password);
    Task<List<UserListDto>>       GetAllActiveAsync();
}

// ── 权限服务 ─────────────────────────────────────────────────
public interface IPermissionService
{
    Task<List<string>>    GetUserPermissionsAsync(long userId);
    Task<List<MenuTreeDto>> GetUserMenuTreeAsync(long userId);
    Task<(int DataScope, long? DeptId)> GetUserDataScopeAsync(long userId);
    Task<bool>            HasPermAsync(long userId, string perm);
    Task                  ClearUserCacheAsync(long userId);
    Task                  ClearRoleUsersCacheAsync(long roleId);
}

// ── 角色服务 ─────────────────────────────────────────────────
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

// ── 菜单服务 ─────────────────────────────────────────────────
public interface IMenuService
{
    Task<List<MenuTreeDto>>  GetTreeAsync(bool onlyVisible = false);
    Task<MenuTreeDto?>       GetByIdAsync(long id);
    Task<long>               CreateAsync(CreateMenuDto dto, string operBy);
    Task                     UpdateAsync(UpdateMenuDto dto, string operBy);
    Task                     DeleteAsync(long id);
}

// ── 部门服务 ─────────────────────────────────────────────────
public interface IDeptService
{
    Task<List<DeptTreeDto>>  GetTreeAsync();
    Task<DeptTreeDto?>       GetByIdAsync(long id);
    Task<long>               CreateAsync(CreateDeptDto dto, string operBy);
    Task                     UpdateAsync(UpdateDeptDto dto, string operBy);
    Task                     DeleteAsync(long id);
    Task<List<DeptTreeDto>>  GetChildrenAsync(long parentId);
}

// ── 字典服务 ─────────────────────────────────────────────────
public interface IDictService
{
    Task<List<DictDataDto>> GetDataByTypeAsync(string dictType);
    Task<List<DictTypeDto>> GetAllTypesAsync();
}

// ── 日志服务 ─────────────────────────────────────────────────
public interface IOperLogService
{
    Task LogAsync(string title, string? content = null,
        string? businessType = null, long? businessId = null);
}

// ── 项目服务 ─────────────────────────────────────────────────
public interface IProjectService
{
    Task<PagedResult<ProjectListDto>> GetPagedAsync(ProjectQueryDto query, long operUserId);
    Task<ProjectDetailDto?>           GetDetailAsync(long id);
    Task<long>                        CreateAsync(CreateProjectDto dto, string operBy);
    Task                              UpdateAsync(UpdateProjectDto dto, string operBy);
    Task                              ChangeStatusAsync(ChangeStatusDto dto, string operBy);
    Task                              TerminateAsync(long id, string reason, string operBy);
    Task<string>                      GenerateProjNoAsync();
    // 成员
    Task<long>   AddMemberAsync(long projectId, CreateMemberDto dto, string operBy);
    Task         UpdateMemberAsync(long projectId, UpdateMemberDto dto, string operBy);
    Task         RemoveMemberAsync(long projectId, long memberId, string operBy);
    // 里程碑
    Task<long>   AddMilestoneAsync(long projectId, CreateMilestoneDto dto, string operBy);
    Task         UpdateMilestoneAsync(long projectId, UpdateMilestoneDto dto, string operBy);
    Task         DeleteMilestoneAsync(long milestoneId);
    Task         CompleteMilestoneAsync(long milestoneId, string operBy);
    // 验收
    Task<long>    AddAcceptanceAsync(CreateAcceptanceDto dto, string operBy);
    Task<decimal> GetTotalReceivedAsync(long projectId);
    // 合同
    Task<long> AddContractAsync(CreateContractDto dto, string operBy);
    Task       DeleteContractAsync(long contractId);
    // 发票
    Task<long> AddInvoiceAsync(CreateInvoiceDto dto, string operBy);
    Task       ConfirmInvoiceReceivedAsync(long invoiceId, DateTime receivedDate, string operBy);
    // 文件
    Task<long> AddFileAsync(long projectId, string category, string fileName,
        string filePath, long fileSize, string? description, string? version, string operBy);
    Task       DeleteFileAsync(long fileId);
    // 统计
    Task<object> GetDashboardStatsAsync();
    Task<object> GetMyStatsAsync(long employeeId);
}

// ── 员工查询服务（供项目/概预算下拉菜单使用，绑定hr_employee表）─────
public interface IEmployeeQueryService
{
    /// <summary>获取所有在职员工（状态0试用/1在职），供下拉菜单选择</summary>
    Task<List<EmployeeSimpleDto>> GetAllOnJobAsync();
}
