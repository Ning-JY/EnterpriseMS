using System.Net;
using AutoMapper;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.Bid;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.DTOs.Bid;

namespace EnterpriseMS.Services.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // User
        CreateMap<SysUser, UserListDto>()
            .ForMember(d => d.DeptName,  o => o.MapFrom(s => s.Dept != null ? s.Dept.DeptName : null))
            // 姓名/角色名在库里可能被存成 HTML 实体串（如 肖玲 → &#x8096;&#x73B6;），展示前统一解码，避免页面显示实体原文
            .ForMember(d => d.RealName,  o => o.MapFrom(s => WebUtility.HtmlDecode(s.RealName ?? "")))
            .ForMember(d => d.RoleNames, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role != null ? WebUtility.HtmlDecode(ur.Role.RoleName) : "").ToList()));
        CreateMap<SysUser, UserDetailDto>()
            .IncludeBase<SysUser, UserListDto>()
            .ForMember(d => d.RealName, o => o.MapFrom(s => WebUtility.HtmlDecode(s.RealName ?? "")))
            .ForMember(d => d.RoleIds, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.RoleId).ToList()));
        CreateMap<CreateUserDto, SysUser>();
        CreateMap<UpdateUserDto, SysUser>();

        // Role
        CreateMap<SysRole, RoleListDto>();
        CreateMap<CreateRoleDto, SysRole>();

        // Menu
        CreateMap<SysMenu, MenuTreeDto>();
        CreateMap<SysMenu, MenuListDto>();
        CreateMap<CreateMenuDto, SysMenu>();

        // Dept
        CreateMap<SysDept, DeptTreeDto>();
        CreateMap<SysDept, DeptListDto>();
        CreateMap<CreateDeptDto, SysDept>();

        // Project
        CreateMap<Project, ProjectListDto>()
            .ForMember(d => d.DeptName,          o => o.MapFrom(s => s.Dept != null ? s.Dept.DeptName : null))
            .ForMember(d => d.ProjectLeaderName, o => o.MapFrom(s => s.ProjectLeader != null ? WebUtility.HtmlDecode(s.ProjectLeader.RealName) : null))
            .ForMember(d => d.ActualAmount,      o => o.MapFrom(s => s.ActualContractAmount))
            .ForMember(d => d.MilestoneDone,     o => o.MapFrom(s => s.Milestones.Count(m => m.Status == 2)))
            .ForMember(d => d.MilestoneTotal,    o => o.MapFrom(s => s.Milestones.Count))
            .ForMember(d => d.OwnerContact,      o => o.MapFrom(s => s.OwnerContact));
        CreateMap<Project, ProjectDetailDto>()
            .IncludeBase<Project, ProjectListDto>()
            .ForMember(d => d.DeptId,       o => o.MapFrom(s => s.DeptId))
            .ForMember(d => d.LimitPrice,   o => o.MapFrom(s => s.LimitPrice))
            .ForMember(d => d.BuildingScale,o => o.MapFrom(s => s.BuildingScale))
            .ForMember(d => d.OwnerContact, o => o.MapFrom(s => s.OwnerContact));
        CreateMap<CreateProjectDto, Project>()
            .ForMember(d => d.Members, o => o.Ignore())
            .ForMember(d => d.Milestones, o => o.Ignore()); 
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
            .ForMember(d => d.Dept,           o => o.Ignore());

        // ProjectMember
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? WebUtility.HtmlDecode(s.Employee.RealName) : ""));
        CreateMap<CreateMemberDto, ProjectMember>();

        // Milestone
        CreateMap<ProjectMilestone, ProjectMilestoneDto>()
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner != null ? WebUtility.HtmlDecode(s.Owner.RealName) : null));
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

        // Employee
        CreateMap<Employee, EmployeeListDto>()
            .ForMember(d => d.DeptName, o => o.MapFrom(s => s.Dept != null ? s.Dept.DeptName : null));
        CreateMap<Employee, EmployeeDetailDto>()
            .IncludeBase<Employee, EmployeeListDto>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EmpNo, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Contracts, o => o.Ignore())
            .ForMember(d => d.Certificates, o => o.Ignore())
            .ForMember(d => d.Dept, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.FormalDate, o => o.Ignore())
            .ForMember(d => d.LeaveDate, o => o.Ignore());
        CreateMap<EmployeeContract, EmployeeContractDto>();
        CreateMap<EmployeeCertificate, EmployeeCertificateDto>();

        // Bid
        CreateMap<BidProject, BidProjectDto>()
            .ForMember(d => d.StatusName, o => o.MapFrom(src => GetBidStatusName(src.Status)))
            .ForMember(d => d.ParseStageName, o => o.MapFrom(src => GetParseStageName(src.ParseStage)));
        CreateMap<BidRequirement, BidRequirementDto>();
        CreateMap<BidDocument, BidDocumentDto>();
    }

    private static string GetParseStageName(int stage) => stage switch
    {
        0 => "未解析",
        1 => "解析中",
        2 => "待人工确认",
        3 => "已确认",
        _ => "未知"
    };

    private static string GetBidStatusName(int status) => status switch
    {
        0 => "草稿",
        1 => "解析中",
        2 => "生成中",
        3 => "审查中",
        4 => "就绪",
        5 => "已提交",
        6 => "已中标",
        7 => "未中标",
        _ => "未知"
    };
}
