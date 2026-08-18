using System;
using System.Collections.Generic;
using System.IO;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using MiniExcelLibs;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.DTOs.System;
using EnterpriseMS.Services.DTOs.User;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers.System;

[Authorize, Route("system/user")]
public class UserController : BaseAuthController
{
    private readonly IUserService    _userSvc;
    private readonly IRoleService    _roleSvc;
    private readonly IDeptService    _deptSvc;
    private readonly IOperLogService _logSvc;
    private readonly IUnitOfWork     _uow;
    private readonly IEmployeeService _empSvc;

    public UserController(IUserService userSvc, IRoleService roleSvc,
        IDeptService deptSvc, IOperLogService logSvc,
        IUnitOfWork uow, IPermissionService permSvc, IEmployeeService empSvc)
        : base(permSvc)
    {
        _userSvc = userSvc;
        _roleSvc = roleSvc;
        _deptSvc = deptSvc;
        _logSvc = logSvc;
        _uow = uow;
        _empSvc = empSvc;
    }

    /// <summary>列表页外壳：数据由 layui table 异步向 list 接口拉取</summary>
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        return View();
    }

    /// <summary>列表数据（layui table 数据源）</summary>
    [HttpGet("list")]
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> List([FromQuery] UserQueryDto query)
        => ApiOk(await _userSvc.GetPagedAsync(query));

    /// <summary>新增 / 编辑表单页（在 layer iframe 弹层中打开）</summary>
    [HttpGet("form")]
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> Form(long? id)
    {
        ViewBag.Depts = await _deptSvc.GetTreeAsync();
        ViewBag.Posts = await _uow.Posts.GetListAsync();
        ViewBag.Roles = await _roleSvc.GetAllActiveAsync();

        // 员工下拉预渲染（原前端 ems.loadSelect AJAX → 改为服务端同步渲染，避免打开时异步撑高弹窗）
        var emps = await _empSvc.GetOnJobAsync();
        var boundIds = await _empSvc.GetBoundEmployeeIdsAsync();
        ViewBag.Employees = emps.Select(e => new
        {
            e.Id,
            e.RealName,
            DeptName = e.Dept?.DeptName,
            Display = e.Dept != null ? $"{e.RealName}（{e.Dept.DeptName}）" : e.RealName,
            IsBound = boundIds.Contains(e.Id)
        }).ToList();

        UserDetailDto? model = id.HasValue ? await _userSvc.GetDetailAsync(id.Value) : null;
        // 编辑且已绑定员工：预渲染员工档案，初始化时直接填充，避免打开时 AJAX 拉详情撑高
        if (model?.EmployeeId is long empId)
        {
            var emp = await _empSvc.GetDetailAsync(empId);
            if (emp != null)
                ViewBag.EmployeeDetail = new { emp.RealName, emp.Phone, emp.Email, DeptId = emp.DeptId };
        }
        return View(model);
    }

    [HttpGet("detail/{id}")]
    [HasPermission("sys:user:list")]
    public async Task<IActionResult> Detail(long id)
    {
        var user = await _userSvc.GetDetailAsync(id);
        if (user == null) return NotFound();
        return ApiOk(user);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:add")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        try
        {
            var id = await _userSvc.CreateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("新增用户", $"用户名：{dto.Username}", "INSERT", id);
            return ApiOk("用户创建成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:edit")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return ApiFail(GetErrors());
        try
        {
            await _userSvc.UpdateAsync(dto, User.GetRealName());
            await _logSvc.LogAsync("修改用户", $"用户ID：{dto.Id}", "UPDATE", dto.Id);
            return ApiOk("修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("delete/{id}")]
    [HasPermission("sys:user:delete")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _userSvc.DeleteAsync(id, User.GetRealName());
            await _logSvc.LogAsync("删除用户", $"用户ID：{id}", "DELETE", id);
            return ApiOk("删除成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("status")]
    [HasPermission("sys:user:edit")]
    public async Task<IActionResult> SetStatus(long id, int status)
    {
        try
        {
            await _userSvc.SetStatusAsync(id, status, User.GetRealName());
            return ApiOk(status == 1 ? "已启用" : "已禁用");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("resetpwd")]
    [HasPermission("sys:user:reset")]
    public async Task<IActionResult> ResetPwd(long id, string newPwd)
    {
        try
        {
            await _userSvc.ResetPasswordAsync(id, newPwd, User.GetRealName());
            await _logSvc.LogAsync("重置密码", $"用户ID：{id}", "UPDATE", id);
            return ApiOk("密码已重置");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpPost("changepwd"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePwd([FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetUserId();
        try
        {
            await _userSvc.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            return ApiOk("密码修改成功");
        }
        catch (Exception ex) when (ex is BusinessException or NotFoundException)
        {
            return ApiFail(ex.Message);
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleSvc.GetAllActiveAsync();
        return ApiOk(roles);
    }

    /// <summary>下载用户批量导入模板（xlsx）</summary>
    [HttpGet("template")]
    [HasPermission("sys:user:add")]
    public IActionResult DownloadTemplate()
    {
        var templateRows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["用户名*"]  = "zhangsan",
                ["姓名*"]    = "张三",
                ["手机号"]   = "13800138000",
                ["邮箱"]     = "zhangsan@example.com",
                ["所属部门"] = "第一事业部",
                ["岗位"]     = "工程师",
                ["角色"]     = "普通员工",
                ["状态"]     = "正常",
            },
            new()
            {
                ["用户名*"]  = "lisi",
                ["姓名*"]    = "李四",
                ["手机号"]   = "13900139000",
                ["邮箱"]     = "lisi@example.com",
                ["所属部门"] = "第二事业部",
                ["岗位"]     = "项目经理",
                ["角色"]     = "部门主管",
                ["状态"]     = "正常",
            },
        };

        var ms = new MemoryStream();
        ms.SaveAs(templateRows);
        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "用户批量导入模板.xlsx");
    }

    /// <summary>
    /// 批量导入用户：解析 xlsx/xls/csv，按用户名去重；
    /// 已存在则覆盖关键信息（姓名/手机/邮箱/部门/岗位/状态/角色），不存在则新建（默认密码 123456）。
    /// </summary>
    [HttpPost("import"), ValidateAntiForgeryToken]
    [HasPermission("sys:user:add")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiFail("请选择要导入的文件");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var excelType = ext switch
        {
            ".csv" => ExcelType.CSV,
            _      => ExcelType.XLSX,
        };
        if (ext is not (".xlsx" or ".xls" or ".csv"))
            return ApiFail("仅支持 Excel(.xlsx/.xls) 或 CSV 文件");

        // 预加载 部门/岗位/角色 名称→Id 映射字典
        var deptMap = await BuildDeptMapAsync();
        var postMap = (await _uow.Posts.GetListAsync())
            .Where(p => !string.IsNullOrWhiteSpace(p.PostName))
            .ToDictionary(p => p.PostName.Trim(), p => p.Id, StringComparer.OrdinalIgnoreCase);
        var roles = await _roleSvc.GetAllActiveAsync();
        var roleMap = roles.ToDictionary(r => r.RoleName.Trim(), r => r.Id, StringComparer.OrdinalIgnoreCase);

        int created = 0, updated = 0, failed = 0;
        var errors = new List<string>();
        var operBy = User.GetRealName();

        await using var stream = file.OpenReadStream();
        var rows = MiniExcel.Query(stream, useHeaderRow: true, excelType: excelType);
        int rowNo = 1;

        foreach (var row in rows)
        {
            rowNo++;
            var dict = (IDictionary<string, object>)row;
            try
            {
                var username = GetCell(dict, "用户名", "用户名*")?.ToString()?.Trim();
                var realName = GetCell(dict, "姓名", "姓名*")?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(realName))
                    continue;                                   // 跳过完全空白的行
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(realName))
                {
                    errors.Add($"第{rowNo}行：用户名和姓名不能为空");
                    failed++; continue;
                }

                var phone    = GetCell(dict, "手机号")?.ToString()?.Trim();
                var email    = GetCell(dict, "邮箱")?.ToString()?.Trim();
                var deptName = GetCell(dict, "所属部门")?.ToString()?.Trim();
                var postName = GetCell(dict, "岗位")?.ToString()?.Trim();
                var roleCell = GetCell(dict, "角色")?.ToString()?.Trim();
                var status   = ParseStatus(GetCell(dict, "状态")?.ToString());

                var deptId = Resolve(deptMap, deptName);
                var postId = Resolve(postMap, postName);
                var roleIds = new List<long>();
                if (!string.IsNullOrWhiteSpace(roleCell))
                    foreach (var rn in roleCell.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var rid = Resolve(roleMap, rn.Trim());
                        if (rid.HasValue) roleIds.Add(rid.Value);
                    }

                var exist = await _uow.Users.Query().FirstOrDefaultAsync(u => u.Username == username);
                if (exist != null)
                {
                    exist.RealName  = realName!;
                    exist.Phone     = phone;
                    exist.Email     = email;
                    exist.DeptId    = deptId;
                    exist.PostId    = postId;
                    exist.Status    = status;
                    exist.UpdatedBy = operBy;
                    _uow.Users.Update(exist);
                    await _uow.SaveChangesAsync();
                    if (roleIds.Any()) await _userSvc.AssignRolesAsync(exist.Id, roleIds);
                    updated++;
                }
                else
                {
                    var user = new SysUser
                    {
                        Username     = username!,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12),
                        RealName     = realName!,
                        Phone        = phone,
                        Email        = email,
                        DeptId       = deptId,
                        PostId       = postId,
                        Status       = status,
                        CreatedBy    = operBy,
                    };
                    await _uow.Users.AddAsync(user);
                    await _uow.SaveChangesAsync();
                    if (roleIds.Any()) await _userSvc.AssignRolesAsync(user.Id, roleIds);
                    created++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"第{rowNo}行处理失败：{ex.Message}");
                failed++;
            }
        }

        var msg = $"导入完成：新增 {created} 条，更新 {updated} 条，失败 {failed} 条";
        await _logSvc.LogAsync("批量导入用户", msg, "IMPORT", 0);
        return ApiOk(new { created, updated, failed, errors }, msg);
    }

    private static object? GetCell(IDictionary<string, object> row, params string[] names)
    {
        foreach (var n in names)
            if (row.TryGetValue(n, out var v) && v != null) return v;
        foreach (var kv in row)
        {
            var key = kv.Key.Trim().TrimEnd('*');
            if (names.Any(n => n.Trim().TrimEnd('*').Equals(key, StringComparison.OrdinalIgnoreCase)))
                return kv.Value;
        }
        return null;
    }

    private static long? Resolve(Dictionary<string, long> map, string? name)
        => string.IsNullOrWhiteSpace(name) ? null
         : map.TryGetValue(name.Trim(), out var id) ? id : null;

    private static int ParseStatus(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 1;
        var t = s.Trim();
        if (t is "1" or "正常" or "启用" or "active") return 1;
        if (t is "0" or "禁用" or "停用" or "inactive") return 0;
        return 1;
    }

    private async Task<Dictionary<string, long>> BuildDeptMapAsync()
    {
        var tree = await _deptSvc.GetTreeAsync();
        var map = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        void Collect(DeptTreeDto d)
        {
            if (!string.IsNullOrWhiteSpace(d.DeptName) && !map.ContainsKey(d.DeptName))
                map[d.DeptName] = d.Id;
            foreach (var c in d.Children) Collect(c);
        }
        foreach (var d in tree) Collect(d);
        return map;
    }

}
