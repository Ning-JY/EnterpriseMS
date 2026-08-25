using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPermissionCache _permCache;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork uow, IMapper mapper,
        IPermissionCache permCache, ILogger<UserService> logger)
    {
        _uow = uow; _mapper = mapper;
        _permCache = permCache; _logger = logger;
    }

    public async Task<PagedResult<UserListDto>> GetPagedAsync(UserQueryDto query)
    {
        var q = _uow.Users.Query()
            .Include(u => u.Dept)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(u => u.Username.Contains(query.Keyword) ||
                             u.RealName.Contains(query.Keyword) ||
                             (u.Phone != null && u.Phone.Contains(query.Keyword)));
        if (query.DeptId.HasValue) q = q.Where(u => u.DeptId == query.DeptId);
        if (query.Status.HasValue)  q = q.Where(u => u.Status == query.Status);

        var paged = await q.OrderByDescending(u => u.CreatedAt)
                           .ToPagedAsync(query.Page, query.Size);
        var list  = paged.Items;

        var dtos = _mapper.Map<List<UserListDto>>(list);

        // 批量加载绑定的员工姓名
        var empIds = list.Where(u => u.EmployeeId.HasValue).Select(u => u.EmployeeId!.Value).ToList();
        if (empIds.Any())
        {
            var empNames = await _uow.Employees.Query()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, e.RealName })
                .ToListAsync();
            var empDict = empNames.ToDictionary(e => e.Id, e => e.RealName);
            foreach (var dto in dtos)
            {
                var user = list.First(u => u.Id == dto.Id);
                if (user.EmployeeId.HasValue && empDict.TryGetValue(user.EmployeeId.Value, out var empName))
                    dto.EmployeeName = empName;
            }
        }

        return new PagedResult<UserListDto>
        {
            Items    = dtos,
            Total    = paged.Total,
            Page     = query.Page,
            PageSize = query.Size,
        };
    }

    public async Task<UserDetailDto?> GetDetailAsync(long id)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.Dept)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;
        var dto = _mapper.Map<UserDetailDto>(user);
        dto.EmployeeId = user.EmployeeId;
        dto.DeptId     = user.DeptId;   // 修复：编辑表单需回填部门（AutoMapper 仅映射 DeptName 而非原始 Id）
        dto.Remark     = user.Remark;   // 修复：编辑表单需回填备注
        // 如果绑定了员工，查出姓名供前端显示
        if (user.EmployeeId.HasValue)
        {
            var emp = await _uow.Employees.GetByIdAsync(user.EmployeeId.Value);
            dto.EmployeeName = emp?.RealName;
        }
        return dto;
    }

    public async Task<SysUser?> GetByUsernameAsync(string username)
        => await _uow.Users.Query(false)
                           .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<long> CreateAsync(CreateUserDto dto, string operBy)
    {
        if (await _uow.Users.AnyAsync(u => u.Username == dto.Username))
            throw new BusinessException("用户名已存在");
        // 同一员工不能被两个账号绑定
        if (dto.EmployeeId.HasValue &&
            await _uow.Users.AnyAsync(u => u.EmployeeId == dto.EmployeeId))
            throw new BusinessException("该员工已绑定其他账号，请先解绑");

        var user = new SysUser
        {
            Username     = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
            RealName     = dto.RealName,
            Phone        = dto.Phone,
            DeptId       = dto.DeptId,
            Remark       = dto.Remark,
            EmployeeId   = dto.EmployeeId,   // 仅作交叉引用指针，不自动同步档案
            CreatedBy    = operBy,
        };
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();
        if (dto.RoleIds.Any())
            await AssignRolesAsync(user.Id, dto.RoleIds);
        _logger.LogInformation("创建用户 {Username} by {OperBy}", dto.Username, operBy);
        return user.Id;
    }

    public async Task UpdateAsync(UpdateUserDto dto, string operBy)
    {
        var user = await _uow.Users.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("用户不存在");

        // 校验员工绑定唯一性（同一员工不能被两个账号绑定）
        if (dto.EmployeeId.HasValue)
        {
            var conflict = await _uow.Users.AnyAsync(
                u => u.EmployeeId == dto.EmployeeId && u.Id != dto.Id);
            if (conflict)
                throw new BusinessException("该员工已绑定其他账号，请先解绑");
        }

        user.RealName   = dto.RealName;
        user.Phone      = dto.Phone;
        user.DeptId     = dto.DeptId;
        user.Remark     = dto.Remark;
        user.EmployeeId = dto.EmployeeId;   // 允许 null（解绑）
        user.UpdatedBy  = operBy;

        _uow.Users.Update(user);
        // 仅当选择了角色时才覆盖；空列表表示“保留原角色”，不清空
        if (dto.RoleIds.Any())
            await AssignRolesAsync(dto.Id, dto.RoleIds);
        await _uow.SaveChangesAsync();
        await _permCache.RemoveUserPermsAsync(dto.Id);
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("用户不存在");
        if (user.Username == "admin")
            throw new BusinessException("超级管理员不可删除");
        user.EmployeeId = null;   // 释放员工唯一索引，允许该员工被其他账号重新绑定
        user.UpdatedBy  = operBy;
        _uow.Users.SoftDelete(user);
        await _uow.SaveChangesAsync();
        await _permCache.RemoveUserPermsAsync(id);
    }

    public async Task SetStatusAsync(long id, int status, string operBy)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("用户不存在");
        if (user.Username == "admin" && status == 0)
            throw new BusinessException("超级管理员不可禁用");
        user.Status    = status;
        user.UpdatedBy = operBy;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        if (status == 0) await _permCache.RemoveUserPermsAsync(id);
    }

    public async Task ResetPasswordAsync(long id, string newPwd, string operBy)
    {
        if (string.IsNullOrWhiteSpace(newPwd) || newPwd.Length < 6)
            throw new BusinessException("密码长度不能少于6位");
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("用户不存在");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd, 12);
        user.UpdatedBy    = operBy;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(long id, string oldPwd, string newPwd)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("用户不存在");
        if (!BCrypt.Net.BCrypt.Verify(oldPwd, user.PasswordHash))
            throw new BusinessException("原密码错误");
        if (newPwd.Length < 6)
            throw new BusinessException("新密码长度不能少于6位");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd, 12);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    public async Task AssignRolesAsync(long userId, List<long> roleIds)
    {
        var old = await _uow.UserRoles.GetListAsync(r => r.UserId == userId);
        _uow.UserRoles.RemoveRange(old);

        if (roleIds.Any())
        {
            var newRoles = roleIds.Distinct()
                .Select(rid => new SysUserRole { UserId = userId, RoleId = rid });
            await _uow.UserRoles.AddRangeAsync(newRoles);
        }
        await _uow.SaveChangesAsync();
        await _permCache.RemoveUserPermsAsync(userId);
        await _permCache.RemoveUserMenuIdsAsync(userId);
    }

    public async Task UpdateLastLoginAsync(long id, string? ip = null)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return;
        user.LastLoginTime = DateTime.UtcNow;
        user.LastLoginIp   = ip;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    // 登录时根据用户ID取角色代码集合（原在 AccountController 内直查 SysUserRoles/SysRoles）
    public async Task<List<string>> GetRoleCodesAsync(long userId)
    {
        return await _uow.UserRoles.Query()
            .Where(ur => ur.UserId == userId)
            .Join(_uow.Roles.Query(),
                ur => ur.RoleId, r => r.Id,
                (ur, r) => r.RoleCode)
            .ToListAsync();
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password)
    {
        var user = await GetByUsernameAsync(username);
        if (user == null || user.Status == 0) return false;
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<List<UserListDto>> GetAllActiveAsync()
    {
        var list = await _uow.Users.GetListAsync(u => u.Status == 1);
        return _mapper.Map<List<UserListDto>>(list);
    }
}
