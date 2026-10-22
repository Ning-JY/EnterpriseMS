using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>员工自定义字段服务：详情页"其他信息"栏，字段名与内容均由用户手动维护。</summary>
public class EmployeeExtraService : IEmployeeExtraService
{
    private readonly IUnitOfWork _uow;
    public EmployeeExtraService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EmployeeExtraDto>> GetListAsync(long employeeId)
    {
        var list = await _uow.EmployeeExtras.GetListAsync(x => x.EmployeeId == employeeId);
        return list.OrderBy(x => x.Sort).ThenBy(x => x.CreatedAt).Select(x => new EmployeeExtraDto
        {
            Id         = x.Id,
            EmployeeId = x.EmployeeId,
            FieldName  = x.FieldName,
            FieldValue = x.FieldValue,
            Sort       = x.Sort
        }).ToList();
    }

    public async Task<Dictionary<long, string>> GetSummariesAsync(IEnumerable<long> employeeIds)
    {
        var ids = employeeIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<long, string>();
        var list = await _uow.EmployeeExtras.GetListAsync(x => ids.Contains(x.EmployeeId));
        return list
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => string.Join("；", g
                    .OrderBy(x => x.Sort).ThenBy(x => x.CreatedAt)
                    .Select(x => string.IsNullOrWhiteSpace(x.FieldValue)
                        ? x.FieldName
                        : $"{x.FieldName}：{x.FieldValue}")));
    }

    public async Task<long> SaveAsync(EmployeeExtraDto dto, string? operBy)
    {
        if (string.IsNullOrWhiteSpace(dto.FieldName)) throw new BusinessException("请填写字段名");
        if (dto.Id > 0)
        {
            var entity = await _uow.EmployeeExtras.GetByIdAsync(dto.Id);
            if (entity == null) throw new NotFoundException("记录不存在");
            entity.FieldName  = dto.FieldName.Trim();
            entity.FieldValue = dto.FieldValue;
            entity.Sort       = dto.Sort;
            entity.UpdatedBy  = operBy;
            await _uow.SaveChangesAsync();
            return entity.Id;
        }
        var created = new EmployeeExtraField
        {
            Id         = SnowflakeId.Next(),
            EmployeeId = dto.EmployeeId,
            FieldName  = dto.FieldName.Trim(),
            FieldValue = dto.FieldValue,
            Sort       = dto.Sort,
            CreatedBy  = operBy ?? "system"
        };
        await _uow.EmployeeExtras.AddAsync(created);
        await _uow.SaveChangesAsync();
        return created.Id;
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _uow.EmployeeExtras.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException("记录不存在");
        _uow.EmployeeExtras.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
