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
    {
        _dictSvc = dictSvc;
    }

    [HasPermission("sys:dict:list")]
    public IActionResult Index() => View();

    [HttpGet("list")]
    [HasPermission("sys:dict:list")]
    public async Task<IActionResult> List(string? keyword, int page = 1, int size = 10)
        => ApiOk(await _dictSvc.GetPagedAsync(keyword, page, size));

    // 字典项管理列表（含停用项）—— 供字典管理页右侧「业务类型/字典项」表格使用
    [HttpGet("data/list")]
    [HasPermission("sys:dict:list")]
    public async Task<IActionResult> DataList(string? dictType, string? keyword)
        => ApiOk(await _dictSvc.GetDataListAsync(dictType ?? "", keyword));

    // 新增 / 编辑字典项表单（iframe 弹层）
    [HttpGet("data/form")]
    [HasPermission("sys:dict:list")]
    public async Task<IActionResult> DataForm(long? id, string? dictType)
    {
        DictDataDto? model = null;
        if (id.HasValue && id.Value > 0)
        {
            model = await _dictSvc.GetDataByIdAsync(id.Value);
            if (model == null) return NotFound();
        }
        ViewBag.DictType     = model?.DictType ?? dictType ?? "";
        ViewBag.DictTypeName = ViewBag.DictType;
        if (!string.IsNullOrWhiteSpace((string)ViewBag.DictType))
        {
            var t = (await _dictSvc.GetAllTypesAsync())
                .FirstOrDefault(x => x.DictType == (string)ViewBag.DictType);
            if (t != null) ViewBag.DictTypeName = t.DictName;
        }
        return View("DataForm", model);
    }

    [HttpGet("data/{dictType}")]
    public async Task<IActionResult> GetData(string dictType)
    {
        var data = await _dictSvc.GetDataByTypeAsync(dictType);
        return ApiOk(data);
    }

    // 新增 / 编辑字典类型表单（iframe 弹层）
    [HttpGet("form")]
    [HasPermission("sys:dict:list")]
    public async Task<IActionResult> Form(long? id)
    {
        DictTypeDto? model = null;
        if (id.HasValue)
        {
            model = await _dictSvc.GetByIdAsync(id.Value);
            if (model == null) return NotFound();
        }
        return View(model);
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
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
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
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
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
