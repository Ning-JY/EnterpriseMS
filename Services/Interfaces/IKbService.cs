using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.Kb;

namespace EnterpriseMS.Services.Interfaces;

// ── 知识库服务 ─────────────────────────────────────────────
public interface IKbService
{
    Task<PagedResult<KbFileDto>>  GetPagedAsync(KbQueryDto query, bool adminView = false);
    Task<KbFileDto?>              GetDetailAsync(long id);
    Task<long>                    UploadAsync(KbUploadDto dto, string operBy);
    Task                          UpdateAsync(KbUpdateDto dto, string operBy);
    Task                          TogglePinAsync(long id, string operBy);
    Task                          DeleteAsync(long id, string operBy);
    Task<(string path, string name, string mime, long size)?> GetDownloadInfoAsync(long id);
    Task<List<KbCategoryDto>>     GetCategoriesAsync();
}
