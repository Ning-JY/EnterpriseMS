using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 员工自定义字段（详情页"其他信息"栏）────────────────────
public interface IEmployeeExtraService
{
    Task<List<EmployeeExtraDto>> GetListAsync(long employeeId);
    Task<long>                   SaveAsync(EmployeeExtraDto dto, string? operBy);
    Task                         DeleteAsync(long id);
}
