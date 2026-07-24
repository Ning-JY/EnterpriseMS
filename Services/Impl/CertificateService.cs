using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 证书管理服务实现。承接原 CertificateController 的数据访问与文件上传。
/// 与 Contract 一致统一走 FileUploadHelper（修复原 Certificate 手写文件流的风格不一致问题）。
/// </summary>
public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork           _uow;
    private readonly IDictService          _dictSvc;
    private readonly IEmployeeQueryService _empQrySvc;

    public CertificateService(IUnitOfWork uow, IDictService dictSvc,
        IEmployeeQueryService empQrySvc)
    {
        _uow = uow; _dictSvc = dictSvc; _empQrySvc = empQrySvc;
    }

    public async Task<PagedResult<EmployeeCertificate>> GetPagedAsync(
        string? keyword, int? status, int page, int size)
    {
        var q = _uow.Certificates.Query().Include(c => c.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => (c.Employee != null && c.Employee.RealName.Contains(keyword)) ||
                             c.CertName.Contains(keyword));
        if (status.HasValue) q = q.Where(c => c.Status == status);

        var warnDate  = DateTime.UtcNow.Date.AddDays(60);
        var warnCount = await q.CountAsync(c => c.Status == 0 &&
                                c.ExpireDate.HasValue && c.ExpireDate <= warnDate);
        var paged     = await q.OrderByDescending(c => c.CreatedAt).ToPagedAsync(page, size);
        paged.WarnCount = warnCount;
        return paged;
    }

    public Task<List<EmployeeSimpleDto>> GetEmployeesAsync()
        => _empQrySvc.GetAllOnJobAsync();

    public Task<List<DictDataDto>> GetCertTypesAsync()
        => _dictSvc.GetDataByTypeAsync(DictType.CertType);

    public async Task<long> CreateWithFileAsync(long employeeId, string certName, string certType,
        string? certNo, string? issueOrg, DateTime? issueDate, DateTime? expireDate,
        IFormFile? file, string operBy)
    {
        if (employeeId == 0 || string.IsNullOrWhiteSpace(certName))
            throw new BusinessException("请填写员工和证书名称");

        string? filePath = null; string? fileName = null;
        if (file != null && file.Length > 0)
        {
            var saved = await FileUploadHelper.SaveUploadFile(file, "hr/certs");
            if (saved.HasValue) { filePath = saved.Value.path; fileName = saved.Value.name; }
        }

        var cert = new EmployeeCertificate
        {
            EmployeeId = employeeId,
            CertName   = certName,
            CertType   = certType,
            CertNo     = certNo,
            IssueOrg   = issueOrg,
            IssueDate  = issueDate,
            ExpireDate = expireDate,
            Status     = 0,
            FilePath   = filePath,
            FileName   = fileName,
            CreatedBy  = operBy,
        };
        await _uow.Certificates.AddAsync(cert);
        await _uow.SaveChangesAsync();
        return cert.Id;
    }

    public async Task<(string path, string name)?> UploadAsync(long id, IFormFile file, string operBy)
    {
        var cert = await _uow.Certificates.GetByIdAsync(id);
        if (cert == null) throw new NotFoundException("证书不存在");

        var result = await FileUploadHelper.SaveUploadFile(file, "hr/certs");
        if (result == null) throw new BusinessException("文件上传失败");

        cert.FilePath = result.Value.path;
        cert.FileName = result.Value.name;
        cert.UpdatedBy = operBy;
        _uow.Certificates.Update(cert);
        await _uow.SaveChangesAsync();
        return result;
    }

    public async Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id)
    {
        var c = await _uow.Certificates.GetByIdAsync(id);
        if (c?.FilePath == null) return null;
        return (c.FilePath, c.FileName ?? "证书附件");
    }

    public async Task DeleteAsync(long id)
    {
        var cert = await _uow.Certificates.GetByIdAsync(id);
        if (cert == null) throw new NotFoundException("证书不存在");
        if (cert.FilePath != null && System.IO.File.Exists(cert.FilePath))
            System.IO.File.Delete(cert.FilePath);
        _uow.Certificates.SoftDelete(cert);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(long id, string operBy)
        => await FileManageHelper.DeleteFileAsync(_uow.Certificates, () => _uow.SaveChangesAsync(), id, operBy);
}
