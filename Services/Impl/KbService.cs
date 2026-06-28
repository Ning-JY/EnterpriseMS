using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Kb;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class KbService : IKbService
{
    public Task<PagedResult<KbFileDto>> GetPagedAsync(KbQueryDto query, bool adminView = false)
    {
        return Task.FromResult(new PagedResult<KbFileDto>());
    }

    public Task<KbFileDto?> GetDetailAsync(long id)
    {
        return Task.FromResult<KbFileDto?>(null);
    }

    public Task<long> UploadAsync(KbUploadDto dto, string operBy)
    {
        return Task.FromResult(0L);
    }

    public Task UpdateAsync(KbUpdateDto dto, string operBy)
    {
        return Task.CompletedTask;
    }

    public Task TogglePinAsync(long id, string operBy)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long id, string operBy)
    {
        return Task.CompletedTask;
    }

    public Task<(string path, string name, string mime, long size)?> GetDownloadInfoAsync(long id)
    {
        return Task.FromResult<(string, string, string, long)?>(null);
    }

    public Task<List<KbCategoryDto>> GetCategoriesAsync()
    {
        return Task.FromResult(new List<KbCategoryDto>());
    }
}
