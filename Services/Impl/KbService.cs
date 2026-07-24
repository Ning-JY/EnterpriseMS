using System.IO;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Kb;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

// 知识库服务实现。承载原 KbController 直接通过 IUnitOfWork 做的写操作（上传/置顶/删除/下载计数），
// 使 Controller 仅做参数校验与路由分发，消除 Controller 越层直接 SaveChanges 的反模式（审计 2.3）。
public class KbService : IKbService
{
    private readonly IUnitOfWork    _uow;
    private readonly IOperLogService _logSvc;

    public KbService(IUnitOfWork uow, IOperLogService logSvc)
    {
        _uow = uow; _logSvc = logSvc;
    }

    public Task<PagedResult<KbFileDto>> GetPagedAsync(KbQueryDto query, bool adminView = false)
        => Task.FromResult(new PagedResult<KbFileDto>());

    public Task<KbFileDto?> GetDetailAsync(long id)
        => Task.FromResult<KbFileDto?>(null);

    public async Task<long> UploadAsync(KbUploadDto dto, string operBy)
    {
        var saved = await FileUploadHelper.SaveUploadFile(dto.File, $"kb/{dto.CategoryId}");
        if (!saved.HasValue)
            throw new BusinessException("不支持的文件类型");

        var category = await _uow.KbCategories.GetByIdAsync(dto.CategoryId ?? 0);
        if (category == null) throw new BusinessException("分类不存在");

        var kbFile = new KbFile
        {
            CategoryId   = dto.CategoryId ?? 0,
            FileName     = string.IsNullOrWhiteSpace(dto.DisplayName)
                           ? Path.GetFileNameWithoutExtension(dto.File.FileName) : dto.DisplayName!,
            OriginalName = dto.File.FileName,
            FilePath     = saved.Value.path,
            FileSize     = dto.File.Length,
            FileExt      = Path.GetExtension(dto.File.FileName).TrimStart('.').ToLower(),
            Description  = dto.Description,
            Version      = dto.Version,
            IsPinned     = dto.IsPinned,
            Status       = 1,
            CreatedBy    = operBy,
        };
        await _uow.KbFiles.AddAsync(kbFile);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("上传知识库文件", $"[{category.Name}] {kbFile.FileName}", "INSERT", kbFile.Id);
        return kbFile.Id;
    }

    public Task UpdateAsync(KbUpdateDto dto, string operBy) => Task.CompletedTask;

    public async Task TogglePinAsync(long id, string operBy)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) throw new BusinessException("文件不存在");
        f.IsPinned  = !f.IsPinned;
        f.UpdatedBy = operBy;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) throw new BusinessException("文件不存在");
        _uow.KbFiles.SoftDelete(f);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("删除知识库文件", f.FileName, "DELETE", id);
    }

    public async Task IncrementDownloadCountAsync(long id)
    {
        var f = await _uow.KbFiles.GetByIdAsync(id);
        if (f == null) return;
        f.DownloadCount++;
        _uow.KbFiles.Update(f);
        await _uow.SaveChangesAsync();
    }

    public Task<(string path, string name, string mime, long size)?> GetDownloadInfoAsync(long id)
        => Task.FromResult<(string, string, string, long)?>(null);

    public Task<List<KbCategoryDto>> GetCategoriesAsync()
        => Task.FromResult(new List<KbCategoryDto>());
}
