using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IDictService
{
    Task<PagedResult<DictTypeDto>> GetPagedAsync(string? keyword, int page, int size);
    Task<List<DictDataDto>> GetDataByTypeAsync(string dictType);
    /// <summary>字典项管理用：返回该类型下全部字典项（含停用）。</summary>
    Task<List<DictDataDto>> GetDataListAsync(string dictType, string? keyword = null);
    Task<DictDataDto?> GetDataByIdAsync(long id);
    Task<List<DictTypeDto>> GetAllTypesAsync();
    Task<DictTypeDto?> GetByIdAsync(long id);
    Task<long> CreateTypeAsync(string dictName, string dictType, int status = 1, string? remark = null);
    Task UpdateTypeAsync(long id, string dictName, string dictType, int status, string? remark = null);
    Task DeleteTypeAsync(long id);
    Task<long> CreateDataAsync(string dictType, string dictLabel, string dictValue, int sort = 0, int isDefault = 0, int status = 1);
    Task UpdateDataAsync(long id, string dictLabel, string dictValue, int sort, int isDefault, int status);
    Task DeleteDataAsync(long id);
}
