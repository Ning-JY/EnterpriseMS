using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IDeptService
{
    Task<List<DeptTreeDto>>  GetTreeAsync();
    Task<DeptTreeDto?>       GetByIdAsync(long id);
    Task<long>               CreateAsync(CreateDeptDto dto, string operBy);
    Task                     UpdateAsync(UpdateDeptDto dto, string operBy);
    Task                     DeleteAsync(long id);
    Task<List<DeptTreeDto>>  GetChildrenAsync(long parentId);
}
