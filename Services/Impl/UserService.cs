using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Infrastructure.Data;
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
    private readonly AppDbContext _db;

    public UserService(IUnitOfWork uow, IMapper mapper,
        IPermissionCache permCache, ILogger<UserService> logger,
        AppDbContext db)
    {
        _uow = uow; _mapper = mapper;
        _permCache = permCache; _logger = logger;
        _db = db;
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

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(u => u.CreatedAt)
                           .Skip((query.Page - 1) * query.Size).Take(query.Size)
                           .ToListAsync();
        return new PagedResult<UserListDto>
        {
            Items    = _mapper.Map<List<UserListDto>>(list),
            Total    = total,
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

        var user = new SysUser
        {
            Username     = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
            RealName     = dto.RealName,
            Phone        = dto.Phone,
            Email        = dto.Email,
            DeptId       = dto.DeptId,
            PostId       = dto.PostId,
            Remark       = dto.Remark,
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

        user.EmployeeId = dto.EmployeeId;  // 允许 null（解绑）
        user.Remark     = dto.Remark;
        user.UpdatedBy  = operBy;

        if (dto.EmployeeId.HasValue)
        {
            // 绑定了员工档案：从员工档案同步基本信息，确保一致性
            var emp = await _uow.Employees.GetByIdAsync(dto.EmployeeId.Value);
            if (emp != null)
            {
                user.RealName = emp.RealName;
                user.Phone    = emp.Phone;
                user.Email    = emp.Email;
                user.DeptId   = emp.DeptId;
                user.PostId   = emp.PostId;
            }
        }
        else
        {
            // 未绑定员工：允许手动维护
            user.RealName = dto.RealName;
            user.Phone    = dto.Phone;
            user.Email    = dto.Email;
            user.DeptId   = dto.DeptId;
            user.PostId   = dto.PostId;
        }
        _uow.Users.Update(user);
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
        user.UpdatedBy = operBy;
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
        // SysUserRole 无软删除，直接用 DbContext 操作
        var old = await _db.SysUserRoles
            .Where(r => r.UserId == userId)
            .ToListAsync();
        _db.SysUserRoles.RemoveRange(old);

        if (roleIds.Any())
        {
            var newRoles = roleIds.Distinct()
                .Select(rid => new EnterpriseMS.Domain.Entities.System.SysUserRole
                    { UserId = userId, RoleId = rid });
            await _db.SysUserRoles.AddRangeAsync(newRoles);
        }
        await _db.SaveChangesAsync();
        await _permCache.RemoveUserPermsAsync(userId);
        await _permCache.RemoveUserMenuIdsAsync(userId);
    }

    public async Task UpdateLastLoginAsync(long id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return;
        user.LastLoginTime = DateTime.Now;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
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
