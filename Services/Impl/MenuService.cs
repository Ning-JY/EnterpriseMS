using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using EnterpriseMS.Web;

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
        var dtos = _mapper.Map<List<MenuTreeDto>>(all);
        // 数据库存的是 FontAwesome 名（如 fa-cogs），统一翻译为 layui 原生图标类
        foreach (var d in dtos) d.Icon = LayuiIcon.From(d.Icon);
        return BuildTree(dtos, 0);
    }

    public async Task<PagedResult<MenuListDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.Menus.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(m => m.MenuName.Contains(keyword) || (m.Perms != null && m.Perms.Contains(keyword)));
        var paged = await q.OrderBy(m => m.Sort).ToPagedAsync(page, size);

        var list = _mapper.Map<List<MenuListDto>>(paged.Items);
        var parentIds = list.Where(x => x.ParentId > 0).Select(x => x.ParentId).Distinct().ToList();
        if (parentIds.Any())
        {
            var parents = await _uow.Menus.GetListAsync(m => parentIds.Contains(m.Id));
            var nameMap = parents.ToDictionary(m => m.Id, m => m.MenuName);
            foreach (var item in list)
                if (item.ParentId > 0 && nameMap.TryGetValue(item.ParentId, out var pname))
                    item.ParentName = pname;
        }
        var allParentIds = (await _uow.Menus.Query().Select(m => m.ParentId).ToListAsync())
            .Where(pid => pid > 0).ToHashSet();
        foreach (var item in list)
            item.HasChildren = allParentIds.Contains(item.Id);
        return new PagedResult<MenuListDto>
        { Items = list, Total = paged.Total, Page = page, PageSize = size };
    }

    // 菜单树形表格：返回扁平全量列表（含 ParentId / HasChildren），由前端 layui table 的 tree 配置渲染层级
    public async Task<List<MenuListDto>> GetFlatListAsync(string? keyword)
    {
        var q = _uow.Menus.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(m => m.MenuName.Contains(keyword) || (m.Perms != null && m.Perms.Contains(keyword)));
        var list = _mapper.Map<List<MenuListDto>>(await q.OrderBy(m => m.Sort).ToListAsync());

        var idMap = list.ToDictionary(x => x.Id);
        // 上级菜单名（即使被 keyword 过滤，也尽量用列表内已知名称）
        foreach (var item in list)
            if (item.ParentId > 0 && idMap.TryGetValue(item.ParentId, out var p))
                item.ParentName = p.MenuName;

        // HasChildren：是否存在以本节点为父的节点
        var allParentIds = list.Select(x => x.ParentId).Where(pid => pid > 0).ToHashSet();
        foreach (var item in list)
            item.HasChildren = allParentIds.Contains(item.Id);

        // 数据库存的是 FontAwesome 名（如 fa-cogs），统一翻译为 layui 原生图标类
        foreach (var item in list) item.Icon = LayuiIcon.From(item.Icon);

        // 计算层级深度并按「父在前、子在后」的树形顺序返回（用于列表缩进展示）
        var childrenOf = list.Where(x => x.ParentId > 0)
                             .GroupBy(x => x.ParentId)
                             .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Sort).ToList());
        var roots = list.Where(x => x.ParentId == 0).OrderBy(x => x.Sort).ToList();
        var ordered = new List<MenuListDto>();
        void Walk(MenuListDto node, int depth)
        {
            node.Depth = depth;
            ordered.Add(node);
            if (childrenOf.TryGetValue(node.Id, out var kids))
                foreach (var k in kids) Walk(k, depth + 1);
        }
        foreach (var r in roots) Walk(r, 0);
        // 父节点不在列表内的孤点（如被 keyword 过滤掉父节点）兜底放在末尾
        foreach (var item in list)
            if (!ordered.Contains(item)) { item.Depth = 0; ordered.Add(item); }
        return ordered;
    }

    private List<MenuTreeDto> BuildTree(List<MenuTreeDto> all, long parentId)
        => all.BuildTree(parentId, m => m.Id, m => m.ParentId, (m, c) => m.Children = c);

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
