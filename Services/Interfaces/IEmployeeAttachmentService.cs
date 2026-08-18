using EnterpriseMS.Domain.Entities.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 员工附件服务（详情页「附件管理」Tab）──
public interface IEmployeeAttachmentService
{
    Task<List<EmployeeAttachment>> GetListAsync(long employeeId);
    Task<long>                     UploadAsync(long employeeId, IFormFile file, string? remark, string operBy);
    Task                           DeleteAsync(long id);
    Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id);
}
