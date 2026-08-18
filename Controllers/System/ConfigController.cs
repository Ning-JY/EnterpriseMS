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
    public IActionResult Index() => View();

    [HttpGet("list")]
    [HasPermission("sys:config:list")]
    public async Task<IActionResult> List(string? keyword, int page = 1, int size = 10)
        => ApiOk(await _configSvc.GetPagedAsync(keyword, page, size));

    /// <summary>编辑系统参数表单（在 layer dialog 弹层中打开）</summary>
    [HttpGet("form")]
    [HasPermission("sys:config:edit")]
    public async Task<IActionResult> Form()
    {
        var configs = await _configSvc.GetAllAsync();
        return View(configs);
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
