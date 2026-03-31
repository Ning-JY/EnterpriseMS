using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// 员工查询服务 - 专门为下拉菜单提供员工列表。
/// 项目台账的技术负责人/商务负责人/成员，概预算任务的技术负责人，
/// 全部绑定到 hr_employee 表，而不是 sys_user 表。
/// </summary>
public class EmployeeQueryService : IEmployeeQueryService
{
    private readonly IUnitOfWork _uow;

    public EmployeeQueryService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EmployeeSimpleDto>> GetAllOnJobAsync()
    {
        var list = await _uow.Employees.Query()
            .Include(e => e.Dept)
            // 试用期(0)和在职(1)都可参与项目
            .Where(e => e.Status == 0 || e.Status == 1)
            .OrderBy(e => e.Dept != null ? e.Dept.Sort : 999)
            .ThenBy(e => e.RealName)
            .Select(e => new EmployeeSimpleDto
            {
                Id       = e.Id,
                RealName = e.RealName,
                DeptName = e.Dept != null ? e.Dept.DeptName : null,
            })
            .ToListAsync();
        return list;
    }
}
