using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.System;

[Authorize, Route("system/user")]
public class UserController : BaseAuthController
{
    private readonly IUserService    _userSvc;
    private readonly IRoleService    _roleSvc;
    private readonly IDeptService    _deptSvc;
    private readonly IOperLogService _logSvc;
    private readonly IUnitOfWork     _uow;

    public UserController(IUserService userSvc, IRoleService roleSvc,
        IDeptService deptSvc, IOperLogService logSvc,
        IUnitOfWork uow, IPermissionService permSvc)
        : base(permSvc)
    {
        _userSvc = userSvc; _roleSvc = roleSvc; _deptSvc = deptSvc;
        _logSvc = logSvc; _uow = uow;
    }

    [HasPermission("sys:user:list")]
    public async Task<IActionResult> Index(UserQueryDto query)
    {
        var result = await _userSvc.GetPagedAsync(query);
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _uow.Posts.GetListAsync();
        ViewBag.Query = query;
        return View(result);
    }

    [HttpGet("detail/{id}")]
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var user = await _userSvc.GetDetailAsync(id);
        if (user == null) return NotFound();
        return Json(ApiResult<UserDetailDto>.Ok(user));
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:add")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            var id = await _userSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新增用户", $"用户名：{dto.Username}", "INSERT", id);
            return Json(ApiResult<object>.Ok("用户创建成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return Json(ApiResult<object>.Fail(GetErrors()));
        try
        {
            await _userSvc.UpdateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("修改用户", $"用户ID：{dto.Id}", "UPDATE", dto.Id);
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("delete/{id}"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _userSvc.DeleteAsync(id, User.GetRealName());
            await _logSvc.LogAsync("删除用户", $"用户ID：{id}", "DELETE", id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("status"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:edit")]
    public async Task<IActionResult> SetStatus(long id, int status)
    {
        try
        {
            await _userSvc.SetStatusAsync(id, status, User.GetRealName());
            await _logSvc.LogAsync("切换用户状态", $"用户ID：{id}，状态：{(status == 1 ? "启用" : "禁用")}", "UPDATE", id);
            return Json(ApiResult<object>.Ok(status == 1 ? "已启用" : "已禁用"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("resetpwd"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:reset")]
    public async Task<IActionResult> ResetPwd(long id, string newPwd)
    {
        try
        {
            await _userSvc.ResetPasswordAsync(id, newPwd, User.GetRealName());
            await _logSvc.LogAsync("重置密码", $"用户ID：{id}", "UPDATE", id);
            return Json(ApiResult<object>.Ok("密码已重置"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("changepwd"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePwd([FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetUserId();
        try
        {
            await _userSvc.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            return Json(ApiResult<object>.Ok("密码修改成功"));
        }
        catch (BusinessException ex)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpGet("roles")]
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleSvc.GetAllActiveAsync();
        return Json(ApiResult<object>.Ok(roles));
    }

    private string GetErrors() => string.Join("；",
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
}
