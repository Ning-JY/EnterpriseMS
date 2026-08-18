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
/// 合同管理服务实现。承接原 ContractController 的数据访问与文件上传，
/// 通过 FileUploadHelper 统一处理附件落盘，避免 Controller 直接碰 IUnitOfWork。
/// </summary>
public class ContractService : IContractService
{
    private readonly IUnitOfWork           _uow;
    private readonly IDictService          _dictSvc;
    private readonly IEmployeeQueryService _empQrySvc;

    public ContractService(IUnitOfWork uow, IDictService dictSvc,
        IEmployeeQueryService empQrySvc)
    {
        _uow = uow; _dictSvc = dictSvc; _empQrySvc = empQrySvc;
    }

    public async Task<PagedResult<EmployeeContract>> GetPagedAsync(
        string? keyword, int? status, int page, int size)
    {
        var q = _uow.Contracts.Query().Include(c => c.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => (c.Employee != null && c.Employee.RealName.Contains(keyword)) ||
                             c.ContractNo.Contains(keyword));
        if (status.HasValue) q = q.Where(c => c.Status == status);

        var warnDate  = DateTime.UtcNow.Date.AddDays(30);
        var warnCount = await q.CountAsync(c => c.Status == 0 && c.EndDate <= warnDate);
        var paged     = await q.OrderByDescending(c => c.CreatedAt).ToPagedAsync(page, size);
        paged.WarnCount = warnCount;
        return paged;
    }

    public Task<List<EmployeeSimpleDto>> GetEmployeesAsync()
        => _empQrySvc.GetAllOnJobAsync();

    public Task<List<DictDataDto>> GetContractTypesAsync()
        => _dictSvc.GetDataByTypeAsync(DictType.ContractType);

    public async Task<long> CreateWithFileAsync(long employeeId, string contractNo, string contractType,
        DateTime startDate, DateTime endDate, DateTime? signDate, string? remark,
        IFormFile? file, string operBy)
    {
        if (employeeId == 0 || string.IsNullOrWhiteSpace(contractNo))
            throw new BusinessException("请填写员工和合同编号");

        string? filePath = null; string? fileName = null;
        if (file != null && file.Length > 0)
        {
            var saved = await FileUploadHelper.SaveUploadFile(file, "hr/contracts");
            if (saved.HasValue) { filePath = saved.Value.path; fileName = saved.Value.name; }
        }

        var contract = new EmployeeContract
        {
            EmployeeId  = employeeId,
            ContractNo  = contractNo,
            ContractType = contractType,
            StartDate   = startDate,
            EndDate     = endDate,
            SignDate    = signDate,
            Status      = 0,
            FilePath    = filePath,
            FileName    = fileName,
            Remark      = remark,
            CreatedBy   = operBy,
        };
        await _uow.Contracts.AddAsync(contract);
        await _uow.SaveChangesAsync();
        return contract.Id;
    }

    public async Task<(string path, string name)?> UploadAsync(long id, IFormFile file, string operBy)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id);
        if (contract == null) throw new NotFoundException("合同不存在");

        var result = await FileUploadHelper.SaveUploadFile(file, "hr/contracts");
        if (result == null) throw new BusinessException("文件上传失败");

        contract.FilePath = result.Value.path;
        contract.FileName = result.Value.name;
        contract.UpdatedBy = operBy;
        _uow.Contracts.Update(contract);
        await _uow.SaveChangesAsync();
        return result;
    }

    public async Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id)
    {
        var c = await _uow.Contracts.GetByIdAsync(id);
        if (c?.FilePath == null) return null;
        return (c.FilePath, c.FileName ?? "合同附件");
    }

    public async Task UpdateAsync(ContractUpdateDto dto, string operBy)
    {
        var c = await _uow.Contracts.GetByIdAsync(dto.Id);
        if (c == null) throw new NotFoundException("合同不存在");
        c.ContractNo   = dto.ContractNo;
        c.ContractType = dto.ContractType;
        c.StartDate    = dto.StartDate;
        c.EndDate      = dto.EndDate;
        c.SignDate     = dto.SignDate;
        c.Status       = dto.Status;
        c.Remark       = dto.Remark;
        c.UpdatedBy    = operBy;
        _uow.Contracts.Update(c);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var ct = await _uow.Contracts.GetByIdAsync(id);
        if (ct == null) throw new NotFoundException("合同不存在");
        if (ct.FilePath != null && System.IO.File.Exists(ct.FilePath))
            System.IO.File.Delete(ct.FilePath);
        _uow.Contracts.SoftDelete(ct);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(long id, string operBy)
        => await FileManageHelper.DeleteFileAsync(_uow.Contracts, () => _uow.SaveChangesAsync(), id, operBy);

    public async Task TerminateAsync(long id, string operBy)
    {
        var c = await _uow.Contracts.GetByIdAsync(id);
        if (c == null) throw new NotFoundException("合同不存在");
        c.Status    = 1;
        c.UpdatedBy = operBy;
        _uow.Contracts.Update(c);
        await _uow.SaveChangesAsync();
    }
}
