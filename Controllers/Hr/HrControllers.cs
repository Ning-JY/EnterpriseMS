using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.Hr;

// ── 员工信息 ──────────────────────────────────────────────────
[Authorize, Route("hr/employee")]
public class EmployeeController : BaseAuthController
{
    private readonly IUnitOfWork    _uow;
    private readonly IDeptService   _deptSvc;
    private readonly IDictService   _dictSvc;
    private readonly IOperLogService _logSvc;

    public EmployeeController(IUnitOfWork uow, IDeptService deptSvc,
        IDictService dictSvc, IOperLogService logSvc,
        IPermissionService permSvc)
        : base(permSvc)
    {
        _uow = uow; _deptSvc = deptSvc; _dictSvc = dictSvc; _logSvc = logSvc;
    }

    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Index(string? keyword, long? deptId, int? status,
        int page = 1, int size = 15)
    {
        var q = _uow.Employees.Query()
            .Include(e => e.Dept).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(e => e.RealName.Contains(keyword) ||
                             e.EmpNo.Contains(keyword) ||
                             (e.Phone != null && e.Phone.Contains(keyword)));
        if (deptId.HasValue) q = q.Where(e => e.DeptId == deptId);
        if (status.HasValue)  q = q.Where(e => e.Status == status);
        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(e => e.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();
        ViewBag.Depts    = await _deptSvc.GetTreeAsync();
        ViewBag.Keyword  = keyword; ViewBag.DeptId = deptId; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Size = size; ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)size);
        return View(list);
    }

    [HttpGet("detail/{id}")]
    [HasPermission("hr:employee:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var emp = await _uow.Employees.Query()
            .Include(e => e.Dept)
            .Include(e => e.Contracts)
            .Include(e => e.Certificates)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emp == null) return NotFound();
        ViewBag.DictCertType  = await _dictSvc.GetDataByTypeAsync("cert_type");
        ViewBag.CertTypes     = await _dictSvc.GetDataByTypeAsync("cert_type");
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync("contract_type");
        ViewBag.Depts         = await _deptSvc.GetTreeAsync();
        return View(emp);
    }

    // 供User/Index员工弹窗调用 - 返回JSON
    [HttpGet("json/{id}")]
    public async Task<IActionResult> GetJson(long id)
    {
        var emp = await _uow.Employees.Query()
            .Include(e => e.Dept)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emp == null) return Json(ApiResult<object>.Fail("员工不存在"));
        return Json(ApiResult<object>.Ok(new {
            emp.Id, emp.EmpNo, emp.RealName, emp.Gender, emp.Phone, emp.Email,
            emp.IdCard, emp.DeptId, DeptName = emp.Dept?.DeptName,
            emp.PostId, emp.Status, emp.Remark,
            EntryDate = emp.EntryDate?.ToString("yyyy-MM-dd"),
            FormalDate = emp.FormalDate?.ToString("yyyy-MM-dd"),
            ProbationEndDate = emp.ProbationEndDate?.ToString("yyyy-MM-dd"),
        }));
    }

    [HttpGet("edit/{id}")]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var emp = await _uow.Employees.Query()
            .Include(e => e.Dept)
            .Include(e => e.Contracts)
            .Include(e => e.Certificates)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emp == null) return NotFound();
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        return View(emp);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:add")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RealName))
            return Json(ApiResult<object>.Fail("姓名不能为空"));
        var count = await _uow.Employees.CountAsync();
        var emp   = new Employee
        {
            EmpNo    = $"EMP{DateTime.Now.Year}{(count+1):D4}",
            RealName = req.RealName, Gender = req.Gender,
            Phone = req.Phone, Email = req.Email, IdCard = req.IdCard,
            DeptId = req.DeptId, PostId = req.PostId,
            Status = 0, EntryDate = req.EntryDate,
            ProbationEndDate = req.ProbationEndDate,
            Remark = req.Remark, CreatedBy = User.GetRealName(),
        };
        await _uow.Employees.AddAsync(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("新增员工", $"姓名：{emp.RealName}，工号：{emp.EmpNo}", "INSERT", emp.Id);
        return Json(ApiResult<object>.Ok(new { id = emp.Id }, "员工信息已保存"));
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("hr:employee:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest req)
    {
        var emp = await _uow.Employees.GetByIdAsync(req.Id);
        if (emp == null) return Json(ApiResult<object>.Fail("员工不存在"));
        emp.RealName         = req.RealName;  emp.Gender = req.Gender;
        emp.Phone            = req.Phone;    emp.Email  = req.Email; emp.IdCard = req.IdCard;
        emp.DeptId           = req.DeptId;   emp.PostId = req.PostId;
        emp.EntryDate        = req.EntryDate; emp.ProbationEndDate = req.ProbationEndDate;
        emp.Remark           = req.Remark;   emp.UpdatedBy = User.GetRealName();
        _uow.Employees.Update(emp);

        // 反向同步：若该员工已绑定登录账号，同步更新账号基本信息保持一致
        var boundUser = await _uow.Users.Query(false)
            .FirstOrDefaultAsync(u => u.EmployeeId == emp.Id);
        if (boundUser != null)
        {
            boundUser.RealName  = emp.RealName;
            boundUser.Phone     = emp.Phone;
            boundUser.Email     = emp.Email;
            boundUser.DeptId    = emp.DeptId;
            boundUser.PostId    = emp.PostId;
            boundUser.UpdatedBy = User.GetRealName();
            _uow.Users.Update(boundUser);
        }

        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("修改成功" + (boundUser != null ? "，已同步至登录账号" : "")));
    }

    [HttpPost("formal")]
    [HasPermission("hr:employee:formal")]
    public async Task<IActionResult> Formal(long id, DateTime formalDate)
    {
        var emp = await _uow.Employees.GetByIdAsync(id);
        if (emp == null) return Json(ApiResult<object>.Fail("员工不存在"));
        emp.Status = 1; emp.FormalDate = formalDate; emp.UpdatedBy = User.GetRealName();
        _uow.Employees.Update(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("员工转正", $"{emp.RealName} 转正日期：{formalDate:yyyy-MM-dd}", "UPDATE", id);
        return Json(ApiResult<object>.Ok("转正操作成功"));
    }

    [HttpPost("leave")]
    [HasPermission("hr:employee:leave")]
    public async Task<IActionResult> Leave(long id, DateTime leaveDate, string? reason)
    {
        var emp = await _uow.Employees.GetByIdAsync(id);
        if (emp == null) return Json(ApiResult<object>.Fail("员工不存在"));
        emp.Status = 2; emp.LeaveDate = leaveDate;
        emp.Remark = string.IsNullOrWhiteSpace(reason) ? emp.Remark : $"离职原因：{reason}";
        emp.UpdatedBy = User.GetRealName();
        _uow.Employees.Update(emp);
        await _uow.SaveChangesAsync();
        await _logSvc.LogAsync("员工离职", $"{emp.RealName} 离职日期：{leaveDate:yyyy-MM-dd}", "UPDATE", id);
        return Json(ApiResult<object>.Ok("离职操作成功"));
    }

    // 供用户管理绑定员工使用
    [HttpGet("options"), AllowAnonymous]
    public async Task<IActionResult> Options()
    {
        var emps = await _uow.Employees.Query()
            .Include(e => e.Dept)
            .Where(e => e.Status == 0 || e.Status == 1)
            .OrderBy(e => e.Dept != null ? e.Dept.Sort : 999)
            .ThenBy(e => e.RealName)
            .ToListAsync();
        var boundIds = await _uow.Users.Query()
            .Where(u => u.EmployeeId != null)
            .Select(u => u.EmployeeId!.Value)
            .ToListAsync();
        var result = emps.Select(e => new {
            e.Id, e.RealName,
            DeptName = e.Dept?.DeptName,
            Display  = e.Dept != null ? $"{e.RealName}（{e.Dept.DeptName}）" : e.RealName,
            IsBound  = boundIds.Contains(e.Id),
        });
        return Json(ApiResult<object>.Ok(result));
    }
}

// ── 合同管理 ──────────────────────────────────────────────────
[Authorize, Route("hr/contract")]
public class ContractController : BaseAuthController
{
    private readonly IUnitOfWork           _uow;
    private readonly IDictService          _dictSvc;
    private readonly IOperLogService       _logSvc;
    private readonly IEmployeeQueryService _empQrySvc;

    public ContractController(IUnitOfWork uow, IDictService dictSvc,
        IOperLogService logSvc, IEmployeeQueryService empQrySvc,
        IPermissionService permSvc)
        : base(permSvc)

    { _uow = uow; _dictSvc = dictSvc; _logSvc = logSvc; _empQrySvc = empQrySvc; }
    [HasPermission("hr:contract:list")]
    public async Task<IActionResult> Index(string? keyword, int? status, int page = 1, int size = 15)
    {
        var q = _uow.Contracts.Query().Include(c => c.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => (c.Employee != null && c.Employee.RealName.Contains(keyword)) ||
                             c.ContractNo.Contains(keyword));
        if (status.HasValue) q = q.Where(c => c.Status == status);
        var warnDate = DateTime.Today.AddDays(30);
        ViewBag.WarnCount = await q.CountAsync(c => c.Status == 0 && c.EndDate <= warnDate);
        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(c => c.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();
        ViewBag.ContractTypes = await _dictSvc.GetDataByTypeAsync("contract_type");
        ViewBag.Employees     = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Keyword = keyword; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Total = total; ViewBag.Size = size;
        return View(list);
    }

    [HttpPost("create-with-file")]
    [HasPermission("hr:contract:add")]
    public async Task<IActionResult> CreateWithFile(
        [FromForm] long EmployeeId, [FromForm] string ContractNo,
        [FromForm] string ContractType, [FromForm] DateTime StartDate,
        [FromForm] DateTime EndDate, [FromForm] DateTime? SignDate,
        [FromForm] string? Remark, IFormFile? file)
    {
        if (EmployeeId == 0 || string.IsNullOrWhiteSpace(ContractNo))
            return Json(ApiResult<object>.Fail("请填写员工和合同编号"));

        string? filePath = null; string? fileName = null;
        if (file != null && file.Length > 0)
        {
            var saved = await SaveUploadFile(file, "hr/contracts");
            if (saved.HasValue) { filePath = saved.Value.path; fileName = saved.Value.name; }
        }
        var contract = new EmployeeContract
        {
            EmployeeId = EmployeeId, ContractNo = ContractNo,
            ContractType = ContractType, StartDate = StartDate,
            EndDate = EndDate, SignDate = SignDate,
            Status = 0, FilePath = filePath, FileName = fileName,
            Remark = Remark, CreatedBy = User.GetRealName(),
        };
        await _uow.Contracts.AddAsync(contract);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("合同已保存"));
    }

    [HttpPost("upload/{id}")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> Upload(long id, IFormFile file)
    {
        var contract = await _uow.Contracts.GetByIdAsync(id);
        if (contract == null) return Json(ApiResult<object>.Fail("合同不存在"));
        var result = await SaveUploadFile(file, "hr/contracts");
        if (result == null) return Json(ApiResult<object>.Fail("文件上传失败"));
        contract.FilePath = result.Value.path;
        contract.FileName = result.Value.name;
        contract.UpdatedBy = User.GetRealName();
        _uow.Contracts.Update(contract);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(new { filePath = result.Value.path, fileName = result.Value.name }, "附件上传成功"));
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(long id)
    {
        var c = await _uow.Contracts.GetByIdAsync(id);
        if (c?.FilePath == null || !global::System.IO.File.Exists(c.FilePath)) return NotFound();
        var bytes = await global::System.IO.File.ReadAllBytesAsync(c.FilePath);
        return File(bytes, "application/octet-stream", c.FileName ?? "合同附件");
    }

    [HttpPost("delete/{id}")]
    [HasPermission("hr:contract:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var ct = await _uow.Contracts.GetByIdAsync(id);
        if (ct == null) return Json(ApiResult<object>.Fail("合同不存在"));
        if (ct.FilePath != null && global::System.IO.File.Exists(ct.FilePath))
            global::System.IO.File.Delete(ct.FilePath);
        _uow.Contracts.SoftDelete(ct);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("合同已删除"));
    }

    /// <summary>删除合同附件（保留合同记录，仅清除附件）</summary>
    [HttpPost("file/delete/{id}")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> DeleteFile(long id)
    {
        var ct = await _uow.Contracts.GetByIdAsync(id);
        if (ct == null) return Json(ApiResult<object>.Fail("合同不存在"));
        if (ct.FilePath != null && global::System.IO.File.Exists(ct.FilePath))
            global::System.IO.File.Delete(ct.FilePath);
        ct.FilePath  = null;
        ct.FileName  = null;
        ct.UpdatedBy = User.GetRealName();
        _uow.Contracts.Update(ct);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("附件已删除"));
    }

    [HttpPost("terminate")]
    [HasPermission("hr:contract:edit")]
    public async Task<IActionResult> Terminate(long id)
    {
        var c = await _uow.Contracts.GetByIdAsync(id);
        if (c == null) return Json(ApiResult<object>.Fail("合同不存在"));
        c.Status = 1; c.UpdatedBy = User.GetRealName();
        _uow.Contracts.Update(c);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("合同已终止"));
    }

    private async Task<(string path, string name)?> SaveUploadFile(IFormFile? file, string folder)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 20 * 1024 * 1024) return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);
        var ext  = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);
        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);
        return (path, file.FileName);
    }
}

