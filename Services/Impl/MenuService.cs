using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public MenuService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<List<MenuTreeDto>> GetTreeAsync(bool onlyVisible = false)
    {
        var q = _uow.Menus.Query();
        if (onlyVisible) q = q.Where(m => m.Visible == 1 && m.Status == 1);
        var all = await q.OrderBy(m => m.Sort).ToListAsync();
        return BuildTree(_mapper.Map<List<MenuTreeDto>>(all), 0);
    }

    private List<MenuTreeDto> BuildTree(List<MenuTreeDto> all, long parentId)
        => all.Where(m => m.ParentId == parentId)
              .Select(m => { m.Children = BuildTree(all, m.Id); return m; }).ToList();

    public async Task<MenuTreeDto?> GetByIdAsync(long id)
    {
        var m = await _uow.Menus.GetByIdAsync(id);
        return m == null ? null : _mapper.Map<MenuTreeDto>(m);
    }

    public async Task<long> CreateAsync(CreateMenuDto dto, string operBy)
    {
        var menu = _mapper.Map<SysMenu>(dto);
        menu.CreatedBy = operBy;
        await _uow.Menus.AddAsync(menu);
        await _uow.SaveChangesAsync();
        return menu.Id;
    }

    public async Task UpdateAsync(UpdateMenuDto dto, string operBy)
    {
        var menu = await _uow.Menus.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("菜单不存在");
        menu.MenuName  = dto.MenuName;
        menu.ParentId  = dto.ParentId;
        menu.MenuType  = dto.MenuType;
        menu.Perms     = dto.Perms;
        menu.Icon      = dto.Icon;
        menu.Path      = dto.Path;
        menu.Component = dto.Component;
        menu.Sort      = dto.Sort;
        menu.Visible   = dto.Visible;
        menu.UpdatedBy = operBy;
        _uow.Menus.Update(menu);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (await _uow.Menus.AnyAsync(m => m.ParentId == id))
            throw new BusinessException("存在子菜单，不可删除");
        var menu = await _uow.Menus.GetByIdAsync(id)
            ?? throw new NotFoundException("菜单不存在");
        _uow.Menus.SoftDelete(menu);
        await _uow.SaveChangesAsync();
    }
}
