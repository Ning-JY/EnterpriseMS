using EnterpriseMS.Common;
using EnterpriseMS.Services.DTOs.System;

namespace EnterpriseMS.Services.Interfaces;

public interface IMenuService
{
    Task<PagedResult<MenuListDto>> GetPagedAsync(string? keyword, int page, int size);
    Task<List<MenuListDto>> GetFlatListAsync(string? keyword);
    Task<List<MenuTreeDto>>  GetTreeAsync(bool onlyVisible = false);
    Task<MenuTreeDto?>       GetByIdAsync(long id);
    Task<long>               CreateAsync(CreateMenuDto dto, string operBy);
    Task                     UpdateAsync(UpdateMenuDto dto, string operBy);
    Task                     DeleteAsync(long id);
}
