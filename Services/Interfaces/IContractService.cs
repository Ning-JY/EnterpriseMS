using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

// ── 合同管理服务 ───────────────────────────────────────────
public interface IContractService
{
    Task<(List<EmployeeContract> Items, int Total, int WarnCount)> GetPagedAsync(
        string? keyword, int? status, int page, int size);

    Task<List<EmployeeSimpleDto>> GetEmployeesAsync();
    Task<List<DictDataDto>>      GetContractTypesAsync();

    Task<long> CreateWithFileAsync(long employeeId, string contractNo, string contractType,
        DateTime startDate, DateTime endDate, DateTime? signDate, string? remark,
        IFormFile? file, string operBy);

    Task<(string path, string name)?> UploadAsync(long id, IFormFile file, string operBy);
    Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id);
    Task DeleteAsync(long id);
    Task DeleteFileAsync(long id, string operBy);
    Task TerminateAsync(long id, string operBy);
}