// ── 证书管理 ──────────────────────────────────────────────────
[Authorize, Route("hr/cert")]
public class CertificateController : BaseAuthController
{
    private readonly IUnitOfWork           _uow;
    private readonly IDictService          _dictSvc;
    private readonly IOperLogService       _logSvc;
    private readonly IEmployeeQueryService _empQrySvc;

    public CertificateController(IUnitOfWork uow, IDictService dictSvc,
        IOperLogService logSvc, IEmployeeQueryService empQrySvc,
        IPermissionService permSvc)
        : base(permSvc)

    { _uow = uow; _dictSvc = dictSvc; _logSvc = logSvc; _empQrySvc = empQrySvc; }
    [HasPermission("hr:cert:list")]
    public async Task<IActionResult> Index(string? keyword, int? status, int page = 1, int size = 15)
    {
        var q = _uow.Certificates.Query().Include(c => c.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(c => (c.Employee != null && c.Employee.RealName.Contains(keyword)) ||
                             c.CertName.Contains(keyword));
        if (status.HasValue) q = q.Where(c => c.Status == status);
        var warnDate = DateTime.Today.AddDays(60);
        ViewBag.WarnCount = await q.CountAsync(c => c.Status == 0 &&
                                c.ExpireDate.HasValue && c.ExpireDate <= warnDate);
        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(c => c.CreatedAt)
                           .Skip((page-1)*size).Take(size).ToListAsync();
        ViewBag.CertTypes = await _dictSvc.GetDataByTypeAsync("cert_type");
        ViewBag.Employees = await _empQrySvc.GetAllOnJobAsync();
        ViewBag.Keyword = keyword; ViewBag.Status = status;
        ViewBag.Page = page; ViewBag.Total = total; ViewBag.Size = size;
        return View(list);
    }

    [HttpPost("create-with-file")]
    [HasPermission("hr:cert:add")]
    public async Task<IActionResult> CreateWithFile(
        [FromForm] long EmployeeId, [FromForm] string CertName,
        [FromForm] string CertType,  [FromForm] string? CertNo,
        [FromForm] string? IssueOrg, [FromForm] DateTime? IssueDate,
        [FromForm] DateTime? ExpireDate, IFormFile? file)
    {
        if (EmployeeId == 0 || string.IsNullOrWhiteSpace(CertName))
            return Json(ApiResult<object>.Fail("请填写员工和证书名称"));
        string? filePath = null; string? fileName = null;
        if (file != null && file.Length > 0)
        {
            var saved = await SaveUploadFile(file, "hr/certs");
            if (saved.HasValue) { filePath = saved.Value.path; fileName = saved.Value.name; }
        }
        var cert = new EmployeeCertificate
        {
            EmployeeId = EmployeeId, CertName = CertName, CertType = CertType,
            CertNo = CertNo, IssueOrg = IssueOrg, IssueDate = IssueDate,
            ExpireDate = ExpireDate, Status = 0,
            FilePath = filePath, FileName = fileName,
            CreatedBy = User.GetRealName(),
        };
        await _uow.Certificates.AddAsync(cert);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("证书已保存"));
    }


