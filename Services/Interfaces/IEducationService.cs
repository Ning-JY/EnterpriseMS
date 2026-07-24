using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 教育经历服务 ───────────────────────────────────────────
public interface IEducationService
{
    Task<List<EducationDto>> GetListAsync(long employeeId);
    Task<long>               CreateAsync(CreateEducationDto dto, long employeeId, string? operBy);
    Task                     UpdateAsync(EducationDto dto);
    Task                     DeleteAsync(long id);
}
