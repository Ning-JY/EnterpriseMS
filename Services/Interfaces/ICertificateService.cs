using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

// ── 证书管理服务 ───────────────────────────────────────────
public interface ICertificateService
{
    Task<(List<EmployeeCertificate> Items, int Total, int WarnCount)> GetPagedAsync(
        string? keyword, int? status, int page, int size);

    Task<List<EmployeeSimpleDto>> GetEmployeesAsync();
    Task<List<DictDataDto>>      GetCertTypesAsync();

    Task<long> CreateWithFileAsync(long employeeId, string certName, string certType,
        string? certNo, string? issueOrg, DateTime? issueDate, DateTime? expireDate,
        IFormFile? file, string operBy);

    Task<(string path, string name)?> UploadAsync(long id, IFormFile file, string operBy);
    Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id);
    Task DeleteAsync(long id);
    Task DeleteFileAsync(long id, string operBy);
}
