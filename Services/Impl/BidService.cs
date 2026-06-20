using System.Runtime.CompilerServices;
using AutoMapper;
using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Bid;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Enums;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.AI;
using EnterpriseMS.Services.AI.Models;
using EnterpriseMS.Services.DTOs.Bid;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

public class BidService : IBidService
{
    private readonly IRepository<BidProject> _bidProjectRepo;
    private readonly IRepository<BidRequirement> _requirementRepo;
    private readonly IRepository<BidDocument> _documentRepo;
    private readonly IRepository<BidTemplate> _templateRepo;
    private readonly IRepository<Employee> _employeeRepo;
    private readonly IRepository<EmployeeCertificate> _certRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAIService _aiService;
    private readonly DocumentParser _parser;
    private readonly IMapper _mapper;
    private readonly ILogger<BidService> _logger;

    public BidService(
        IRepository<BidProject> bidProjectRepo,
        IRepository<BidRequirement> requirementRepo,
        IRepository<BidDocument> documentRepo,
        IRepository<BidTemplate> templateRepo,
        IRepository<Employee> employeeRepo,
        IRepository<EmployeeCertificate> certRepo,
        IUnitOfWork uow,
        IAIService aiService,
        DocumentParser parser,
        IMapper mapper,
        ILogger<BidService> logger)
    {
        _bidProjectRepo = bidProjectRepo;
        _requirementRepo = requirementRepo;
        _documentRepo = documentRepo;
        _templateRepo = templateRepo;
        _employeeRepo = employeeRepo;
        _certRepo = certRepo;
        _uow = uow;
        _aiService = aiService;
        _parser = parser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<BidProjectDto>> GetPagedAsync(BidListQuery query)
    {
        var result = await _bidProjectRepo.GetPagedAsync(
            query.Page,
            query.PageSize,
            predicate: BuildPredicate(query),
            orderBy: e => e.CreatedAt,
            descending: true);

        return new PagedResult<BidProjectDto>
        {
            Items = _mapper.Map<List<BidProjectDto>>(result.Items),
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<BidProjectDto?> GetDetailAsync(long id)
    {
        var entity = await _bidProjectRepo.GetByIdAsync(id);
        if (entity == null) return null;

        var dto = _mapper.Map<BidProjectDto>(entity);

        var requirements = await _requirementRepo.GetListAsync(r => r.BidProjectId == id);
        dto.Requirements = _mapper.Map<List<BidRequirementDto>>(requirements);

        var documents = await _documentRepo.GetListAsync(d => d.BidProjectId == id);
        dto.Documents = _mapper.Map<List<BidDocumentDto>>(documents);

        return dto;
    }

    public async Task<long> CreateAsync(BidProjectCreateDto dto, string operBy)
    {
        var entity = new BidProject
        {
            ProjectId = dto.ProjectId,
            ProjectName = "",
            ProjectCode = "",
            Tenderer = dto.Tenderer,
            Budget = dto.Budget,
            Deadline = dto.Deadline,
            Status = (int)BidProjectStatus.Draft,
            Remark = dto.Remark,
            CreatedBy = operBy
        };

        await _bidProjectRepo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(long id, BidProjectUpdateDto dto, string operBy)
    {
        var entity = await _bidProjectRepo.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException($"投标项目不存在: {id}");

        if (dto.Tenderer != null) entity.Tenderer = dto.Tenderer;
        if (dto.Budget != null) entity.Budget = dto.Budget;
        if (dto.Deadline != null) entity.Deadline = dto.Deadline;
        if (dto.Status != null) entity.Status = dto.Status.Value;
        if (dto.Remark != null) entity.Remark = dto.Remark;
        entity.UpdatedBy = operBy;

        _bidProjectRepo.Update(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id, string operBy)
    {
        var entity = await _bidProjectRepo.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException($"投标项目不存在: {id}");

        _bidProjectRepo.SoftDelete(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<BidAnalysisResult> AnalyzeBidDocumentAsync(BidAnalyzeRequest request)
    {
        string content;
        using (var stream = request.File.OpenReadStream())
        {
            content = _parser.Parse(stream, request.File.FileName);
        }

        return await _aiService.AnalyzeBidDocumentAsync(content);
    }

    public async Task SaveAnalysisResultAsync(long bidProjectId, BidAnalysisResult result)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(bidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {bidProjectId}");

        bidProject.ProjectName = result.ProjectName;
        bidProject.ProjectCode = result.ProjectCode;
        bidProject.Tenderer = result.Tenderer;
        bidProject.Budget = result.Budget;
        bidProject.Deadline = result.Deadline;
        bidProject.Status = (int)BidProjectStatus.Analyzing;

        _bidProjectRepo.Update(bidProject);

        var existingReqs = await _requirementRepo.GetListAsync(r => r.BidProjectId == bidProjectId);
        foreach (var req in existingReqs)
            _requirementRepo.SoftDelete(req);

        var allRequirements = new List<(string Category, string Content, int? Score)>();
        result.Qualifications.ForEach(q => allRequirements.Add(("资质要求", q, null)));
        result.TechnicalRequirements.ForEach(q => allRequirements.Add(("技术要求", q, null)));
        result.CommercialRequirements.ForEach(q => allRequirements.Add(("商务要求", q, null)));
        result.ScoringCriteria.ForEach(s => allRequirements.Add(("评分标准", s.Description ?? s.Item, s.MaxScore)));

        foreach (var (category, content, score) in allRequirements)
        {
            await _requirementRepo.AddAsync(new BidRequirement
            {
                BidProjectId = bidProjectId,
                Category = category,
                Content = content,
                ScoreWeight = score,
                CreatedBy = "AI"
            });
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<BidDocumentDto> GenerateChapterAsync(BidGenerateRequest request)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(request.BidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {request.BidProjectId}");

        var requirements = await _requirementRepo.GetListAsync(r => r.BidProjectId == request.BidProjectId);
        var requirementsText = string.Join("\n", requirements.Select(r => $"[{r.Category}] {r.Content}"));

        var chapterRequest = new BidChapterRequest
        {
            ChapterName = request.ChapterName,
            ProjectName = bidProject.ProjectName,
            ProjectDescription = bidProject.Remark,
            Requirements = requirementsText,
            ScoringCriteria = requirements.Where(r => r.Category == "评分标准").Select(r => r.Content).ToList(),
            TargetWordCount = request.TargetWordCount,
            CustomRequirements = request.CustomRequirements
        };

        var content = await _aiService.GenerateChapterAsync(chapterRequest);

        // 删除同名旧文档，再创建新文档
        var existingDocs = await _documentRepo.GetListAsync(
            d => d.BidProjectId == request.BidProjectId && d.ChapterName == request.ChapterName);
        foreach (var oldDoc in existingDocs)
        {
            _documentRepo.SoftDelete(oldDoc);
        }

        var document = new BidDocument
        {
            BidProjectId = request.BidProjectId,
            ChapterName = request.ChapterName,
            ChapterType = request.ChapterType,
            Content = content,
            Status = (int)BidDocumentStatus.AiGenerated,
            WordCount = content.Length,
            CreatedBy = "AI"
        };

        await _documentRepo.AddAsync(document);
        await _uow.SaveChangesAsync();
        return _mapper.Map<BidDocumentDto>(document);
    }

    public async IAsyncEnumerable<string> GenerateChapterStreamAsync(BidGenerateRequest request)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(request.BidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {request.BidProjectId}");

        var requirements = await _requirementRepo.GetListAsync(r => r.BidProjectId == request.BidProjectId);
        var requirementsText = string.Join("\n", requirements.Select(r => $"[{r.Category}] {r.Content}"));

        var chapterRequest = new BidChapterRequest
        {
            ChapterName = request.ChapterName,
            ProjectName = bidProject.ProjectName,
            ProjectDescription = bidProject.Remark,
            Requirements = requirementsText,
            ScoringCriteria = requirements.Where(r => r.Category == "评分标准").Select(r => r.Content).ToList(),
            TargetWordCount = request.TargetWordCount,
            CustomRequirements = request.CustomRequirements
        };

        await foreach (var chunk in _aiService.GenerateChapterStreamAsync(chapterRequest))
        {
            yield return chunk;
        }
    }

    public async Task<List<BidDocumentDto>> GenerateFullBidAsync(BidGenerateFullRequest request)
    {
        var chapters = request.Chapters ?? new List<string>
        {
            "技术方案", "商务方案", "项目管理", "人员配置", "质量保证", "售后服务"
        };

        var results = new List<BidDocumentDto>();

        foreach (var chapter in chapters)
        {
            var chapterRequest = new BidGenerateRequest
            {
                BidProjectId = request.BidProjectId,
                ChapterName = chapter,
                TargetWordCount = request.TargetWordCount
            };

            var result = await GenerateChapterAsync(chapterRequest);
            results.Add(result);
        }

        var bidProject = await _bidProjectRepo.GetByIdAsync(request.BidProjectId);
        if (bidProject != null)
        {
            bidProject.Status = (int)BidProjectStatus.Generating;
            _bidProjectRepo.Update(bidProject);
            await _uow.SaveChangesAsync();
        }

        return results;
    }

    public async Task<BidReviewResult> ReviewBidAsync(BidReviewRequest request)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(request.BidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {request.BidProjectId}");

        var documents = await _documentRepo.GetListAsync(d => d.BidProjectId == request.BidProjectId);
        var bidContent = string.Join("\n\n", documents.Select(d => $"## {d.ChapterName}\n{d.Content}"));

        var requirements = await _requirementRepo.GetListAsync(r => r.BidProjectId == request.BidProjectId);
        var requirementsText = string.Join("\n", requirements.Select(r => $"[{r.Category}] {r.Content}"));

        return await _aiService.ReviewBidDocumentAsync(bidContent, requirementsText);
    }

    public async Task UpdateDocumentContentAsync(long documentId, string content)
    {
        var doc = await _documentRepo.GetByIdAsync(documentId);
        if (doc == null) throw new NotFoundException($"文档不存在: {documentId}");

        doc.Content = content;
        doc.WordCount = content.Length;
        doc.Status = (int)BidDocumentStatus.Reviewed;

        _documentRepo.Update(doc);
        await _uow.SaveChangesAsync();
    }

    public async Task<BidDocumentDto?> GetDocumentAsync(long docId)
    {
        var doc = await _documentRepo.GetByIdAsync(docId);
        if (doc == null) return null;
        return _mapper.Map<BidDocumentDto>(doc);
    }

    public async Task SaveStreamedDocumentAsync(long bidProjectId, string chapterName, int chapterType, string content)
    {
        // 删除同名旧文档
        var existingDocs = await _documentRepo.GetListAsync(
            d => d.BidProjectId == bidProjectId && d.ChapterName == chapterName);
        foreach (var oldDoc in existingDocs)
        {
            _documentRepo.SoftDelete(oldDoc);
        }

        // 创建新文档
        var document = new BidDocument
        {
            BidProjectId = bidProjectId,
            ChapterName = chapterName,
            ChapterType = chapterType,
            Content = content,
            Status = (int)BidDocumentStatus.AiGenerated,
            WordCount = content.Length,
            CreatedBy = "AI"
        };

        await _documentRepo.AddAsync(document);
        await _uow.SaveChangesAsync();
    }

    public async Task<BidAssembleResult> AssembleBidDocumentAsync(long bidProjectId, string part)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(bidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {bidProjectId}");

        var documents = await _documentRepo.GetListAsync(d => d.BidProjectId == bidProjectId);
        documents = documents.OrderBy(d => d.SortOrder).ToList();

        if (!documents.Any()) throw new BusinessException("暂无章节内容，请先生成标书");

        var technicalChapters = new List<BidAssembleChapter>();
        var commercialChapters = new List<BidAssembleChapter>();

        // 技术部分：技术方案、项目管理、人员配置、质量保证、售后服务
        var technicalNames = new[] { "技术方案", "项目管理", "人员配置", "质量保证", "售后服务" };
        // 商务部分：商务方案
        var commercialNames = new[] { "商务方案" };

        foreach (var doc in documents)
        {
            var chapter = new BidAssembleChapter
            {
                Name = doc.ChapterName,
                Content = doc.Content ?? "",
                WordCount = doc.WordCount ?? doc.Content?.Length ?? 0
            };

            if (technicalNames.Any(n => doc.ChapterName.Contains(n)))
                technicalChapters.Add(chapter);
            else if (commercialNames.Any(n => doc.ChapterName.Contains(n)))
                commercialChapters.Add(chapter);
            else
            {
                // 未分类的章节根据类型判断
                if (doc.ChapterType <= 1) // 技术相关
                    technicalChapters.Add(chapter);
                else
                    commercialChapters.Add(chapter);
            }
        }

        var result = new BidAssembleResult
        {
            ProjectName = bidProject.ProjectName,
            AssembleTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (part == "all" || part == "technical")
        {
            result.TechnicalPart = BuildPart("技术部分", technicalChapters);
        }

        if (part == "all" || part == "commercial")
        {
            result.CommercialPart = BuildPart("商务部分", commercialChapters);
        }

        if (part == "all")
        {
            var allChapters = technicalChapters.Concat(commercialChapters).ToList();
            result.FullDocument = BuildPart("投标文件", allChapters);
        }

        return result;
    }

    private BidAssemblePart BuildPart(string title, List<BidAssembleChapter> chapters)
    {
        var content = string.Join("\n\n", chapters.Select(c => c.Content));
        return new BidAssemblePart
        {
            Title = title,
            Content = content,
            WordCount = content.Length,
            Chapters = chapters
        };
    }

    public async Task<PersonnelMatchResult> MatchPersonnelAsync(PersonnelMatchRequest request)
    {
        var bidProject = await _bidProjectRepo.GetByIdAsync(request.BidProjectId);
        if (bidProject == null) throw new NotFoundException($"投标项目不存在: {request.BidProjectId}");

        // 获取招标要求中的资质要求
        var requirements = await _requirementRepo.GetListAsync(r => r.BidProjectId == request.BidProjectId);
        var qualificationReqs = requirements
            .Where(r => r.Category.Contains("资质") || r.Category.Contains("人员"))
            .Select(r => r.Content)
            .ToList();

        // 获取所有在职员工及其证书
        var employees = await _employeeRepo.GetListAsync(e => e.Status == 1); // 1=在职
        var matchedPersonnel = new List<MatchedPersonnel>();

        foreach (var emp in employees)
        {
            var certs = await _certRepo.GetListAsync(c => c.EmployeeId == emp.Id && c.Status == 0); // 0=有效
            var empMatch = new MatchedPersonnel
            {
                EmployeeId = emp.Id,
                Name = emp.RealName,
                DeptName = emp.Dept?.DeptName,
                Education = emp.Education,
                WorkYears = emp.EntryDate.HasValue ? (int)((DateTime.Now - emp.EntryDate.Value).TotalDays / 365) : 0,
                Certificates = certs.Select(c => new MatchedCertificate
                {
                    CertName = c.CertName,
                    CertNo = c.CertNo,
                    IssueOrg = c.IssueOrg,
                    ExpireDate = c.ExpireDate
                }).ToList(),
                MatchedRequirements = new List<string>()
            };

            // 匹配资质要求
            foreach (var req in qualificationReqs)
            {
                if (certs.Any(c => MatchesRequirement(c.CertName, req)) ||
                    MatchesText(emp.Education, req) ||
                    MatchesText(emp.Remark, req))
                {
                    empMatch.MatchedRequirements.Add(req);
                }
            }

            empMatch.MatchScore = empMatch.MatchedRequirements.Count * 10 + certs.Count * 5 + empMatch.WorkYears;
            matchedPersonnel.Add(empMatch);
        }

        // 按匹配分数排序，取前N名
        var topPersonnel = matchedPersonnel
            .Where(p => p.MatchedRequirements.Any())
            .OrderByDescending(p => p.MatchScore)
            .Take(request.MaxPersonnel)
            .ToList();

        // 找出未匹配的要求
        var matchedReqs = topPersonnel.SelectMany(p => p.MatchedRequirements).Distinct().ToList();
        var unmatched = qualificationReqs.Where(r => !matchedReqs.Contains(r)).ToList();

        return new PersonnelMatchResult
        {
            MatchedPersonnel = topPersonnel,
            UnmatchedRequirements = unmatched,
            Summary = $"共匹配 {topPersonnel.Count} 名符合条件的人员，{unmatched.Count} 项要求未找到匹配人员"
        };
    }

    public async Task<string> GeneratePersonnelSectionAsync(PersonnelMatchRequest request)
    {
        var matchResult = await MatchPersonnelAsync(request);

        var prompt = $@"
请根据以下人员信息，撰写投标文件的「人员配置」章节：

---匹配的人员列表---
{string.Join("\n", matchResult.MatchedPersonnel.Select(p => $"姓名：{p.Name}，部门：{p.DeptName}，学历：{p.Education}，工作年限：{p.WorkYears}年，证书：{string.Join("、", p.Certificates.Select(c => c.CertName))}"))}
---结束---

---未匹配的要求---
{(matchResult.UnmatchedRequirements.Any() ? string.Join("\n", matchResult.UnmatchedRequirements) : "无")}
---结束---

请撰写人员配置章节，包括：
1. 项目组织架构
2. 核心人员介绍（姓名、职务、资质、经验）
3. 人员配置满足招标要求的说明
";

        var chapterRequest = new BidChapterRequest
        {
            ChapterName = "人员配置",
            ProjectName = (await _bidProjectRepo.GetByIdAsync(request.BidProjectId))?.ProjectName ?? "",
            Requirements = prompt,
            TargetWordCount = 3000
        };

        return await _aiService.GenerateChapterAsync(chapterRequest);
    }

    private bool MatchesRequirement(string certName, string requirement)
    {
        if (string.IsNullOrEmpty(certName) || string.IsNullOrEmpty(requirement)) return false;
        certName = certName.ToLower();
        requirement = requirement.ToLower();

        // 更灵活的匹配：证书名称或要求中包含相关关键词即可
        var certKeywords = new[] { "注册", "工程师", "咨询", "规划", "造价", "监理", "建造", "结构", "建筑", "设计" };
        var reqKeywords = new[] { "注册", "工程师", "咨询", "规划", "造价", "监理", "建造", "结构", "建筑", "设计", "资质" };

        // 如果证书名称包含某个关键词，且要求也包含相关关键词，则匹配
        return certKeywords.Any(ck => certName.Contains(ck) && reqKeywords.Any(rk => requirement.Contains(rk)));
    }

    private bool MatchesText(string? text, string requirement)
    {
        if (string.IsNullOrEmpty(text)) return false;
        text = text.ToLower();
        requirement = requirement.ToLower();

        var keywords = new[] { "本科", "硕士", "博士", "高级", "中级", "注册" };
        return keywords.Any(k => text.Contains(k) && requirement.Contains(k));
    }

    private System.Linq.Expressions.Expression<Func<BidProject, bool>>? BuildPredicate(BidListQuery query)
    {
        if (query.ProjectId.HasValue || query.Status.HasValue || !string.IsNullOrEmpty(query.Keyword))
        {
            return e =>
                (!query.ProjectId.HasValue || e.ProjectId == query.ProjectId.Value) &&
                (!query.Status.HasValue || e.Status == query.Status.Value) &&
                (string.IsNullOrEmpty(query.Keyword) || e.ProjectName.Contains(query.Keyword));
        }
        return null;
    }
}
