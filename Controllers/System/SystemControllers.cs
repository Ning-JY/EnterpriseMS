using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Controllers.System;

// ── 部门管理 ──────────────────────────────────────────────────
[Authorize, Route("system/dept")]
public class DeptController : BaseAuthController
{
    private readonly IDeptService _deptSvc;
    private readonly IOperLogService _logSvc;

    public DeptController(IDeptService deptSvc, IOperLogService logSvc,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _deptSvc = deptSvc; _logSvc = logSvc;
    }

    [HasPermission("sys:dept:list")]
    public async Task<IActionResult> Index()
    {
        var tree = await _deptSvc.GetTreeAsync();
        return View(tree);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _deptSvc.GetTreeAsync();
        return Json(ApiResult<object>.Ok(tree));
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dept:add")]
    public async Task<IActionResult> Create([FromBody] CreateDeptDto dto)
    {
        try
        {
            var id = await _deptSvc.CreateAsync(dto, User.GetRealName());
            return Json(ApiResult<object>.Ok(new { id }, "创建成功"));
        }
        catch (BusinessException ex) { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dept:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateDeptDto dto)
    {
        try
        {
            await _deptSvc.UpdateAsync(dto, User.GetRealName());
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:dept:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _deptSvc.DeleteAsync(id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }
}

// ── 字典管理 ──────────────────────────────────────────────────
[Authorize, Route("system/dict")]
public class DictController : BaseAuthController
{
    private readonly IDictService _dictSvc;
    public DictController(IDictService dictSvc, IPermissionService permSvc)
        : base(permSvc)
        => _dictSvc = dictSvc;

    [HasPermission("sys:dict:list")]
    public async Task<IActionResult> Index()
    {
        var types = await _dictSvc.GetAllTypesAsync();
        return View(types);
    }

    [HttpGet("data/{dictType}")]
    public async Task<IActionResult> GetData(string dictType)
    {
        var data = await _dictSvc.GetDataByTypeAsync(dictType);
        return Json(ApiResult<object>.Ok(data));
    }

    [HttpPost("type/create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:add")]
    public async Task<IActionResult> CreateType([FromBody] CreateDictTypeDto dto)
    {
        try
        {
            var id = await _dictSvc.CreateTypeAsync(dto.DictName, dto.DictType, dto.Status, dto.Remark);
            return Json(ApiResult<object>.Ok(new { id }, "创建成功"));
        }
        catch (BusinessException ex) { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("type/update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:edit")]
    public async Task<IActionResult> UpdateType([FromBody] UpdateDictTypeDto dto)
    {
        try
        {
            await _dictSvc.UpdateTypeAsync(dto.Id, dto.DictName, dto.DictType, dto.Status, dto.Remark);
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("type/delete/{id}")]
    [HasPermission("sys:dict:delete")]
    public async Task<IActionResult> DeleteType(long id)
    {
        try
        {
            await _dictSvc.DeleteTypeAsync(id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("data/create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:add")]
    public async Task<IActionResult> CreateData([FromBody] CreateDictDataDto dto)
    {
        try
        {
            var id = await _dictSvc.CreateDataAsync(dto.DictType, dto.DictLabel, dto.DictValue, dto.Sort, dto.IsDefault, dto.Status);
            return Json(ApiResult<object>.Ok(new { id }, "创建成功"));
        }
        catch (BusinessException ex) { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("data/update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:edit")]
    public async Task<IActionResult> UpdateData([FromBody] UpdateDictDataDto dto)
    {
        try
        {
            await _dictSvc.UpdateDataAsync(dto.Id, dto.DictLabel, dto.DictValue, dto.Sort, dto.IsDefault, dto.Status);
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("data/delete/{id}")]
    [HasPermission("sys:dict:delete")]
    public async Task<IActionResult> DeleteData(long id)
    {
        try
        {
            await _dictSvc.DeleteDataAsync(id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }
}

// ── 操作日志 ──────────────────────────────────────────────────
[Authorize, Route("system/log")]
public class LogController : BaseAuthController
{
    private readonly AppDbContext _db;
    public LogController(AppDbContext db, IPermissionService permSvc)
        : base(permSvc)
        => _db = db;

    [HasPermission("sys:log:list")]
    public async Task<IActionResult> Index(string? keyword, int page = 1, int size = 20)
    {
        var q = _db.SysOperLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(l => l.Title.Contains(keyword) ||
                             (l.OperName != null && l.OperName.Contains(keyword)));
        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(l => l.OperTime)
                           .Skip((page-1)*size).Take(size).ToListAsync();
        ViewBag.Keyword = keyword;
        ViewBag.Page    = page;
        ViewBag.Size    = size;
        ViewBag.Total   = total;
        return View(list);
    }
}

// ── 菜单管理 ──────────────────────────────────────────────────
[Authorize, Route("system/menu")]
public class MenuController : BaseAuthController
{
    private readonly IMenuService _menuSvc;
    public MenuController(IMenuService menuSvc, IPermissionService permSvc)
        : base(permSvc)
        => _menuSvc = menuSvc;

    [HasPermission("sys:menu:list")]
    public async Task<IActionResult> Index()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return View(tree);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var tree = await _menuSvc.GetTreeAsync();
        return Json(ApiResult<object>.Ok(tree));
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:menu:add")]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto)
    {
        try
        {
            var id = await _menuSvc.CreateAsync(dto, User.Identity?.Name ?? "system");
            return Json(ApiResult<object>.Ok(new { id }, "创建成功"));
        }
        catch (BusinessException ex) { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:menu:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateMenuDto dto)
    {
        try
        {
            await _menuSvc.UpdateAsync(dto, User.Identity?.Name ?? "system");
            return Json(ApiResult<object>.Ok("修改成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:menu:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _menuSvc.DeleteAsync(id);
            return Json(ApiResult<object>.Ok("删除成功"));
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return Json(ApiResult<object>.Fail(ex.Message)); }
    }
}

// ── 系统参数设置 ──────────────────────────────────────────────
[Authorize, Route("system/config")]
public class ConfigController : BaseAuthController
{
    private readonly AppDbContext _db;
    public ConfigController(AppDbContext db, IPermissionService permSvc)
        : base(permSvc)
        => _db = db;

    [HasPermission("sys:config:list")]
    public async Task<IActionResult> Index()
    {
        var configs = await _db.SysConfigs
            .OrderBy(c => c.GroupName).ThenBy(c => c.Sort)
            .ToListAsync();
        ViewBag.Configs = configs;
        return View();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _db.SysConfigs.OrderBy(c => c.GroupName).ToListAsync();
        return Json(ApiResult<object>.Ok(configs));
    }

    [HttpPost("save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] List<SysConfigDto> configs)
    {
        try
        {
            foreach (var dto in configs)
            {
                var existing = await _db.SysConfigs.FirstOrDefaultAsync(c => c.ConfigKey == dto.ConfigKey);
                if (existing != null)
                {
                    existing.ConfigValue = dto.ConfigValue;
                }
                else
                {
                    _db.SysConfigs.Add(new EnterpriseMS.Domain.Entities.System.SysConfig
                    {
                        Id = SnowflakeId.Next(),
                        ConfigKey = dto.ConfigKey,
                        ConfigValue = dto.ConfigValue,
                        ConfigType = dto.ConfigType ?? "text",
                        GroupName = dto.GroupName ?? "system",
                        Sort = dto.Sort
                    });
                }
            }
            await _db.SaveChangesAsync();
            return Json(ApiResult<object>.Ok("保存成功"));
        }
        catch (Exception ex)
        {
            return Json(ApiResult<object>.Fail($"保存失败：{ex.Message}"));
        }
    }
}