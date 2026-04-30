using AutoMapper;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Enums;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IUnitOfWork uow, IMapper mapper, ILogger<EmployeeService> logger)
    { _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeeQueryDto query)
    {
        var q = _uow.Employees.Query()
            .Include(e => e.Dept)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(e => e.RealName.Contains(query.Keyword)
                           || e.EmpNo.Contains(query.Keyword)
                           || (e.Phone != null && e.Phone.Contains(query.Keyword)));
        if (query.DeptId.HasValue)
            q = q.Where(e => e.DeptId == query.DeptId);
        if (query.Status.HasValue)
            q = q.Where(e => e.Status == query.Status.Value);

        var total = await q.CountAsync();
        var list = await q.OrderByDescending(e => e.CreatedAt)
                          .Skip((query.Page - 1) * query.Size).Take(query.Size)
                          .ToListAsync();

        return new PagedResult<EmployeeListDto>
        {
            Items = _mapper.Map<List<EmployeeListDto>>(list),
            Total = total,
            Page = query.Page,
            PageSize = query.Size
        };
    }

    public async Task<EmployeeDetailDto?> GetDetailAsync(long id)
    {
        var emp = await _uow.Employees.Query(false)
            .Include(e => e.Dept)
            .Include(e => e.Contracts)
            .Include(e => e.Certificates)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emp == null) return null;

        var dto = _mapper.Map<EmployeeDetailDto>(emp);
        dto.Contracts = _mapper.Map<List<EmployeeContractDto>>(
            emp.Contracts.OrderByDescending(c => c.StartDate).ToList());
        dto.Certificates = _mapper.Map<List<EmployeeCertificateDto>>(
            emp.Certificates.OrderByDescending(c => c.IssueDate).ToList());
        return dto;
    }

    public async Task<long> CreateAsync(CreateEmployeeDto dto, string operBy)
    {
        var emp = _mapper.Map<Employee>(dto);
        emp.EmpNo = await GenerateEmpNoAsync();
        emp.Status = 0; // 试用期
        emp.CreatedBy = operBy;
        emp.CreatedAt = DateTime.Now;

        await _uow.Employees.AddAsync(emp);
        await _uow.SaveChangesAsync();
        _logger.LogInformation("新增员工：{Name}，工号：{EmpNo}", emp.RealName, emp.EmpNo);
        return emp.Id;
    }

    public async Task UpdateAsync(UpdateEmployeeDto dto, string operBy)
    {
        var emp = await _uow.Employees.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("员工不存在");

        _mapper.Map(dto, emp);
        emp.UpdatedBy = operBy;
        emp.UpdatedAt = DateTime.Now;
        await _uow.SaveChangesAsync();

        // 反向同步到绑定的 SysUser
        var user = await _uow.Users.Query(false)
            .FirstOrDefaultAsync(u => u.EmployeeId == emp.Id);
        if (user != null)
        {
            user.RealName = emp.RealName;
            await _uow.SaveChangesAsync();
        }

        _logger.LogInformation("修改员工：{Id}，姓名：{Name}", emp.Id, emp.RealName);
    }

    public async Task FormalAsync(long id, string operBy)
    {
        var emp = await _uow.Employees.GetByIdAsync(id)
            ?? throw new NotFoundException("员工不存在");

        if (emp.Status != 0)
            throw new BusinessException("只有试用期员工才能办理转正");

        emp.Status = 1; // 在职
        emp.FormalDate = DateTime.Now;
        emp.UpdatedBy = operBy;
        emp.UpdatedAt = DateTime.Now;
        await _uow.SaveChangesAsync();
        _logger.LogInformation("员工转正：{Id}，姓名：{Name}", emp.Id, emp.RealName);
    }

    public async Task LeaveAsync(long id, string operBy, string? reason = null)
    {
        var emp = await _uow.Employees.GetByIdAsync(id)
            ?? throw new NotFoundException("员工不存在");

        if (emp.Status == 2)
            throw new BusinessException("该员工已经是离职状态");

        emp.Status = 2; // 离职
        emp.LeaveDate = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
            emp.Remark = string.IsNullOrWhiteSpace(emp.Remark)
                ? $"离职原因：{reason}"
                : $"{emp.Remark}；离职原因：{reason}";
        emp.UpdatedBy = operBy;
        emp.UpdatedAt = DateTime.Now;

        // 禁用关联的 SysUser 登录权限
        var boundUser = await _uow.Users.Query(false)
            .FirstOrDefaultAsync(u => u.EmployeeId == emp.Id);
        if (boundUser != null)
        {
            boundUser.Status = (int)UserStatus.Disabled;
            boundUser.UpdatedBy = operBy;
            boundUser.UpdatedAt = DateTime.Now;
        }

        await _uow.SaveChangesAsync();
        _logger.LogInformation("员工离职：{Id}，姓名：{Name}，已{BindStatus}",
            emp.Id, emp.RealName, boundUser != null ? "禁用关联账号" : "无关联账号");
    }

    private async Task<string> GenerateEmpNoAsync()
    {
        var year = DateTime.Now.Year.ToString();
        var count = await _uow.Employees.Query()
            .CountAsync(e => e.EmpNo.StartsWith($"EMP{year}"));
        return $"EMP{year}{(count + 1):D4}";
    }
}
