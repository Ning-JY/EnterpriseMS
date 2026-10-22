using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 员工自定义字段（详情页"其他信息"栏）────────────────────
public interface IEmployeeExtraService
{
    Task<List<EmployeeExtraDto>> GetListAsync(long employeeId);
    /// <summary>批量取多人其他信息合并文本（列表页一页一次查询，不按人逐条查）。Key 为员工 Id，无字段的员工不出现在字典里。</summary>
    Task<Dictionary<long, string>> GetSummariesAsync(IEnumerable<long> employeeIds);
    Task<long>                   SaveAsync(EmployeeExtraDto dto, string? operBy);
    Task                         DeleteAsync(long id);
}
