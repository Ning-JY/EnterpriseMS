using EnterpriseMS.Common;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.Hr;

// ── 教育经历管理 ──────────────────────────────────────────────
[Authorize, Route("hr/education")]
public class EducationController : BaseAuthController
{
    private readonly IEducationService _svc;
    public EducationController(IEducationService svc, IPermissionService permSvc)
        : base(permSvc)
    {
        _svc = svc;
    }

    [HttpGet("list/{employeeId}")]
    public async Task<IActionResult> List(long employeeId)
    {
        var dtos = await _svc.GetListAsync(employeeId);
        return ApiOk(dtos);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Create([FromBody] CreateEducationDto dto, [FromQuery] long employeeId)
    {
        try
        {
            var id = await _svc.CreateAsync(dto, employeeId, User.Identity?.Name);
            return ApiOk(new { id }, "添加成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Update([FromBody] EducationDto dto)
    {
        try { await _svc.UpdateAsync(dto); return ApiOk("修改成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _svc.DeleteAsync(id); return ApiOk("删除成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
