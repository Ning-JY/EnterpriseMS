using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Constants;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.Hr;
using EnterpriseMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;

namespace EnterpriseMS.Controllers.Hr;

// ── 员工信息 ──────────────────────────────────────────────────
[Authorize, Route("hr/employee")]
public class EmployeeController : BaseAuthController
{
    private readonly IEmployeeService _empSvc;
    private readonly IDeptService _deptSvc;
    private readonly IDictService _dictSvc;

    public EmployeeController(IEmployeeService empSvc, IDeptService deptSvc,
        IDictService dictSvc, IPermissionService permSvc)
        : base(permSvc)
    {
        _empSvc = empSvc; _deptSvc = deptSvc; _dictSvc = dictSvc;
    }

    [HasPermission("hr:employee:list")]
    public IActionResult Index()
    {
        // 列表数据由 /hr/employee/list (AJAX) 提供，本页仅做容器。
        return View();
    }

    // ── AJAX 列表数据（新标准 layui 表格）──
    [HttpGet("list")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> List(string? keyword, long? deptId, int? status, int page = 1, int size = 15)
    {
        var query = new EmployeeQueryDto
        {
            Keyword = keyword,
            DeptId = deptId,
            Status = status,
            Page = page,
            Size = size
        };
        var paged = await _empSvc.GetPagedAsync(query);
        var items = paged.Items.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            EmpNo = e.EmpNo,
            RealName = e.RealName,
            Gender = e.Gender,
            Phone = e.Phone,
            DeptId = e.DeptId,
            DeptName = e.Dept?.DeptName,
            Status = e.Status,
            EntryDate = e.EntryDate,
            Avatar = e.ProfilePhoto
        }).ToList();
        return ApiOk(new PagedResult<EmployeeListDto>
        {
            Items = items,
            Total = paged.Total,
            Page = page,
            PageSize = size
        });
    }

    [HttpGet("detail/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var emp = await _empSvc.GetDetailAsync(id);
        if (emp == null)
        {
            return NotFound();
        }

        ViewBag.CertTypes = await _dictSvc.GetDataByTypeAsync(DictType.CertType);
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync(DictType.ContractType);
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _empSvc.GetPostsAsync();
        return View(emp);
    }

    // 供 User/Index 员工弹窗调用 - 返回 JSON
    [HttpGet("json/{id}")]
    public async Task<IActionResult> GetJson(long id)
    {
        var emp = await _empSvc.GetByIdAsync(id);
        if (emp == null)
        {
            return ApiFail("员工不存在");
        }

        return ApiOk(new
        {
            emp.Id,
            emp.EmpNo,
            emp.RealName,
            emp.Gender,
            emp.Phone,
            emp.Email,
            emp.IdCard,
            emp.DeptId,
            DeptName = emp.Dept?.DeptName,
            emp.PostId,
            emp.Status,
            emp.Remark,
            EntryDate = emp.EntryDate?.ToString("yyyy-MM-dd"),
            FormalDate = emp.FormalDate?.ToString("yyyy-MM-dd"),
            ProbationEndDate = emp.ProbationEndDate?.ToString("yyyy-MM-dd"),
        });
    }

    [HttpGet("edit/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var emp = await _empSvc.GetDetailAsync(id);
        if (emp == null)
        {
            return NotFound();
        }

        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _empSvc.GetPostsAsync();
        var empStatusOpts = await _dictSvc.GetDataByTypeAsync(DictType.EmployeeStatus);
        ViewBag.EmployeeStatusOptions = empStatusOpts;
        var empStatusMap = new Dictionary<int, string>();
        foreach (var o in empStatusOpts)
        {
            if (int.TryParse(o.DictValue, out var v))
            {
                empStatusMap[v] = o.DictLabel;
            }
        }

        ViewBag.EmployeeStatusMap = empStatusMap;
        return View(emp);
    }

    // 供详情页卡片内「编辑」弹窗预填：返回该员工全部档案字段
    [HttpGet("full/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Full(long id)
    {
        var emp = await _empSvc.GetByIdAsync(id);
        if (emp == null) return ApiFail("员工不存在");
        return ApiOk(new
        {
            emp.Id, emp.RealName, emp.Gender, emp.Phone, emp.Email, emp.IdCard,
            emp.Nationality, BirthDate = emp.BirthDate?.ToString("yyyy-MM-dd"),
            emp.PoliticalStatus, emp.NativePlace, emp.Address,
            emp.EmergencyContact, emp.EmergencyPhone,
            emp.DeptId, emp.PostId,
            EntryDate = emp.EntryDate?.ToString("yyyy-MM-dd"),
            ProbationEndDate = emp.ProbationEndDate?.ToString("yyyy-MM-dd"),
            FormalDate = emp.FormalDate?.ToString("yyyy-MM-dd"),
            WorkStartDate = emp.WorkStartDate?.ToString("yyyy-MM-dd"),
            emp.SocialInsuranceNo, emp.BankAccount, emp.BankName,
            emp.Education, emp.HighestDegree, emp.GraduateSchool, emp.Major,
            emp.TechnicalTitle, emp.TechnicalLevel
        });
    }

    // 头像上传：保存文件并写回 ProfilePhoto
    [HttpPost("avatar-upload")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> AvatarUpload(long id, IFormFile file)
    {
        try
        {
            var emp = await _empSvc.GetByIdAsync(id);
            if (emp == null) return ApiFail("员工不存在");
            var saved = await FileUploadHelper.SaveUploadFile(file, "hr/avatars");
            if (saved == null) return ApiFail("文件上传失败（格式或大小不合规）");
            await _empSvc.UpdateProfilePhotoAsync(id, saved.Value.path, User.GetRealName());
            return ApiOk(new { path = saved.Value.path }, "头像已更新");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    // 头像图片输出（文件存于 wwwroot 之外，必须经此端点返回）
    [HttpGet("avatar/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Avatar(long id)
    {
        var emp = await _empSvc.GetByIdAsync(id);
        if (emp?.ProfilePhoto == null || !global::System.IO.File.Exists(emp.ProfilePhoto))
            return NotFound();
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(emp.ProfilePhoto, out var contentType))
            contentType = "application/octet-stream";
        return PhysicalFile(emp.ProfilePhoto, contentType);
    }

    // ── 新增（GET）：复用 Edit 视图，传入空模型 ──
    [HttpGet("create")]
    [HasPermission("hr:employee:add")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _empSvc.GetPostsAsync();
        var empStatusOpts = await _dictSvc.GetDataByTypeAsync(DictType.EmployeeStatus);
        ViewBag.EmployeeStatusOptions = empStatusOpts;
        var empStatusMap = new Dictionary<int, string>();
        foreach (var o in empStatusOpts)
        {
            if (int.TryParse(o.DictValue, out var v))
            {
                empStatusMap[v] = o.DictLabel;
            }
        }
        ViewBag.EmployeeStatusMap = empStatusMap;
        return View("Edit", new Employee());
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:add")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RealName))
        {
            return ApiFail("姓名不能为空");
        }

        try
        {
            var id = await _empSvc.CreateAsync(new CreateEmployeeDto
            {
                RealName = req.RealName,
                Gender = req.Gender,
                Phone = req.Phone,
                Email = req.Email,
                IdCard = req.IdCard,
                DeptId = req.DeptId,
                PostId = req.PostId,
                EntryDate = req.EntryDate,
                ProbationEndDate = req.ProbationEndDate,
                Remark = req.Remark,
            }, User.GetRealName());
            return ApiOk(new { id }, "员工信息已保存");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest req)
    {
        try
        {
            await _empSvc.UpdateAsync(new UpdateEmployeeDto
            {
                Id = req.Id,
                RealName = req.RealName,
                Gender = req.Gender,
                Phone = req.Phone,
                Email = req.Email,
                IdCard = req.IdCard,
                DeptId = req.DeptId,
                PostId = req.PostId,
                EntryDate = req.EntryDate,
                ProbationEndDate = req.ProbationEndDate,
                Remark = req.Remark,
                BirthDate = req.BirthDate,
                NativePlace = req.NativePlace,
                Education = req.Education,
                Major = req.Major,
                GraduateSchool = req.GraduateSchool,
                EmergencyContact = req.EmergencyContact,
                EmergencyPhone = req.EmergencyPhone,
                Address = req.Address,
                BankAccount = req.BankAccount,
                BankName = req.BankName,
                Nationality = req.Nationality,
                PoliticalStatus = req.PoliticalStatus,
                HighestDegree = req.HighestDegree,
                WorkStartDate = req.WorkStartDate,
                TechnicalTitle = req.TechnicalTitle,
                TechnicalLevel = req.TechnicalLevel,
                SocialInsuranceNo = req.SocialInsuranceNo,
                ProfilePhoto = req.ProfilePhoto,
            }, User.GetRealName());
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("formal")]
    [HasPermission("hr:employee:formal")]
    public async Task<IActionResult> Formal([FromBody] FormalRequest req)
    {
        try
        {
            await _empSvc.FormalAsync(req.Id, req.FormalDate, User.GetRealName());
            return ApiOk("转正操作成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    [HttpPost("leave")]
    [HasPermission("hr:employee:leave")]
    public async Task<IActionResult> Leave([FromBody] LeaveRequest req)
    {
        try
        {
            await _empSvc.LeaveAsync(req.Id, req.LeaveDate, req.Reason, User.GetRealName());
            return ApiOk("离职操作成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        { return ApiFail(ex.Message); }
    }

    // ── 分卡片部分更新（详情页卡片内编辑，只改对应卡片字段，避免整表覆盖）──
    [HttpPost("update-basic"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> UpdateBasic([FromBody] UpdateBasicDto dto)
    {
        try { await _empSvc.UpdateBasicAsync(dto, User.GetRealName()); return ApiOk("保存成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException) { return ApiFail(ex.Message); }
    }

    [HttpPost("update-job"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> UpdateJob([FromBody] UpdateJobDto dto)
    {
        try { await _empSvc.UpdateJobAsync(dto, User.GetRealName()); return ApiOk("保存成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException) { return ApiFail(ex.Message); }
    }

    [HttpPost("update-education"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> UpdateEducation([FromBody] UpdateEducationDto dto)
    {
        try { await _empSvc.UpdateEducationAsync(dto, User.GetRealName()); return ApiOk("保存成功"); }
        catch (Exception ex) when (ex is BusinessException or NotFoundException) { return ApiFail(ex.Message); }
    }

    // 供用户管理绑定员工使用
    [HttpGet("options")]
    [Authorize]
    public async Task<IActionResult> Options()
    {
        var emps = await _empSvc.GetOnJobAsync();
        var boundIds = await _empSvc.GetBoundEmployeeIdsAsync();
        var result = emps.Select(e => new
        {
            e.Id,
            e.RealName,
            DeptName = e.Dept?.DeptName,
            Display = e.Dept != null ? $"{e.RealName}（{e.Dept.DeptName}）" : e.RealName,
            IsBound = boundIds.Contains(e.Id),
        });
        return ApiOk(result);
    }
}

// ── 员工创建/更新请求模型 ──
public class CreateEmployeeRequest
{
    public string RealName { get; set; } = "";
    public int Gender { get; set; } = 1;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdCard { get; set; }
    public long? DeptId { get; set; }
    public long? PostId { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public string? Remark { get; set; }
    // ── 扩充字段（人事档案）──
    public DateTime? BirthDate { get; set; }
    public string? NativePlace { get; set; }
    public string? Education { get; set; }
    public string? Major { get; set; }
    public string? GraduateSchool { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Address { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Nationality { get; set; }
    public string? PoliticalStatus { get; set; }
    public string? HighestDegree { get; set; }
    public DateTime? WorkStartDate { get; set; }
    public string? TechnicalTitle { get; set; }
    public string? TechnicalLevel { get; set; }
    public string? SocialInsuranceNo { get; set; }
    public string? ProfilePhoto { get; set; }
}
public class UpdateEmployeeRequest : CreateEmployeeRequest { public long Id { get; set; } }

// ── 转正 / 离职请求模型（[FromBody] 接收 JSON，解决员工不存在问题）──
public class FormalRequest
{
    public long Id { get; set; }
    public DateTime FormalDate { get; set; }
}
public class LeaveRequest
{
    public long Id { get; set; }
    public DateTime LeaveDate { get; set; }
    public string? Reason { get; set; }
}
