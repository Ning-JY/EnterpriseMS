using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>教育经历服务实现。承接原 EducationController 的数据访问。</summary>
public class EducationService : IEducationService
{
    private readonly IUnitOfWork _uow;
    public EducationService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EducationDto>> GetListAsync(long employeeId)
    {
        var list = await _uow.Educations.GetListAsync(e => e.EmployeeId == employeeId);
        return list.OrderBy(e => e.StartDate).Select(e => new EducationDto
        {
            Id         = e.Id,
            EmployeeId = e.EmployeeId,
            SchoolName = e.SchoolName,
            Major      = e.Major,
            Degree     = e.Degree,
            StartDate  = e.StartDate,
            EndDate    = e.EndDate,
            IsFullTime = e.IsFullTime,
            Remark     = e.Remark
        }).ToList();
    }

    public async Task<long> CreateAsync(CreateEducationDto dto, long employeeId, string? operBy)
    {
        var entity = new EmployeeEducation
        {
            Id         = SnowflakeId.Next(),
            EmployeeId = employeeId,
            SchoolName = dto.SchoolName,
            Major      = dto.Major,
            Degree     = dto.Degree,
            StartDate  = dto.StartDate,
            EndDate    = dto.EndDate,
            IsFullTime = dto.IsFullTime,
            Remark     = dto.Remark,
            CreatedBy  = operBy ?? "system"
        };
        await _uow.Educations.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(EducationDto dto)
    {
        var entity = await _uow.Educations.GetByIdAsync(dto.Id);
        if (entity == null) throw new NotFoundException("记录不存在");
        entity.SchoolName = dto.SchoolName;
        entity.Major      = dto.Major;
        entity.Degree     = dto.Degree;
        entity.StartDate  = dto.StartDate;
        entity.EndDate    = dto.EndDate;
        entity.IsFullTime = dto.IsFullTime;
            entity.Remark     = dto.Remark;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _uow.Educations.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException("记录不存在");
        _uow.Educations.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
