using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class DeptService : IDeptService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DeptService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    public async Task<List<DeptTreeDto>> GetTreeAsync()
    {
        var all = await _uow.Depts.Query().OrderBy(d => d.Sort).ToListAsync();
        return BuildTree(_mapper.Map<List<DeptTreeDto>>(all), 0);
    }

    public async Task<PagedResult<DeptListDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.Depts.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(d => d.DeptName.Contains(keyword) || (d.Leader != null && d.Leader.Contains(keyword)));
        var paged = await q.OrderBy(d => d.Sort).ToPagedAsync(page, size);

        var list = _mapper.Map<List<DeptListDto>>(paged.Items);
        // 填充上级部门名称
        var parentIds = list.Where(x => x.ParentId > 0).Select(x => x.ParentId).Distinct().ToList();
        if (parentIds.Any())
        {
            var parents = await _uow.Depts.GetListAsync(d => parentIds.Contains(d.Id));
            var nameMap = parents.ToDictionary(d => d.Id, d => d.DeptName);
            foreach (var item in list)
                if (item.ParentId > 0 && nameMap.TryGetValue(item.ParentId, out var pname))
                    item.ParentName = pname;
        }
        // 标记是否存在子部门（前端据此隐藏「删除」按钮，避免误删父级）
        var allParentIds = (await _uow.Depts.Query().Select(d => d.ParentId).ToListAsync())
            .Where(pid => pid > 0).ToHashSet();
        foreach (var item in list)
            item.HasChildren = allParentIds.Contains(item.Id);
        return new PagedResult<DeptListDto>
        { Items = list, Total = paged.Total, Page = page, PageSize = size };
    }

    private List<DeptTreeDto> BuildTree(List<DeptTreeDto> all, long parentId)
        => all.BuildTree(parentId, d => d.Id, d => d.ParentId, (d, c) => d.Children = c);

    public async Task<DeptTreeDto?> GetByIdAsync(long id)
    {
        var d = await _uow.Depts.GetByIdAsync(id);
        return d == null ? null : _mapper.Map<DeptTreeDto>(d);
    }

    public async Task<List<DeptTreeDto>> GetChildrenAsync(long parentId)
    {
        var list = await _uow.Depts.GetListAsync(d => d.ParentId == parentId);
        return _mapper.Map<List<DeptTreeDto>>(list);
    }

    public async Task<long> CreateAsync(CreateDeptDto dto, string operBy)
    {
        var parent = await _uow.Depts.GetByIdAsync(dto.ParentId);
        var ancestors = parent == null ? "0" : parent.Ancestors + "," + parent.Id;
        var dept = new SysDept
        {
            DeptName  = dto.DeptName, ParentId = dto.ParentId,
            Ancestors = ancestors, Leader = dto.Leader,
            Phone = dto.Phone, Sort = dto.Sort, Status = dto.Status, CreatedBy = operBy,
        };
        await _uow.Depts.AddAsync(dept);
        await _uow.SaveChangesAsync();
        return dept.Id;
    }

    public async Task UpdateAsync(UpdateDeptDto dto, string operBy)
    {
        var dept = await _uow.Depts.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("部门不存在");
        var oldParentId = dept.ParentId;
        dept.DeptName  = dto.DeptName;
        dept.ParentId  = dto.ParentId;
        dept.Leader    = dto.Leader;
        dept.Phone     = dto.Phone;
        dept.Sort      = dto.Sort;
        dept.Status    = dto.Status;
        dept.UpdatedBy = operBy;
        // 上级部门变化时同步重算祖先链，保持树结构正确
        if (oldParentId != dto.ParentId)
        {
            var parent = await _uow.Depts.GetByIdAsync(dto.ParentId);
            dept.Ancestors = parent == null ? "0" : parent.Ancestors + "," + parent.Id;
        }
        _uow.Depts.Update(dept);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (await _uow.Depts.AnyAsync(d => d.ParentId == id))
            throw new BusinessException("存在子部门，不可删除");
        if (await _uow.Users.AnyAsync(u => u.DeptId == id))
            throw new BusinessException("部门下存在用户，不可删除");
        var dept = await _uow.Depts.GetByIdAsync(id)
            ?? throw new NotFoundException("部门不存在");
        _uow.Depts.SoftDelete(dept);
        await _uow.SaveChangesAsync();
    }
}
