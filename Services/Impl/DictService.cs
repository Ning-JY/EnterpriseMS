using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
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
                     DictValue = d.DictValue, Sort = d.Sort, IsDefault = d.IsDefault })
                   .ToList();
    }

    public async Task<List<DictTypeDto>> GetAllTypesAsync()
    {
        var list = await _uow.DictTypes.GetListAsync(d => d.Status == 1);
        return list.Select(d => new DictTypeDto
            { Id = d.Id, DictName = d.DictName, DictType = d.DictType, Status = d.Status })
            .ToList();
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
            CreatedAt = DateTime.Now,
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
            CreatedAt = DateTime.Now,
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
