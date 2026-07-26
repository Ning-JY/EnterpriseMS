using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.System;

// ── 系统参数设置 ──────────────────────────────────────────────
[Authorize, Route("system/config")]
public class ConfigController : BaseAuthController
{
    private readonly IConfigService _configSvc;
    public ConfigController(IConfigService configSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _configSvc = configSvc;
    }

    [HasPermission("sys:config:list")]
    public async Task<IActionResult> Index()
    {
        var configs = await _configSvc.GetAllAsync();
        ViewBag.Configs = configs;
        return View();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _configSvc.GetAllAsync();
        return ApiOk(configs);
    }

    [HttpPost("save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] List<SysConfigDto> configs)
    {
        try
        {
            await _configSvc.SaveAsync(configs);
            return ApiOk("保存成功");
        }
        catch (Exception ex)
        {
            return ApiFail($"保存失败：{ex.Message}");
        }
    }
}