    [HttpPost("upload/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> Upload(long id, IFormFile file)
    {
        var cert = await _uow.Certificates.GetByIdAsync(id);
        if (cert == null) return Json(ApiResult<object>.Fail("证书不存在"));
        var dir  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "hr/certs");
        Directory.CreateDirectory(dir);
        var ext  = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);
        using (var fs = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(fs);
        cert.FilePath = path;
        cert.FileName = file.FileName;
        cert.UpdatedBy = User.GetRealName();
        _uow.Certificates.Update(cert);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok(new { filePath = path, fileName = file.FileName }, "证书附件上传成功"));
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(long id)
    {
        var c = await _uow.Certificates.GetByIdAsync(id);
        if (c?.FilePath == null || !global::System.IO.File.Exists(c.FilePath)) return NotFound();
        var bytes = await global::System.IO.File.ReadAllBytesAsync(c.FilePath);
        return File(bytes, "application/octet-stream", c.FileName ?? "证书附件");
    }

    [HttpPost("file/delete/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> DeleteFile(long id)
    {
        var cert = await _uow.Certificates.GetByIdAsync(id);
        if (cert == null) return Json(ApiResult<object>.Fail("证书不存在"));
        if (cert.FilePath != null && global::System.IO.File.Exists(cert.FilePath))
            global::System.IO.File.Delete(cert.FilePath);
        cert.FilePath = null; cert.FileName = null; cert.UpdatedBy = User.GetRealName();
        _uow.Certificates.Update(cert);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("附件已删除"));
    }

    [HttpPost("delete/{id}")]
    [HasPermission("hr:cert:edit")]
    public async Task<IActionResult> Delete(long id)
    {
        var cert = await _uow.Certificates.GetByIdAsync(id);
        if (cert == null) return Json(ApiResult<object>.Fail("证书不存在"));
        if (cert.FilePath != null && global::System.IO.File.Exists(cert.FilePath))
            global::System.IO.File.Delete(cert.FilePath);
        _uow.Certificates.SoftDelete(cert);
        await _uow.SaveChangesAsync();
        return Json(ApiResult<object>.Ok("证书已删除"));
    }

    private async Task<(string path, string name)?> SaveUploadFile(IFormFile? file, string folder)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 20 * 1024 * 1024) return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);
        var ext  = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid():N}{ext}";
        var fpath = Path.Combine(dir, name);
        using var fs = new FileStream(fpath, FileMode.Create);
        await file.CopyToAsync(fs);
        return (fpath, file.FileName);
    }
}

// ── Request 模型 ──────────────────────────────────────────────
public class CreateEmployeeRequest
{
    public string    RealName { get; set; } = "";
    public int       Gender   { get; set; } = 1;
    public string?   Phone    { get; set; }
    public string?   Email    { get; set; }
    public string?   IdCard   { get; set; }
    public long?     DeptId   { get; set; }
    public long?     PostId   { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public string?   Remark   { get; set; }
}
public class UpdateEmployeeRequest : CreateEmployeeRequest { public long Id { get; set; } }

public class CreateContractRequest
{
    public long      EmployeeId   { get; set; }
    public string    ContractNo   { get; set; } = "";
    public string    ContractType { get; set; } = "";
    public DateTime  StartDate    { get; set; }
    public DateTime  EndDate      { get; set; }
    public DateTime? SignDate     { get; set; }
    public string?   Remark       { get; set; }
}

public class CreateCertRequest
{
    public long      EmployeeId { get; set; }
    public string    CertName   { get; set; } = "";
    public string    CertType   { get; set; } = "";
    public string?   CertNo     { get; set; }
    public string?   IssueOrg   { get; set; }
    public DateTime? IssueDate  { get; set; }
    public DateTime? ExpireDate { get; set; }
}
