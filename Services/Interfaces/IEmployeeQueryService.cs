using EnterpriseMS.Services.DTOs.Hr;

namespace EnterpriseMS.Services.Interfaces;

// ── 员工查询服务（供项目/概预算下拉菜单使用，绑定hr_employee表）─────
public interface IEmployeeQueryService
{
    /// <summary>获取所有在职员工（状态0试用/1在职），供下拉菜单选择</summary>
    Task<List<EmployeeSimpleDto>> GetAllOnJobAsync();
}
