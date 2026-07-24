using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 员工档案服务 ───────────────────────────────────────────
public interface IEmployeeService
{
    Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryDto query);
    Task<List<long>>            GetBoundEmployeeIdsAsync();
    Task<Employee?>             GetDetailAsync(long id);
    Task<List<SysPost>>         GetPostsAsync();
    Task<Employee?>             GetByIdAsync(long id);
    Task<List<Employee>>        GetOnJobAsync();
    Task<long>                  CreateAsync(CreateEmployeeDto dto, string operBy);
    Task                        UpdateAsync(UpdateEmployeeDto dto, string operBy);
    Task                        FormalAsync(long id, DateTime formalDate, string operBy);
    Task                        LeaveAsync(long id, DateTime leaveDate, string? reason, string operBy);
}
