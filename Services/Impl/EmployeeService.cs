using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class EmployeeService : IEmployeeService
{
    public Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeeQueryDto query)
    {
        return Task.FromResult(new PagedResult<EmployeeListDto>());
    }

    public Task<EmployeeDetailDto?> GetDetailAsync(long id)
    {
        return Task.FromResult<EmployeeDetailDto?>(null);
    }

    public Task<long> CreateAsync(CreateEmployeeDto dto, string operBy)
    {
        return Task.FromResult(0L);
    }

    public Task UpdateAsync(UpdateEmployeeDto dto, string operBy)
    {
        return Task.CompletedTask;
    }

    public Task FormalAsync(long id, string operBy)
    {
        return Task.CompletedTask;
    }

    public Task LeaveAsync(long id, string operBy, string? reason = null)
    {
        return Task.CompletedTask;
    }
}
