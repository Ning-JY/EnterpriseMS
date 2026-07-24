using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>工作经历服务实现。承接原 WorkExpController 的数据访问。</summary>
public class WorkExpService : IWorkExpService
{
    private readonly IUnitOfWork _uow;
    public WorkExpService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<WorkExpDto>> GetListAsync(long employeeId)
    {
        var list = await _uow.WorkExperiences.GetListAsync(w => w.EmployeeId == employeeId);
        return list.OrderBy(w => w.StartDate).Select(w => new WorkExpDto
        {
            Id         = w.Id,
            EmployeeId = w.EmployeeId,
            CompanyName = w.CompanyName,
            Position   = w.Position,
            StartDate  = w.StartDate,
            EndDate    = w.EndDate,
            Remark     = w.Remark
        }).ToList();
    }

    public async Task<long> CreateAsync(CreateWorkExpDto dto, long employeeId, string? operBy)
    {
        var entity = new EmployeeWorkExp
        {
            Id         = SnowflakeId.Next(),
            EmployeeId = employeeId,
            CompanyName = dto.CompanyName,
            Position   = dto.Position,
            StartDate  = dto.StartDate,
            EndDate    = dto.EndDate,
            Remark     = dto.Remark,
            CreatedAt  = DateTime.Now,
            CreatedBy  = operBy ?? "system"
        };
        await _uow.WorkExperiences.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(WorkExpDto dto)
    {
        var entity = await _uow.WorkExperiences.GetByIdAsync(dto.Id);
        if (entity == null) throw new NotFoundException("记录不存在");
        entity.CompanyName = dto.CompanyName;
        entity.Position    = dto.Position;
        entity.StartDate   = dto.StartDate;
        entity.EndDate     = dto.EndDate;
        entity.Remark      = dto.Remark;
        entity.UpdatedAt   = DateTime.Now;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _uow.WorkExperiences.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException("记录不存在");
        _uow.WorkExperiences.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }
}
