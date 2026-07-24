using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 工作经历服务 ───────────────────────────────────────────
public interface IWorkExpService
{
    Task<List<WorkExpDto>> GetListAsync(long employeeId);
    Task<long>             CreateAsync(CreateWorkExpDto dto, long employeeId, string? operBy);
    Task                   UpdateAsync(WorkExpDto dto);
    Task                   DeleteAsync(long id);
}
