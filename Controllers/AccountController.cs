using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userSvc;
    private readonly IPermissionCache _cache;
    private readonly AppDbContext _db;

    public AccountController(IUserService userSvc,
        IPermissionCache cache, AppDbContext db)
    { _userSvc = userSvc; _cache = cache; _db = db; }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        // 注意：不在这里判断 IsAuthenticated 做跳转
        // 避免 Cookie/Redis 异常时造成 Login ↔ Home 死循环
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string username, string password, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "用户名和密码不能为空");
            return View();
        }

        var user = await _userSvc.GetByUsernameAsync(username);
        if (user == null || !await _userSvc.ValidatePasswordAsync(username, password))
        {
            ModelState.AddModelError("", "用户名或密码错误");
            return View();
        }
        if (user.Status == 0)
        {
            ModelState.AddModelError("", "账号已被禁用，请联系管理员");
            return View();
        }

        // 查询角色
        var roleCodes = _db.SysUserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_db.SysRoles, ur => ur.RoleId, r => r.Id, (ur, r) => r.RoleCode)
            .ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.RealName),
            new("Username",                user.Username),
            new("DeptId",                  user.DeptId?.ToString() ?? ""),
        };
        claims.AddRange(roleCodes.Select(rc => new Claim(ClaimTypes.Role, rc)));

        var identity  = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        try
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"登录写入异常：{ex.Message}");
            return View();
        }

        _ = _userSvc.UpdateLastLoginAsync(user.Id);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _cache.RemoveUserPermsAsync(userId);
        await _cache.RemoveUserMenuIdsAsync(userId);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
