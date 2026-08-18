using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userSvc;
    private readonly IPermissionCache _cache;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userSvc,
        IPermissionCache cache, ILogger<AccountController> logger)
    { _userSvc = userSvc; _cache = cache; _logger = logger; }

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

        try
        {
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

            // 查询角色（下沉到 UserService，避免 Controller 直连 DbContext）
            var roleCodes = await _userSvc.GetRoleCodesAsync(user.Id);

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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false });

            await _userSvc.UpdateLastLoginAsync(user.Id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // 登录流程任何环节异常都直接在登录页提示，避免被全局异常过滤器
            // 渲染到独立错误页而丢失上下文（docker 部署排障友好）。
            _logger.LogError(ex, "登录失败 username={Username}", username);
            ModelState.AddModelError("", $"登录失败：{ex.Message}");
            return View();
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
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
