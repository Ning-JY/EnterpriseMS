using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.Hr;

// ── 员工信息 ──────────────────────────────────────────────────
[Authorize, Route("hr/employee")]
public class EmployeeController : BaseAuthController
{
    private readonly IEmployeeService _empSvc;
    private readonly IDeptService     _deptSvc;
    private readonly IDictService     _dictSvc;

    public EmployeeController(IEmployeeService empSvc, IDeptService deptSvc,
        IDictService dictSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _empSvc = empSvc; _deptSvc = deptSvc; _dictSvc = dictSvc;
    }

    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Index(string? keyword, long? deptId, int? status,
        int page = 1, int size = 15)
    {
        var query = new EmployeeQueryDto
        {
            Keyword = keyword, DeptId = deptId, Status = status, Page = page, Size = size
        };
        var paged = await _empSvc.GetPagedAsync(query);

        ViewBag.BoundEmpIds = await _empSvc.GetBoundEmployeeIdsAsync();
        ViewBag.Depts    = await _deptSvc.GetTreeAsync();
        ViewBag.Keyword  = keyword; ViewBag.DeptId = deptId; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Size = size; ViewBag.Total = paged.Total;
        ViewBag.TotalPages = (int)Math.Ceiling(paged.Total / (double)size);
        return View(paged.Items);
    }

    [HttpGet("detail/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var emp = await _empSvc.GetDetailAsync(id);
        if (emp == null) return NotFound();
        ViewBag.CertTypes     = await _dictSvc.GetDataByTypeAsync(DictType.CertType);
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync(DictType.ContractType);
        ViewBag.Depts         = await _deptSvc.GetTreeAsync();
        return View(emp);
    }

    // 供 User/Index 员工弹窗调用 - 返回 JSON
    [HttpGet("json/{id}")]
    public async Task<IActionResult> GetJson(long id)
    {
        var emp = await _empSvc.GetByIdAsync(id);
        if (emp == null) return ApiFail("员工不存在");
        return ApiOk(new
        {
            emp.Id, emp.EmpNo, emp.RealName, emp.Gender, emp.Phone, emp.Email,
            emp.IdCard, emp.DeptId, DeptName = emp.Dept?.DeptName,
            emp.PostId, emp.Status, emp.Remark,
            EntryDate = emp.EntryDate?.ToString("yyyy-MM-dd"),
            FormalDate = emp.FormalDate?.ToString("yyyy-MM-dd"),
            ProbationEndDate = emp.ProbationEndDate?.ToString("yyyy-MM-dd"),
        });
    }

    [HttpGet("edit/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var emp = await _empSvc.GetDetailAsync(id);
        if (emp == null) return NotFound();
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _empSvc.GetPostsAsync();
        return View(emp);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:add")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RealName))
            return ApiFail("姓名不能为空");
        try
        {
            var id = await _empSvc.CreateAsync(new CreateEmployeeDto
            {
                RealName = req.RealName, Gender = req.Gender, Phone = req.Phone,
                Email = req.Email, IdCard = req.IdCard, DeptId = req.DeptId,
                PostId = req.PostId, EntryDate = req.EntryDate,
                ProbationEndDate = req.ProbationEndDate, Remark = req.Remark,
            }, User.GetRealName());
            return ApiOk(new { id }, "员工信息已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest req)
    {
        try
        {
            await _empSvc.UpdateAsync(new UpdateEmployeeDto
            {
                Id = req.Id, RealName = req.RealName, Gender = req.Gender,
                Phone = req.Phone, Email = req.Email, IdCard = req.IdCard,
                DeptId = req.DeptId, PostId = req.PostId, EntryDate = req.EntryDate,
                ProbationEndDate = req.ProbationEndDate, Remark = req.Remark,
            }, User.GetRealName());
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("formal")]
    [HasPermission("hr:employee:formal")]
    public async Task<IActionResult> Formal(long id, DateTime formalDate)
    {
        try
        {
            await _empSvc.FormalAsync(id, formalDate, User.GetRealName());
            return ApiOk("转正操作成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("leave")]
    [HasPermission("hr:employee:leave")]
    public async Task<IActionResult> Leave(long id, DateTime leaveDate, string? reason)
    {
        try
        {
            await _empSvc.LeaveAsync(id, leaveDate, reason, User.GetRealName());
            return ApiOk("离职操作成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    // 供用户管理绑定员工使用
    [HttpGet("options")]
    [Authorize]
    public async Task<IActionResult> Options()
    {
        var emps     = await _empSvc.GetOnJobAsync();
        var boundIds = await _empSvc.GetBoundEmployeeIdsAsync();
        var result = emps.Select(e => new
        {
            e.Id, e.RealName,
            DeptName = e.Dept?.DeptName,
            Display  = e.Dept != null ? $"{e.RealName}（{e.Dept.DeptName}）" : e.RealName,
            IsBound  = boundIds.Contains(e.Id),
        });
        return ApiOk(result);
    }
}

// ── 员工创建/更新请求模型 ──
public class CreateEmployeeRequest
{
    public string    RealName { get; set; } = "";
    public int       Gender   { get; set; } = 1;
    public string?   Phone    { get; set; }
    public string?   Email    { get; set; }
    public string?   IdCard   { get; set; }
    public long?     DeptId   { get; set; }
    public long?     PostId   { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public string?   Remark   { get; set; }
}
public class UpdateEmployeeRequest : CreateEmployeeRequest { public long Id { get; set; } }
