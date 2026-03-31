using AutoMapper;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.DTOs.Project;

namespace EnterpriseMS.Services.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // User
        CreateMap<SysUser, UserListDto>()
            .ForMember(d => d.DeptName,  o => o.MapFrom(s => s.Dept != null ? s.Dept.DeptName : null))
            .ForMember(d => d.RoleNames, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role != null ? ur.Role.RoleName : "").ToList()));
        CreateMap<SysUser, UserDetailDto>()
            .IncludeBase<SysUser, UserListDto>()
            .ForMember(d => d.RoleIds, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.RoleId).ToList()));
        CreateMap<CreateUserDto, SysUser>();
        CreateMap<UpdateUserDto, SysUser>();

        // Role
        CreateMap<SysRole, RoleListDto>();
        CreateMap<CreateRoleDto, SysRole>();

        // Menu
        CreateMap<SysMenu, MenuTreeDto>();
        CreateMap<CreateMenuDto, SysMenu>();

        // Dept
        CreateMap<SysDept, DeptTreeDto>();
        CreateMap<CreateDeptDto, SysDept>();

        // Project
        CreateMap<Project, ProjectListDto>()
            .ForMember(d => d.DeptName,       o => o.MapFrom(s => s.Dept != null ? s.Dept.DeptName : null))
            .ForMember(d => d.TechLeaderName, o => o.MapFrom(s => s.TechLeader != null ? s.TechLeader.RealName : null))
            .ForMember(d => d.BizLeaderName,  o => o.MapFrom(s => s.BizLeader  != null ? s.BizLeader.RealName  : null))
            .ForMember(d => d.ActualAmount,   o => o.MapFrom(s => s.ActualContractAmount))
            .ForMember(d => d.MilestoneDone,  o => o.MapFrom(s => s.Milestones.Count(m => m.Status == 2)))
            .ForMember(d => d.MilestoneTotal, o => o.MapFrom(s => s.Milestones.Count))
            .ForMember(d => d.OwnerContact,   o => o.MapFrom(s => s.OwnerContact));
        CreateMap<Project, ProjectDetailDto>()
            .IncludeBase<Project, ProjectListDto>()
            .ForMember(d => d.DeptId,       o => o.MapFrom(s => s.DeptId))
            .ForMember(d => d.TechLeaderId, o => o.MapFrom(s => s.TechLeaderId))
            .ForMember(d => d.BizLeaderId,  o => o.MapFrom(s => s.BizLeaderId))
            .ForMember(d => d.LimitPrice,   o => o.MapFrom(s => s.LimitPrice))
            .ForMember(d => d.BuildingScale,o => o.MapFrom(s => s.BuildingScale))
            .ForMember(d => d.OwnerContact, o => o.MapFrom(s => s.OwnerContact));
        CreateMap<CreateProjectDto, Project>();
        // UpdateProjectDto -> Project（用于 ProjectService.UpdateAsync 的 Map(dto, proj)）
        CreateMap<UpdateProjectDto, Project>()
            .ForMember(d => d.Id,             o => o.Ignore())  // Id不覆盖
            .ForMember(d => d.CreatedAt,      o => o.Ignore())  // 创建时间不覆盖
            .ForMember(d => d.CreatedBy,      o => o.Ignore())  // 创建人不覆盖
            .ForMember(d => d.IsDeleted,      o => o.Ignore())  // 软删除标记不覆盖
            .ForMember(d => d.Members,        o => o.Ignore())  // 导航属性不覆盖
            .ForMember(d => d.Milestones,     o => o.Ignore())
            .ForMember(d => d.Acceptances,    o => o.Ignore())
            .ForMember(d => d.OperLogs,       o => o.Ignore())
            .ForMember(d => d.Dept,           o => o.Ignore())
            .ForMember(d => d.TechLeader,     o => o.Ignore())
            .ForMember(d => d.BizLeader,      o => o.Ignore());

        // ProjectMember
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.RealName : ""));
        CreateMap<CreateMemberDto, ProjectMember>();

        // Milestone
        CreateMap<ProjectMilestone, ProjectMilestoneDto>()
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner != null ? s.Owner.RealName : null));
        // 新增实体映射
        CreateMap<ProjectContract, ProjectContractDto>();
        CreateMap<ProjectInvoice, ProjectInvoiceDto>();
        CreateMap<ProjectFile, ProjectFileDto>();
        CreateMap<CreateMilestoneDto, ProjectMilestone>();

        // Acceptance
        CreateMap<ProjectAcceptance, ProjectAcceptanceDto>();
        CreateMap<CreateAcceptanceDto, ProjectAcceptance>();

        // ProjectLog
        CreateMap<ProjectOperLog, ProjectLogDto>();
    }
}
// 追加在文件末尾前闭合花括号前，此处用bash追加到文件最后一行之前的方式
