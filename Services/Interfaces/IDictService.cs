using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IDictService
{
    Task<List<DictDataDto>> GetDataByTypeAsync(string dictType);
    Task<List<DictTypeDto>> GetAllTypesAsync();
    Task<long> CreateTypeAsync(string dictName, string dictType, int status = 1, string? remark = null);
    Task UpdateTypeAsync(long id, string dictName, string dictType, int status, string? remark = null);
    Task DeleteTypeAsync(long id);
    Task<long> CreateDataAsync(string dictType, string dictLabel, string dictValue, int sort = 0, int isDefault = 0, int status = 1);
    Task UpdateDataAsync(long id, string dictLabel, string dictValue, int sort, int isDefault, int status);
    Task DeleteDataAsync(long id);
}
