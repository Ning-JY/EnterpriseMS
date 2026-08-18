using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>员工附件服务实现。落盘走统一的 FileUploadHelper，与合同/证书保持一致。</summary>
public class EmployeeAttachmentService : IEmployeeAttachmentService
{
    private readonly IUnitOfWork _uow;

    public EmployeeAttachmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EmployeeAttachment>> GetListAsync(long employeeId)
    {
        var list = await _uow.Attachments.GetListAsync(a => a.EmployeeId == employeeId);
        return list.OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<long> UploadAsync(long employeeId, IFormFile file, string? remark, string operBy)
    {
        if (employeeId == 0) throw new BusinessException("请先指定员工");
        var saved = await FileUploadHelper.SaveUploadFile(file, "hr/attachments");
        if (saved == null) throw new BusinessException("文件上传失败（格式或大小不合规）");

        var attach = new EmployeeAttachment
        {
            EmployeeId = employeeId,
            FileName   = saved.Value.name,
            FilePath   = saved.Value.path,
            FileSize   = file.Length,
            FileType   = System.IO.Path.GetExtension(file.FileName),
            Remark     = remark,
            CreatedBy  = operBy
        };
        await _uow.Attachments.AddAsync(attach);
        await _uow.SaveChangesAsync();
        return attach.Id;
    }

    public async Task DeleteAsync(long id)
    {
        var a = await _uow.Attachments.GetByIdAsync(id);
        if (a == null) throw new NotFoundException("附件不存在");
        if (a.FilePath != null && System.IO.File.Exists(a.FilePath))
            System.IO.File.Delete(a.FilePath);
        _uow.Attachments.SoftDelete(a);
        await _uow.SaveChangesAsync();
    }

    public async Task<(string Path, string FileName)?> GetDownloadInfoAsync(long id)
    {
        var a = await _uow.Attachments.GetByIdAsync(id);
        if (a?.FilePath == null) return null;
        return (a.FilePath, a.FileName ?? "附件");
    }
}
