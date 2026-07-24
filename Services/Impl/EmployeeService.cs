using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 员工档案服务实现。承接原 EmployeeController 的数据访问与业务编排，
/// 使 Controller 不再直接依赖 IUnitOfWork，数据访问统一收敛到 Service 层。
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork     _uow;
    private readonly IOperLogService _logSvc;

    public EmployeeService(IUnitOfWork uow, IOperLogService logSvc)
    {
        _uow = uow; _logSvc = logSvc;
    }

    public async Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryDto query)
    {
        var q = _uow.Employees.Query().Include(e => e.Dept).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(e => e.RealName.Contains(query.Keyword) ||
                             e.EmpNo.Contains(query.Keyword) ||
                             (e.Phone != null && e.Phone.Contains(query.Keyword)));
        if (query.DeptId.HasValue) q = q.Where(e => e.DeptId == query.DeptId);
        if (query.Status.HasValue)  q = q.Where(e => e.Status == query.Status);

        return await q.OrderByDescending(e => e.CreatedAt)
                      .ToPagedAsync(query.Page, query.Size);
    }

    /// <summary>已绑定登录账号的员工ID列表（用于列表标记）</summary>
    public async Task<List<long>> GetBoundEmployeeIdsAsync()
        => await _uow.Users.Query()
            .Where(u => u.EmployeeId.HasValue)
            .Select(u => u.EmployeeId!.Value)
            .ToListAsync();

    public async Task<Employee?> GetDetailAsync(long id)
        => await _uow.Employees.Query()
            .Include(e => e.Dept)
            .Include(e => e.Contracts)
            .Include(e => e.Certificates)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<SysPost>> GetPostsAsync()
        => await _uow.Posts.GetListAsync();

    public async Task<Employee?> GetByIdAsync(long id)
        => await _uow.Employees.Query()
            .Include(e => e.Dept)
            .FirstOrDefaultAsync(e => e.Id == id);

    /// <summary>在职员工（状态0试用/1在职），供下拉选项使用</summary>
    public async Task<List<Employee>> GetOnJobAsync()
        => await _uow.Employees.Query()
            .Include(e => e.Dept)
            .Where(e => e.Status == 0 || e.Status == 1)
            .OrderBy(e => e.Dept != null ? e.Dept.Sort : 999)
            .ThenBy(e => e.RealName)
            .ToListAsync();

    public async Task<long> CreateAsync(CreateEmployeeDto dto, string operBy)
    {
        if (string.IsNullOrWhiteSpace(dto.RealName))
            throw new BusinessException("姓名不能为空");

        // 工号：年份 + 雪花ID后6位，避免并发冲突
        var snowId = SnowflakeId.Next();
        var emp = new Employee
        {
            EmpNo    = $"EMP{DateTime.UtcNow.Year}{snowId % 100000:D5}",
            RealName = dto.RealName,
            Gender   = dto.Gender,
            Phone    = dto.Phone,
            Email    = dto.Email,
            IdCard   = dto.IdCard,
            DeptId   = dto.DeptId,
            PostId   = dto.PostId,
            Status   = 0,
            EntryDate        = dto.EntryDate,
            ProbationEndDate = dto.ProbationEndDate,
            Remark   = dto.Remark,
            CreatedBy = operBy,
        };
        await _uow.Employees.AddAsync(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("新增员工", $"姓名：{emp.RealName}，工号：{emp.EmpNo}", "INSERT", emp.Id);
        return emp.Id;
    }

    public async Task UpdateAsync(UpdateEmployeeDto dto, string operBy)
    {
        var emp = await _uow.Employees.GetByIdAsync(dto.Id);
        if (emp == null) throw new NotFoundException("员工不存在");

        emp.RealName         = dto.RealName;
        emp.Gender           = dto.Gender;
        emp.Phone            = dto.Phone;
        emp.Email            = dto.Email;
        emp.IdCard           = dto.IdCard;
        emp.DeptId           = dto.DeptId;
        emp.PostId           = dto.PostId;
        emp.EntryDate        = dto.EntryDate;
        emp.ProbationEndDate = dto.ProbationEndDate;
        emp.Remark           = dto.Remark;
        emp.UpdatedBy        = operBy;
        _uow.Employees.Update(emp);

        // 反向同步：若该员工已绑定登录账号，同步更新账号基本信息保持一致
        var boundUser = await _uow.Users.Query(false)
            .FirstOrDefaultAsync(u => u.EmployeeId == emp.Id);
        if (boundUser != null)
        {
            boundUser.RealName  = emp.RealName;
            boundUser.Phone     = emp.Phone;
            boundUser.Email     = emp.Email;
            boundUser.DeptId    = emp.DeptId;
            boundUser.PostId    = emp.PostId;
            boundUser.UpdatedBy = operBy;
            _uow.Users.Update(boundUser);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task FormalAsync(long id, DateTime formalDate, string operBy)
    {
        var emp = await _uow.Employees.GetByIdAsync(id);
        if (emp == null) throw new NotFoundException("员工不存在");
        emp.Status      = 1;
        emp.FormalDate  = formalDate;
        emp.UpdatedBy   = operBy;
        _uow.Employees.Update(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("员工转正", $"{emp.RealName} 转正日期：{formalDate:yyyy-MM-dd}", "UPDATE", id);
    }

    public async Task LeaveAsync(long id, DateTime leaveDate, string? reason, string operBy)
    {
        var emp = await _uow.Employees.GetByIdAsync(id);
        if (emp == null) throw new NotFoundException("员工不存在");
        emp.Status    = 2;
        emp.LeaveDate = leaveDate;
        emp.Remark    = string.IsNullOrWhiteSpace(reason) ? emp.Remark : $"离职原因：{reason}";
        emp.UpdatedBy = operBy;
        _uow.Employees.Update(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("员工离职", $"{emp.RealName} 离职日期：{leaveDate:yyyy-MM-dd}", "UPDATE", id);
    }
}
