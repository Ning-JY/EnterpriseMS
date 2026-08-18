using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permSvc;

    public RoleService(IUnitOfWork uow, IMapper mapper, IPermissionService permSvc)
    { _uow = uow; _mapper = mapper; _permSvc = permSvc; }

    public async Task<PagedResult<RoleListDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.Roles.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(r => r.RoleName.Contains(keyword) || r.RoleCode.Contains(keyword));
        var paged = await q.OrderBy(r => r.Sort).ToPagedAsync(page, size);
        return new PagedResult<RoleListDto>
        { Items = _mapper.Map<List<RoleListDto>>(paged.Items), Total = paged.Total, Page = page, PageSize = size };
    }

    public async Task<List<RoleListDto>> GetAllActiveAsync()
    {
        var list = await _uow.Roles.GetListAsync(r => r.Status == 1);
        return _mapper.Map<List<RoleListDto>>(list);
    }

    public async Task<RoleListDto?> GetByIdAsync(long id)
    {
        var r = await _uow.Roles.GetByIdAsync(id);
        return r == null ? null : _mapper.Map<RoleListDto>(r);
    }

    public async Task<long> CreateAsync(CreateRoleDto dto, string operBy)
    {
        if (await _uow.Roles.AnyAsync(r => r.RoleCode == dto.RoleCode))
            throw new BusinessException("角色编码已存在");
        var role = new SysRole
        {
            RoleName = dto.RoleName, RoleCode = dto.RoleCode,
            DataScope = dto.DataScope, Sort = dto.Sort,
            Status = dto.Status, Remark = dto.Remark, CreatedBy = operBy,
        };
        await _uow.Roles.AddAsync(role);
        await _uow.SaveChangesAsync();
        if (dto.MenuIds.Any()) await AssignMenusAsync(role.Id, dto.MenuIds);
        return role.Id;
    }

    public async Task UpdateAsync(UpdateRoleDto dto, string operBy)
    {
        var role = await _uow.Roles.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("角色不存在");
        role.RoleName  = dto.RoleName;
        role.DataScope = dto.DataScope;
        role.Sort      = dto.Sort;
        role.Status    = dto.Status;
        role.Remark    = dto.Remark;
        role.UpdatedBy = operBy;
        _uow.Roles.Update(role);
        // 注意：菜单分配交由独立的 AssignMenus（分配权限页）处理，
        // 此处不可重设 MenuIds，否则编辑角色基本信息会把已分配菜单清空。
        await _uow.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(dto.Id);
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var role = await _uow.Roles.GetByIdAsync(id)
            ?? throw new NotFoundException("角色不存在");
        if (role.RoleCode == "superadmin")
            throw new BusinessException("超级管理员角色不可删除");
        _uow.Roles.SoftDelete(role);
        await _uow.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(id);
    }

    public async Task SetStatusAsync(long id, int status)
    {
        var role = await _uow.Roles.GetByIdAsync(id)
            ?? throw new NotFoundException("角色不存在");
        role.Status = status;
        _uow.Roles.Update(role);
        await _uow.SaveChangesAsync();
    }

    public async Task AssignMenusAsync(long roleId, List<long> menuIds)
    {
        var old = await _uow.RoleMenus.GetListAsync(rm => rm.RoleId == roleId);
        _uow.RoleMenus.RemoveRange(old);
        var news = menuIds.Distinct().Select(mid => new SysRoleMenu { RoleId = roleId, MenuId = mid });
        await _uow.RoleMenus.AddRangeAsync(news);
        await _uow.SaveChangesAsync();
        await _permSvc.ClearRoleUsersCacheAsync(roleId);
    }

    public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
        => await _uow.RoleMenus.Query()
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId).ToListAsync();
}
