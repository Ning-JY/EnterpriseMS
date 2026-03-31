using AutoMapper;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IPermissionService _permSvc;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUnitOfWork uow, IMapper mapper,
        AppDbContext db, IPermissionService permSvc, ILogger<ProjectService> logger)
    { _uow = uow; _mapper = mapper; _db = db; _permSvc = permSvc; _logger = logger; }

    public async Task<PagedResult<ProjectListDto>> GetPagedAsync(ProjectQueryDto query, long operUserId)
    {
        var q = _uow.Projects.Query()
            .Include(p => p.Dept)
            .Include(p => p.TechLeader)
            .Include(p => p.BizLeader)
            .Include(p => p.Milestones)
            .AsQueryable();
        // ── 数据权限过滤 ─────────────────────────────────────────
        var (dataScope, userDeptId) = await _permSvc.GetUserDataScopeAsync(operUserId);

        if (dataScope == 1)
        {
            // 全部数据，不过滤
        }
        else if (dataScope == 3 && userDeptId.HasValue)
        {
            // 本部门及子部门：查出所有子部门ID
            var dept = await _db.SysDepts
                .Where(d => !d.IsDeleted)
                .ToListAsync();
            var deptIds = GetSelfAndChildDeptIds(dept, userDeptId.Value);
            q = q.Where(p => p.DeptId.HasValue && deptIds.Contains(p.DeptId.Value));
        }
        else if (dataScope == 2 && userDeptId.HasValue)
        {
            // 仅本部门
            q = q.Where(p => p.DeptId == userDeptId);
        }
        //  else if (dataScope == 4)
        else
        {
            // 仅本人参与的项目
            var empId = await _db.SysUsers
                .Where(u => u.Id == operUserId)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();
            if (empId.HasValue)
                q = q.Where(p => _db.ProjMembers.Any(m =>
                    m.ProjectId == p.Id && m.EmployeeId == empId && m.Status == 0));
            else
                q = q.Where(p => false);  // 未绑定员工：看不到任何项目
        }
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(p => p.ProjName.Contains(query.Keyword) ||
                             p.OwnerName.Contains(query.Keyword) ||
                             p.ProjNo.Contains(query.Keyword));
        if (query.DeptId.HasValue)
            q = q.Where(p => p.DeptId == query.DeptId);
        if (query.ProgressStatus.HasValue)
            q = q.Where(p => p.ProgressStatus == query.ProgressStatus);

        var total = await q.CountAsync();
        var list = await q.OrderByDescending(p => p.CreatedAt)
                           .Skip((query.Page - 1) * query.Size).Take(query.Size)
                           .ToListAsync();

        var items = _mapper.Map<List<ProjectListDto>>(list);
        foreach (var item in items)
            item.ProgressText = GetProgressText(item.ProgressStatus);

        return new PagedResult<ProjectListDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.Size
        };
    }

    public async Task<ProjectDetailDto?> GetDetailAsync(long id)
    {
        var proj = await _uow.Projects.Query(false)
            .Include(p => p.Dept)
            .Include(p => p.TechLeader)
            .Include(p => p.BizLeader)
            .Include(p => p.Members).ThenInclude(m => m.Employee)
            .Include(p => p.Milestones).ThenInclude(m => m.Owner)
            .Include(p => p.Acceptances)
            .Include(p => p.OperLogs)
            .Include(p => p.Contracts)
            .Include(p => p.Invoices)
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (proj == null) return null;

        var dto = _mapper.Map<ProjectDetailDto>(proj);
        dto.ProgressText = GetProgressText(proj.ProgressStatus);
        dto.TotalReceived = await GetTotalReceivedAsync(id);
        dto.StatusUpdatedAt = proj.StatusUpdatedAt;
        dto.Contracts = _mapper.Map<List<ProjectContractDto>>(
                                proj.Contracts.OrderByDescending(x => x.CreatedAt).ToList());
        dto.Invoices = _mapper.Map<List<ProjectInvoiceDto>>(
                                proj.Invoices.OrderByDescending(x => x.InvoiceDate).ToList());
        dto.Files = _mapper.Map<List<ProjectFileDto>>(
                                proj.Files.OrderByDescending(x => x.CreatedAt).ToList());

        // 计算成员产值
        var actualAmt = proj.ActualContractAmount;
        foreach (var m in dto.Members)
        {
            m.ContractValue = actualAmt * m.Ratio / 100;
            m.ReceivedValue = dto.TotalReceived * m.Ratio / 100;
        }
        return dto;
    }

    public async Task<long> CreateAsync(CreateProjectDto dto, string operBy)
    {
        if (dto.IsJointVenture && (dto.OurRatio == null || dto.OurRatio <= 0))
            throw new BusinessException("联合体项目须填写我方占比");

        var proj = _mapper.Map<Project>(dto);
        proj.ProjNo = string.IsNullOrWhiteSpace(dto.ProjNo)
                         ? await GenerateProjNoAsync() : dto.ProjNo;
        proj.CreatedBy = operBy;

        await _uow.Projects.AddAsync(proj);
        await _uow.SaveChangesAsync();

        // 写入成员
        if (dto.Members.Any())
        {
            var totalRatio = dto.Members.Sum(m => m.Ratio);
            if (Math.Abs(totalRatio - 100) > 0.01m)
                throw new BusinessException("成员占比之和必须等于100%");
            foreach (var m in dto.Members)
            {
                var member = _mapper.Map<ProjectMember>(m);
                member.ProjectId = proj.Id;
                member.CreatedBy = operBy;
                await _uow.ProjMembers.AddAsync(member);
            }
        }
        // 写入里程碑
        foreach (var ms in dto.Milestones)
        {
            var milestone = _mapper.Map<ProjectMilestone>(ms);
            milestone.ProjectId = proj.Id;
            milestone.CreatedBy = operBy;
            await _uow.Milestones.AddAsync(milestone);
        }
        await _uow.SaveChangesAsync();

        await WriteLogAsync(proj.Id, "项目创建",
            $"项目：{proj.ProjName}，合同额：{proj.ContractAmount}万元", operBy);
        return proj.Id;
    }

    public async Task UpdateAsync(UpdateProjectDto dto, string operBy)
    {
        var proj = await _uow.Projects.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("项目不存在");
        if (proj.ProgressStatus == 9)
            throw new BusinessException("已终止项目不可修改");

        var oldAmt = proj.ContractAmount;
        _mapper.Map(dto, proj);
        proj.UpdatedBy = operBy;
        _uow.Projects.Update(proj);
        await _uow.SaveChangesAsync();

        if (oldAmt != proj.ContractAmount)
            await WriteLogAsync(proj.Id, "合同金额变更",
                $"{oldAmt} 万 → {proj.ContractAmount} 万", operBy);
        else
            await WriteLogAsync(proj.Id, "项目信息修改", null, operBy);
    }

    public async Task ChangeStatusAsync(ChangeStatusDto dto, string operBy)
    {
        var proj = await _uow.Projects.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("项目不存在");
        if (dto.NewStatus <= proj.ProgressStatus && dto.NewStatus != proj.ProgressStatus)
            throw new BusinessException("状态只能向前推进，不可回退");

        var oldStatus = GetProgressText(proj.ProgressStatus);
        proj.ProgressStatus = dto.NewStatus;
        proj.StatusUpdatedAt = DateTime.Now;
        if (dto.NewStatus == 8) proj.ActualEndDate = dto.StatusDate ?? DateTime.Today;
        proj.UpdatedBy = operBy;
        _uow.Projects.Update(proj);
        await _uow.SaveChangesAsync();

        await WriteLogAsync(proj.Id, "进度状态变更",
            $"{oldStatus} → {GetProgressText(dto.NewStatus)}" +
            (string.IsNullOrWhiteSpace(dto.Remark) ? "" : $"，备注：{dto.Remark}"), operBy);
    }

    public async Task TerminateAsync(long id, string reason, string operBy)
    {
        var proj = await _uow.Projects.GetByIdAsync(id)
            ?? throw new NotFoundException("项目不存在");
        if (proj.ProgressStatus == 8)
            throw new BusinessException("已完成项目不可终止");
        proj.ProgressStatus = 9;
        proj.UpdatedBy = operBy;
        _uow.Projects.Update(proj);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(id, "项目终止", $"原因：{reason}", operBy);
    }

    public async Task<string> GenerateProjNoAsync()
    {
        var year = DateTime.Now.Year;
        var count = await _uow.Projects.CountAsync(
            p => p.CreatedAt.Year == year);
        return $"PRJ-{year}-{(count + 1):D3}";
    }

    // ── 成员 ────────────────────────────────────────────────
    public async Task<long> AddMemberAsync(long projectId, CreateMemberDto dto, string operBy)
    {
        var existing = await _uow.ProjMembers
            .GetListAsync(m => m.ProjectId == projectId && m.Status == 0);

        // 防止重复添加同一员工
        if (existing.Any(m => m.EmployeeId == dto.EmployeeId))
            throw new BusinessException("该员工已在项目团队中，如需修改请使用编辑功能");

        var totalRatio = existing.Sum(m => m.Ratio) + dto.Ratio;
        if (totalRatio > 100)
            throw new BusinessException($"占比总和将超过100%，当前已分配{existing.Sum(m => m.Ratio):N1}%");

        var member = _mapper.Map<ProjectMember>(dto);
        member.ProjectId = projectId;
        member.CreatedBy = operBy;
        await _uow.ProjMembers.AddAsync(member);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(projectId, "添加成员",
            $"员工ID：{dto.EmployeeId}，占比：{dto.Ratio}%", operBy);
        return member.Id;
    }

    public async Task UpdateMemberAsync(long projectId, UpdateMemberDto dto, string operBy)
    {
        var member = await _uow.ProjMembers.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("成员记录不存在");
        // 使用member自身的ProjectId，不依赖传入的projectId参数
        var actualProjectId = member.ProjectId;
        var others = await _uow.ProjMembers
            .GetListAsync(m => m.ProjectId == actualProjectId && m.Status == 0 && m.Id != dto.Id);
        if (others.Sum(m => m.Ratio) + dto.Ratio > 100)
            throw new BusinessException("占比总和将超过100%");

        member.Role = dto.Role;
        member.DutyDesc = dto.DutyDesc;
        member.Ratio = dto.Ratio;
        member.UpdatedBy = operBy;
        _uow.ProjMembers.Update(member);
        await _uow.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(long projectId, long memberId, string operBy)
    {
        var member = await _uow.ProjMembers.GetByIdAsync(memberId)
            ?? throw new NotFoundException("成员记录不存在");
        member.Status = 1;
        member.LeaveDate = DateTime.Today;
        member.UpdatedBy = operBy;
        _uow.ProjMembers.Update(member);
        await _uow.SaveChangesAsync();
    }

    // ── 里程碑 ───────────────────────────────────────────────
    public async Task<long> AddMilestoneAsync(long projectId, CreateMilestoneDto dto, string operBy)
    {
        var ms = _mapper.Map<ProjectMilestone>(dto);
        ms.ProjectId = projectId;
        ms.CreatedBy = operBy;
        await _uow.Milestones.AddAsync(ms);
        await _uow.SaveChangesAsync();
        return ms.Id;
    }

    public async Task UpdateMilestoneAsync(long projectId, UpdateMilestoneDto dto, string operBy)
    {
        var ms = await _uow.Milestones.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("里程碑不存在");
        ms.MilestoneName = dto.MilestoneName;
        ms.MilestoneType = dto.MilestoneType;
        ms.PlanDate = dto.PlanDate;
        ms.OwnerId = dto.OwnerId;
        ms.AcceptAmount = dto.AcceptAmount;
        ms.Sort = dto.Sort;
        ms.Remark = dto.Remark;
        ms.UpdatedBy = operBy;
        _uow.Milestones.Update(ms);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteMilestoneAsync(long milestoneId)
    {
        var ms = await _uow.Milestones.GetByIdAsync(milestoneId)
            ?? throw new NotFoundException("里程碑不存在");
        _uow.Milestones.SoftDelete(ms);
        await _uow.SaveChangesAsync();
    }

    public async Task CompleteMilestoneAsync(long milestoneId, string operBy)
    {
        var ms = await _uow.Milestones.GetByIdAsync(milestoneId)
            ?? throw new NotFoundException("里程碑不存在");
        ms.Status = 2;
        ms.ActualDate = DateTime.Today;
        ms.IsOverdue = DateTime.Today > ms.PlanDate;
        ms.UpdatedBy = operBy;
        _uow.Milestones.Update(ms);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(ms.ProjectId, "里程碑完成",
            $"{ms.MilestoneName}，实际完成：{ms.ActualDate:yyyy-MM-dd}", operBy);
    }

    // ── 验收 ─────────────────────────────────────────────────
    public async Task<long> AddAcceptanceAsync(CreateAcceptanceDto dto, string operBy)
    {
        var acc = _mapper.Map<ProjectAcceptance>(dto);
        acc.CreatedBy = operBy;
        await _uow.Acceptances.AddAsync(acc);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(dto.ProjectId, "录入验收记录",
            $"批次：{dto.AcceptBatch}，金额：{dto.AcceptAmount}万元", operBy);
        return acc.Id;
    }

    public async Task<decimal> GetTotalReceivedAsync(long projectId)
    {
        // 统计两个来源：
        // 1. ProjectAcceptance（旧的手动验收批次）
        var accTotal = await _uow.Acceptances.Query()
            .Where(a => a.ProjectId == projectId)
            .SumAsync(a => a.AcceptAmount);
        // 2. ProjectInvoice 中已确认收款的记录（回款管理）
        var invTotal = await _uow.ProjInvoices.Query()
            .Where(i => i.ProjectId == projectId && i.IsReceived)
            .SumAsync(i => i.Amount);
        return accTotal + invTotal;
    }

    // ── 合同 ──────────────────────────────────────────────────
    public async Task<long> AddContractAsync(CreateContractDto dto, string operBy)
    {
        var contract = new ProjectContract
        {
            ProjectId = dto.ProjectId,
            ContractNo = dto.ContractNo,
            ContractType = dto.ContractType,
            ContractName = dto.ContractName,
            PartyA = dto.PartyA,
            PartyB = dto.PartyB,
            Amount = dto.Amount,
            SignDate = dto.SignDate,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Remark = dto.Remark,
            Status = 1,
            CreatedBy = operBy,
        };
        await _uow.ProjContracts.AddAsync(contract);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(dto.ProjectId, "新增合同",
            $"合同编号：{dto.ContractNo}，金额：{dto.Amount}万", operBy);
        return contract.Id;
    }

    public async Task DeleteContractAsync(long contractId)
    {
        var c = await _uow.ProjContracts.GetByIdAsync(contractId)
            ?? throw new NotFoundException("合同不存在");
        _uow.ProjContracts.SoftDelete(c);
        await _uow.SaveChangesAsync();
    }

    // ── 发票 ──────────────────────────────────────────────────
    public async Task<long> AddInvoiceAsync(CreateInvoiceDto dto, string operBy)
    {
        var invoice = new ProjectInvoice
        {
            ProjectId = dto.ProjectId,
            InvoiceNo = dto.InvoiceNo,
            InvoiceType = dto.InvoiceType,
            Amount = dto.Amount,
            TaxRate = dto.TaxRate,
            InvoiceDate = dto.InvoiceDate,
            Payer = dto.Payer,
            Remark = dto.Remark,
            IsReceived = false,
            CreatedBy = operBy,
        };
        await _uow.ProjInvoices.AddAsync(invoice);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(dto.ProjectId, "开具发票",
            $"发票号：{dto.InvoiceNo}，金额：{dto.Amount}万", operBy);
        return invoice.Id;
    }

    public async Task ConfirmInvoiceReceivedAsync(long invoiceId, DateTime receivedDate, string operBy)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId)
            ?? throw new NotFoundException("发票不存在");
        inv.IsReceived = true; inv.ReceivedDate = receivedDate; inv.UpdatedBy = operBy;
        _uow.ProjInvoices.Update(inv);
        await _uow.SaveChangesAsync();
    }

    // ── 项目文件 ──────────────────────────────────────────────
    public async Task<long> AddFileAsync(long projectId, string category, string fileName,
        string filePath, long fileSize, string? description, string? version, string operBy)
    {
        var file = new ProjectFile
        {
            ProjectId = projectId,
            FileCategory = category,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            FileExt = Path.GetExtension(fileName).TrimStart('.').ToLower(),
            Description = description,
            Version = version,
            UploadBy = operBy,
            CreatedBy = operBy,
        };
        await _uow.ProjFiles.AddAsync(file);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(projectId, "上传文件", $"[{category}] {fileName}", operBy);
        return file.Id;
    }

    public async Task DeleteFileAsync(long fileId)
    {
        var f = await _uow.ProjFiles.GetByIdAsync(fileId)
            ?? throw new NotFoundException("文件不存在");
        if (System.IO.File.Exists(f.FilePath)) System.IO.File.Delete(f.FilePath);
        _uow.ProjFiles.SoftDelete(f);
        await _uow.SaveChangesAsync();
    }

    // ── 统计 ─────────────────────────────────────────────────
    public async Task<object> GetMyStatsAsync(long employeeId)
    {
        // 找该员工参与的所有项目成员记录
        var memberships = await _uow.ProjMembers.Query()
            .Include(m => m.Project)
            .Where(m => m.EmployeeId == employeeId && m.Status == 0)
            .ToListAsync();

        var stats = new List<object>();
        foreach (var ms in memberships)
        {
            if (ms.Project == null) continue;
            var proj = ms.Project;
            var received = await GetTotalReceivedAsync(proj.Id);
            var actual = proj.ActualContractAmount;
            stats.Add(new
            {
                proj.Id,
                proj.ProjNo,
                proj.ProjName,
                proj.ProgressStatus,
                ProgressText = GetProgressText(proj.ProgressStatus),
                ContractAmount = proj.ContractAmount,
                ActualAmount = actual,
                IsJoint = proj.IsJointVenture,
                OurRatio = proj.OurRatio,
                MyRatio = ms.Ratio,
                Role = ms.Role,
                DutyDesc = ms.DutyDesc,
                JoinDate = ms.JoinDate,
                // 我的应得合同产值 = 我的占比 × 项目实际合同金额
                MyContractValue = Math.Round(actual * ms.Ratio / 100, 2),
                // 我的已实现产值 = 我的占比 × 项目已收款金额
                MyReceivedValue = Math.Round(received * ms.Ratio / 100, 2),
                TotalReceived = received,
            });
        }
        return new
        {
            list = stats,
            total = stats.Count,
            // 汇总
            totalContractValue = memberships
                .Where(m => m.Project != null)
                .Sum(m => Math.Round(m.Project!.ActualContractAmount * m.Ratio / 100, 2)),
        };
    }

    public async Task<object> GetDashboardStatsAsync()
    {
        var total = await _uow.Projects.CountAsync();
        var executing = await _uow.Projects.CountAsync(p => p.ProgressStatus == 6);
        var completed = await _uow.Projects.CountAsync(p => p.ProgressStatus == 8);
        var overdue = await _uow.Milestones.CountAsync(
            m => m.IsOverdue && m.Status != 2);
        return new { total, executing, completed, overdue };
    }

    // ── 私有辅助 ─────────────────────────────────────────────
    // 递归获取部门ID及所有子部门ID
    private static List<long> GetSelfAndChildDeptIds(List<SysDept> allDepts, long deptId)
    {
        var result = new List<long> { deptId };
        var children = allDepts.Where(d => d.ParentId == deptId).ToList();
        foreach (var child in children)
            result.AddRange(GetSelfAndChildDeptIds(allDepts, child.Id));
        return result;
    }
    private async Task WriteLogAsync(long projectId, string title,
        string? content, string operBy)
    {
        var log = new ProjectOperLog
        {
            Id = SnowflakeId.Next(),
            ProjectId = projectId,
            Title = title,
            Content = content,
            OperBy = operBy,
            OperAt = DateTime.Now,
        };
        await _uow.ProjLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }

    public static string GetProgressText(int status) => status switch
    {
        0 => "前期商务",
        1 => "预计启动",
        2 => "标书制作中",
        3 => "投标/磋商中",
        4 => "已中标·签订合同中",
        5 => "已签回合同",
        6 => "执行中",
        7 => "成果提交",
        8 => "已完成",
        9 => "已终止",
        _ => "未知",
    };
}
