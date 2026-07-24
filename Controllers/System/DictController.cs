using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Controllers.System;

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
        return ApiOk(data);
    }

    [HttpPost("type/create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:add")]
    public async Task<IActionResult> CreateType([FromBody] CreateDictTypeDto dto)
    {
        try
        {
            var id = await _dictSvc.CreateTypeAsync(dto.DictName, dto.DictType, dto.Status, dto.Remark);
            return ApiOk(new { id }, "创建成功");
        }
        catch (BusinessException ex) { return ApiFail(ex.Message); }
    }

    [HttpPost("type/update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:edit")]
    public async Task<IActionResult> UpdateType([FromBody] UpdateDictTypeDto dto)
    {
        try
        {
            await _dictSvc.UpdateTypeAsync(dto.Id, dto.DictName, dto.DictType, dto.Status, dto.Remark);
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("type/delete/{id}")]
    [HasPermission("sys:dict:delete")]
    public async Task<IActionResult> DeleteType(long id)
    {
        try
        {
            await _dictSvc.DeleteTypeAsync(id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("data/create"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:add")]
    public async Task<IActionResult> CreateData([FromBody] CreateDictDataDto dto)
    {
        try
        {
            var id = await _dictSvc.CreateDataAsync(dto.DictType, dto.DictLabel, dto.DictValue, dto.Sort, dto.IsDefault, dto.Status);
            return ApiOk(new { id }, "创建成功");
        }
        catch (BusinessException ex) { return ApiFail(ex.Message); }
    }

    [HttpPost("data/update"), ValidateAntiForgeryToken]
    [HasPermission("sys:dict:edit")]
    public async Task<IActionResult> UpdateData([FromBody] UpdateDictDataDto dto)
    {
        try
        {
            await _dictSvc.UpdateDataAsync(dto.Id, dto.DictLabel, dto.DictValue, dto.Sort, dto.IsDefault, dto.Status);
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("data/delete/{id}")]
    [HasPermission("sys:dict:delete")]
    public async Task<IActionResult> DeleteData(long id)
    {
        try
        {
            await _dictSvc.DeleteDataAsync(id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }
}
