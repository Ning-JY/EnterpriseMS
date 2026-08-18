using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class DictService : IDictService
{
    private readonly IUnitOfWork _uow;
    public DictService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<DictDataDto>> GetDataByTypeAsync(string dictType)
    {
        var list = await _uow.DictDatas.GetListAsync(
            d => d.DictType == dictType && d.Status == 1);
        return list.OrderBy(d => d.Sort)
                   .Select(d => new DictDataDto
                   { Id = d.Id, DictType = d.DictType, DictLabel = d.DictLabel,
                     DictValue = d.DictValue, Sort = d.Sort, IsDefault = d.IsDefault,
                     Status = d.Status })
                   .ToList();
    }

    // 字典项管理列表：含停用项，支持按标签/键值模糊搜索
    public async Task<List<DictDataDto>> GetDataListAsync(string dictType, string? keyword = null)
    {
        if (string.IsNullOrWhiteSpace(dictType)) return new List<DictDataDto>();

        var list = await _uow.DictDatas.GetListAsync(d => d.DictType == dictType);
        var query = list.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(d => d.DictLabel.Contains(kw, StringComparison.OrdinalIgnoreCase)
                                  || d.DictValue.Contains(kw, StringComparison.OrdinalIgnoreCase));
        }
        return query.OrderBy(d => d.Sort).ThenBy(d => d.Id)
                    .Select(d => new DictDataDto
                    { Id = d.Id, DictType = d.DictType, DictLabel = d.DictLabel,
                      DictValue = d.DictValue, Sort = d.Sort, IsDefault = d.IsDefault,
                      Status = d.Status })
                    .ToList();
    }

    public async Task<DictDataDto?> GetDataByIdAsync(long id)
    {
        var d = await _uow.DictDatas.GetByIdAsync(id);
        if (d == null) return null;
        return new DictDataDto
        {
            Id = d.Id, DictType = d.DictType, DictLabel = d.DictLabel,
            DictValue = d.DictValue, Sort = d.Sort, IsDefault = d.IsDefault,
            Status = d.Status
        };
    }

    public async Task<PagedResult<DictTypeDto>> GetPagedAsync(string? keyword, int page, int size)
    {
        var q = _uow.DictTypes.Query();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(d => d.DictName.Contains(keyword) || d.DictType.Contains(keyword));
        var paged = await q.OrderBy(d => d.DictType).ToPagedAsync(page, size);
        var items = paged.Items.Select(d => new DictTypeDto
        {
            Id = d.Id, DictName = d.DictName, DictType = d.DictType, Status = d.Status
        }).ToList();
        return new PagedResult<DictTypeDto>
        { Items = items, Total = paged.Total, Page = page, PageSize = size };
    }

    public async Task<List<DictTypeDto>> GetAllTypesAsync()
    {
        var list = await _uow.DictTypes.GetListAsync(d => d.Status == 1);
        return list.Select(d => new DictTypeDto
            { Id = d.Id, DictName = d.DictName, DictType = d.DictType, Status = d.Status })
            .ToList();
    }

    public async Task<DictTypeDto?> GetByIdAsync(long id)
    {
        var entity = await _uow.DictTypes.GetByIdAsync(id);
        if (entity == null) return null;
        return new DictTypeDto
        {
            Id = entity.Id,
            DictName = entity.DictName,
            DictType = entity.DictType,
            Status = entity.Status,
            Remark = entity.Remark
        };
    }

    public async Task<long> CreateTypeAsync(string dictName, string dictType, int status = 1, string? remark = null)
    {
        var entity = new SysDictType
        {
            Id = SnowflakeId.Next(),
            DictName = dictName,
            DictType = dictType,
            Status = status,
            Remark = remark,
            CreatedBy = "system"
        };
        await _uow.DictTypes.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateTypeAsync(long id, string dictName, string dictType, int status, string? remark = null)
    {
        var entity = await _uow.DictTypes.GetByIdAsync(id)
            ?? throw new NotFoundException("字典类型不存在");
        entity.DictName = dictName;
        entity.DictType = dictType;
        entity.Status = status;
        entity.Remark = remark;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteTypeAsync(long id)
    {
        var entity = await _uow.DictTypes.GetByIdAsync(id)
            ?? throw new NotFoundException("字典类型不存在");
        // 代码依赖的字典类型受系统保护，禁止删除（避免对应下拉/逻辑整体崩溃）。
        // 类型内部的单个选项仍可删除（满足字典“增减”需求）。
        if (DictType.All.Contains(entity.DictType))
            throw new BusinessException("系统字典类型（代码依赖）不可删除");
        // 同时软删除关联的字典数据
        var datas = await _uow.DictDatas.GetListAsync(d => d.DictType == entity.DictType);
        foreach (var d in datas) _uow.DictDatas.SoftDelete(d);
        _uow.DictTypes.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<long> CreateDataAsync(string dictType, string dictLabel, string dictValue, int sort = 0, int isDefault = 0, int status = 1)
    {
        var entity = new SysDictData
        {
            Id = SnowflakeId.Next(),
            DictType = dictType,
            DictLabel = dictLabel,
            DictValue = dictValue,
            Sort = sort,
            IsDefault = isDefault,
            Status = status,
            CreatedBy = "system"
        };
        await _uow.DictDatas.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateDataAsync(long id, string dictLabel, string dictValue, int sort, int isDefault, int status)
    {
        var entity = await _uow.DictDatas.GetByIdAsync(id)
            ?? throw new NotFoundException("字典数据不存在");
        entity.DictLabel = dictLabel;
        entity.DictValue = dictValue;
        entity.Sort = sort;
        entity.IsDefault = isDefault;
        entity.Status = status;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteDataAsync(long id)
    {
        var entity = await _uow.DictDatas.GetByIdAsync(id)
            ?? throw new NotFoundException("字典数据不存在");
        _uow.DictDatas.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
