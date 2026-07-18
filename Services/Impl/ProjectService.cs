using AutoMapper;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.DTOs.Project;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permSvc;
    private readonly ILogger<ProjectService> _logger;

    // 允许上传的文件扩展名白名单
    private static readonly HashSet<string> AllowedFileExts = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf","doc","docx","xls","xlsx","ppt","pptx",
        "jpg","jpeg","png","gif","bmp","tiff",
        "zip","rar","7z","txt","csv","dwg","dxf"
    };

    public ProjectService(IUnitOfWork uow, IMapper mapper,
        IPermissionService permSvc, ILogger<ProjectService> logger)
    { _uow = uow; _mapper = mapper; _permSvc = permSvc; _logger = logger; }

    public async Task<PagedResult<ProjectListDto>> GetPagedAsync(ProjectQueryDto query, long operUserId)
    {
        var q = _uow.Projects.Query()
            .Include(p => p.Dept)
            .Include(p => p.TechLeader)
            .Include(p => p.BizLeader)
            .Include(p => p.Milestones)
            .AsQueryable();
        // ── 数据权限过滤（按部门 / 项目成员隔离，列表与详情共用）─────────
        q = await ApplyDataScopeAsync(q, operUserId);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(p => p.ProjName.Contains(query.Keyword) ||
                             p.OwnerName.Contains(query.Keyword) ||
                             p.ProjNo.Contains(query.Keyword));
        if (query.DeptId.HasValue)
            q = q.Where(p => p.DeptId == query.DeptId);
        if (query.ProgressStatus.HasValue)
            q = q.Where(p => p.ProgressStatus == query.ProgressStatus);
        if (!string.IsNullOrWhiteSpace(query.BizType))
            q = q.Where(p => p.BizType == query.BizType);

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

    public async Task<ProjectDetailDto?> GetDetailAsync(long id, long operUserId)
    {
        var q = _uow.Projects.Query(false)
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
            .AsQueryable();

        // 数据权限：非授权范围直接查不到（避免靠 ID 越权查看）
        q = await ApplyDataScopeAsync(q, operUserId);

        var proj = await q.FirstOrDefaultAsync(p => p.Id == id);
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
        // #5 修复：不允许变更到相同状态
        if (dto.NewStatus < proj.ProgressStatus)
            throw new BusinessException("状态只能向前推进，不可回退");
        if (dto.NewStatus == proj.ProgressStatus)
            throw new BusinessException("新状态与当前状态相同，无需变更");

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

    // #1 修复：使用 MaxAsync + 解析最大序号，避免并发冲突和删除后编号重复
    public async Task<string> GenerateProjNoAsync()
    {
        var year = DateTime.Now.Year;
        var prefix = $"PRJ-{year}-";

        var maxNo = await _uow.Projects.Query()
            .Where(p => p.ProjNo.StartsWith(prefix))
            .Select(p => p.ProjNo)
            .ToListAsync();

        var maxSeq = 0;
        foreach (var no in maxNo)
        {
            var dashIdx = no.LastIndexOf('-');
            if (dashIdx >= 0 && int.TryParse(no[(dashIdx + 1)..], out var seq) && seq > maxSeq)
                maxSeq = seq;
        }

        return $"{prefix}{(maxSeq + 1):D3}";
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

    // #3 修复：仅标记退出状态，不设 IsDeleted，保留历史可查
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

    // #11 修复：编辑里程碑时，如果计划日期被延后到未来，恢复逾期标记
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
        // 如果计划日期被延后到未来且未完成，清除逾期标记
        if (ms.Status != 2 && dto.PlanDate >= DateTime.Today && ms.IsOverdue)
            ms.IsOverdue = false;
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

    // #14 新增：编辑验收记录
    public async Task UpdateAcceptanceAsync(UpdateAcceptanceDto dto, string operBy)
    {
        var acc = await _uow.Acceptances.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("验收记录不存在");
        acc.AcceptBatch = dto.AcceptBatch;
        acc.AcceptDate = dto.AcceptDate;
        acc.AcceptAmount = dto.AcceptAmount;
        acc.InvoiceNo = dto.InvoiceNo;
        acc.Remark = dto.Remark;
        acc.UpdatedBy = operBy;
        _uow.Acceptances.Update(acc);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(acc.ProjectId, "修改验收记录",
            $"批次：{dto.AcceptBatch}，金额：{dto.AcceptAmount}万元", operBy);
    }

    // #14 新增：删除验收记录
    public async Task DeleteAcceptanceAsync(long acceptanceId)
    {
        var acc = await _uow.Acceptances.GetByIdAsync(acceptanceId)
            ?? throw new NotFoundException("验收记录不存在");
        _uow.Acceptances.SoftDelete(acc);
        await _uow.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalReceivedAsync(long projectId)
    {
        // 统计两个来源：
        // 1. ProjectAcceptance（手动验收批次）
        var accTotal = await _uow.Acceptances.Query()
            .Where(a => a.ProjectId == projectId)
            .SumAsync(a => a.AcceptAmount);
        // 2. ProjectInvoice 中已确认收款的记录（回款管理）
        var invTotal = await _uow.ProjInvoices.Query()
            .Where(i => i.ProjectId == projectId && i.IsReceived)
            .SumAsync(i => i.Amount);
        // 注意：验收和发票回款为独立业务流程，分别统计
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

    // #13 新增：编辑合同
    public async Task UpdateContractAsync(UpdateContractDto dto, string operBy)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("合同不存在");
        contract.ContractNo = dto.ContractNo;
        contract.ContractType = dto.ContractType;
        contract.ContractName = dto.ContractName;
        contract.PartyA = dto.PartyA;
        contract.PartyB = dto.PartyB;
        contract.Amount = dto.Amount;
        contract.SignDate = dto.SignDate;
        contract.StartDate = dto.StartDate;
        contract.EndDate = dto.EndDate;
        contract.Remark = dto.Remark;
        contract.UpdatedBy = operBy;
        _uow.ProjContracts.Update(contract);
        await _uow.SaveChangesAsync();
        await WriteLogAsync(contract.ProjectId, "修改合同",
            $"合同编号：{dto.ContractNo}，金额：{dto.Amount}万", operBy);
    }

    public async Task DeleteContractAsync(long contractId)
    {
        var c = await _uow.ProjContracts.GetByIdAsync(contractId)
            ?? throw new NotFoundException("合同不存在");
        _uow.ProjContracts.SoftDelete(c);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：合同附件上传（从 Controller 迁移到 Service）
    public async Task UploadContractFileAsync(long contractId, string fileName,
        string filePath, string operBy)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId)
            ?? throw new NotFoundException("合同不存在");
        if (!string.IsNullOrEmpty(contract.FilePath) && File.Exists(contract.FilePath))
            File.Delete(contract.FilePath);
        contract.FilePath = filePath;
        contract.FileName = fileName;
        contract.UpdatedBy = operBy;
        _uow.ProjContracts.Update(contract);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：合同附件删除（从 Controller 迁移到 Service）
    public async Task DeleteContractFileAsync(long contractId, string operBy)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId)
            ?? throw new NotFoundException("合同不存在");
        if (!string.IsNullOrEmpty(contract.FilePath) && File.Exists(contract.FilePath))
            File.Delete(contract.FilePath);
        contract.FilePath = null;
        contract.FileName = null;
        contract.UpdatedBy = operBy;
        _uow.ProjContracts.Update(contract);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：合同文件下载路径获取
    public async Task<(string? filePath, string? fileName)> GetContractFileAsync(long contractId)
    {
        var contract = await _uow.ProjContracts.GetByIdAsync(contractId);
        if (contract == null || string.IsNullOrEmpty(contract.FilePath) || !File.Exists(contract.FilePath))
            return (null, null);
        return (contract.FilePath, contract.FileName);
    }

    // ── 发票 ──────────────────────────────────────────────────
    public async Task<long> AddInvoiceAsync(CreateInvoiceDto dto, string operBy)
    {
        var invoice = new ProjectInvoice
        {
            ProjectId = dto.ProjectId,
            ReceiptName = dto.ReceiptName,
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

    // #18 新增：删除发票（从 Controller 迁移到 Service）
    public async Task DeleteInvoiceAsync(long invoiceId)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId)
            ?? throw new NotFoundException("发票不存在");
        _uow.ProjInvoices.SoftDelete(inv);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：发票文件上传（从 Controller 迁移到 Service）
    public async Task UploadInvoiceFileAsync(long invoiceId, string fileType,
        string fileName, string filePath, string operBy)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId)
            ?? throw new NotFoundException("发票不存在");
        if (fileType == "invoice")
        {
            if (!string.IsNullOrEmpty(inv.InvoiceFile) && File.Exists(inv.InvoiceFile))
                File.Delete(inv.InvoiceFile);
            inv.InvoiceFile = filePath;
            inv.InvoiceFileName = fileName;
        }
        else
        {
            if (!string.IsNullOrEmpty(inv.PaymentFile) && File.Exists(inv.PaymentFile))
                File.Delete(inv.PaymentFile);
            inv.PaymentFile = filePath;
            inv.PaymentFileName = fileName;
        }
        inv.UpdatedBy = operBy;
        _uow.ProjInvoices.Update(inv);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：发票文件下载路径获取
    public async Task<(string? filePath, string? fileName)> GetInvoiceFileAsync(long invoiceId, string fileType)
    {
        var inv = await _uow.ProjInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return (null, null);
        var (fp, fn) = fileType == "invoice"
            ? (inv.InvoiceFile, inv.InvoiceFileName)
            : (inv.PaymentFile, inv.PaymentFileName);
        if (string.IsNullOrEmpty(fp) || !File.Exists(fp)) return (null, null);
        return (fp, fn);
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
        if (File.Exists(f.FilePath)) File.Delete(f.FilePath);
        _uow.ProjFiles.SoftDelete(f);
        await _uow.SaveChangesAsync();
    }

    // #18 新增：文件下载路径获取
    public async Task<(string? filePath, string? fileName, string? fileExt)?> GetFileAsync(long fileId)
    {
        var f = await _uow.ProjFiles.GetByIdAsync(fileId);
        if (f == null || !File.Exists(f.FilePath)) return null;
        return (f.FilePath, f.FileName, f.FileExt);
    }

    // ── 操作日志（分页）───────────────────────────────────────
    // #15 新增：分页查询操作日志
    public async Task<PagedResult<ProjectLogDto>> GetLogsPagedAsync(long projectId, int page, int size)
    {
        var q = _uow.ProjLogs.Query().Where(l => l.ProjectId == projectId);
        var total = await q.CountAsync();
        var list = await q.OrderByDescending(l => l.OperAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync();
        return new PagedResult<ProjectLogDto>
        {
            Items = _mapper.Map<List<ProjectLogDto>>(list),
            Total = total,
            Page = page,
            PageSize = size
        };
    }

    // ── 统计 ─────────────────────────────────────────────────
    public async Task<object> GetMyStatsAsync(long employeeId)
    {
        // 找该员工参与的所有项目成员记录
        var memberships = await _uow.ProjMembers.Query()
            .Include(m => m.Project)
            .Where(m => m.EmployeeId == employeeId && m.Status == 0)
            .ToListAsync();

        var projectIds = memberships.Where(m => m.Project != null).Select(m => m.ProjectId).ToList();

        // 批量查询所有项目的回款总额，避免 N+1 查询
        var accTotals = await _uow.Acceptances.Query()
            .Where(a => projectIds.Contains(a.ProjectId))
            .GroupBy(a => a.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Sum(a => a.AcceptAmount) })
            .ToListAsync();
        var invTotals = await _uow.ProjInvoices.Query()
            .Where(i => projectIds.Contains(i.ProjectId) && i.IsReceived)
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Sum(i => i.Amount) })
            .ToListAsync();
        var receivedByProject = accTotals.ToDictionary(a => a.ProjectId, a => a.Total)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value + invTotals.FirstOrDefault(i => i.ProjectId == kvp.Key)?.Total ?? 0);

        var stats = new List<object>();
        foreach (var ms in memberships)
        {
            if (ms.Project == null) continue;
            var proj = ms.Project;
            var received = receivedByProject.GetValueOrDefault(proj.Id, 0m);
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

    // #7 新增：文件扩展名白名单校验
    public static bool IsFileExtensionAllowed(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.TrimStart('.').ToLower() ?? "";
        return AllowedFileExts.Contains(ext);
    }

    // #6 新增：fileType 参数白名单校验
    public static bool IsValidFileType(string fileType)
        => fileType is "invoice" or "payment";

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

    // ── 数据权限：按角色 DataScope 隔离项目（部门 / 子部门 / 本人成员）──
    // DataScope: 1=全部 2=本部门 3=本部门及子部门 4=仅本人参与项目
    private async Task<IQueryable<Project>> ApplyDataScopeAsync(IQueryable<Project> q, long operUserId)
    {
        var (dataScope, userDeptId) = await _permSvc.GetUserDataScopeAsync(operUserId);

        if (dataScope == 1)
            return q; // 全部数据

        if (dataScope == 3 && userDeptId.HasValue)
        {
            var all = await _uow.Depts.GetListAsync(d => !d.IsDeleted);
            var deptIds = GetSelfAndChildDeptIds(all, userDeptId.Value);
            return q.Where(p => p.DeptId.HasValue && deptIds.Contains(p.DeptId.Value));
        }

        if (dataScope == 2 && userDeptId.HasValue)
            return q.Where(p => p.DeptId == userDeptId);

        // 仅本人参与的项目（DataScope=4 或无部门信息时）
        var empId = await _uow.Users.Query()
            .Where(u => u.Id == operUserId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync();
        if (!empId.HasValue)
            return q.Take(0);

        var memberQuery = _uow.ProjMembers.Query();
        return q.Where(p => memberQuery.Any(m =>
            m.ProjectId == p.Id && m.EmployeeId == empId.Value && m.Status == 0));
    }

    // ── 成果报告：把项目详情映射为模板占位符键值（{{键}}）────────────
    // 模板中使用相同中文键名即可被自动填充；模块类键值为多行文本。
    public Dictionary<string, string> BuildReportFieldValues(ProjectDetailDto p)
    {
        string F(object? v) => v == null ? "" : v.ToString()!;
        var ms = new Dictionary<string, string>
        {
            // 单值字段
            ["项目编号"]       = p.ProjNo,
            ["项目名称"]       = p.ProjName,
            ["项目业主"]       = p.OwnerName,
            ["业主联系人"]     = p.OwnerContact ?? "",
            ["业主电话"]       = p.OwnerPhone ?? "",
            ["业务类型"]       = p.BizType,
            ["采购方式"]       = p.ProcurementType ?? "",
            ["限价金额"]       = p.LimitPrice.HasValue ? $"{p.LimitPrice:N2} 万元" : "",
            ["建设规模"]       = p.BuildingScale ?? "",
            ["承接部门"]       = p.DeptName ?? "",
            ["技术负责人"]     = p.TechLeaderName ?? "",
            ["商务负责人"]     = p.BizLeaderName ?? "",
            ["合同金额"]       = $"{p.ContractAmount:N2} 万元",
            ["实际合同金额"]   = $"{p.ActualAmount:N2} 万元",
            ["是否联合体"]     = p.IsJointVenture ? "是" : "否",
            ["我方占比"]       = p.OurRatio.HasValue ? $"{p.OurRatio}%" : "",
            ["签约日期"]       = p.SignDate?.ToString("yyyy-MM-dd") ?? "",
            ["计划完成日期"]   = p.PlanEndDate?.ToString("yyyy-MM-dd") ?? "",
            ["实际完成日期"]   = p.ActualEndDate?.ToString("yyyy-MM-dd") ?? "",
            ["项目状态"]       = p.ProgressText,
            ["备注"]           = p.Remark ?? "",
            ["已收款"]         = $"{p.TotalReceived:N2} 万元",
            ["收款率"]         = (p.ActualAmount > 0 ? p.TotalReceived / p.ActualAmount * 100 : 0).ToString("N1") + "%",
            ["服务团队人数"]   = p.Members.Count(m => m.Status == 0).ToString(),
            // 模块字段（多行文本）
            ["成员明细"]       = string.Join("\n", p.Members
                .Where(m => m.Status == 0)
                .Select(m => $"· {m.EmployeeName}（{m.Role}） 占比 {m.Ratio}%  职责：{m.DutyDesc ?? "-"}")),
            ["里程碑明细"]     = string.Join("\n", p.Milestones
                .OrderBy(m => m.Sort)
                .Select(m => $"· {m.MilestoneName}  计划 {m.PlanDate:yyyy-MM-dd}  负责人 {m.OwnerName ?? "-"}  状态 {MilestoneStatusText(m.Status)}")),
            ["合同明细"]       = string.Join("\n", p.Contracts
                .Select(c => $"· {c.ContractNo}（{c.ContractType}） 甲方 {c.PartyA}  金额 {c.Amount:N2} 万元  签约 {c.SignDate?.ToString("yyyy-MM-dd") ?? "-"}")),
            ["回款明细"]       = string.Join("\n", p.Invoices
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => $"· {i.ReceiptName}  金额 {i.Amount:N2} 万元  {(i.IsReceived ? "已收" : "待收")}  开票 {i.InvoiceDate?.ToString("yyyy-MM-dd") ?? "-"}")),
            ["验收明细"]       = string.Join("\n", p.Acceptances
                .Select(a => $"· {a.AcceptBatch}  金额 {a.AcceptAmount:N2} 万元  日期 {a.AcceptDate:yyyy-MM-dd}")),
        };
        return ms;
    }

    private static string MilestoneStatusText(int status) => status switch
    {
        0 => "待开始", 1 => "进行中", 2 => "已完成", _ => "未知"
    };

    // #20 修复：状态文本统一维护
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

    // #20 新增：状态 Badge 样式统一维护
    public static string GetProgressBadge(int status) => status switch
    {
        0 or 1 => "badge-secondary",
        2 or 3 => "badge-warning",
        4 or 5 => "badge-info",
        6 or 7 => "badge-primary",
        8      => "badge-success",
        9      => "badge-danger",
        _      => "badge-light",
    };
}
