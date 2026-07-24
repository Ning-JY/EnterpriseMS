# EnterpriseMS 代码审计报告（深度复核版）

**审计时间**: 2026-07-24  
**审计范围**: `C:\Users\ningj\source\repos\EnterpriseMS`  
**代码规模**: 122 个 .cs 文件、44 个 .cshtml、约 25,164 行源码  
**技术栈**: .NET 9 / ASP.NET Core MVC / EF Core (Pomelo/MySQL) / Hangfire / AutoMapper / FluentValidation / Redis

---

## 一、审计结论总览

| 类别 | 严重程度 | 数量 | 代表问题 |
|------|---------|------|---------|
| 分层逻辑混乱 | 🔴 高 | 5 | IUnitOfWork 上帝对象、Controller 越层持久化、两套 DI 套路并存 |
| 代码冗余 / 重复方法 | 🟡 中高 | 6 | BuildTree 复制、GetProgressText 复制、分页三套实现、DeleteFileAsync 三份 |
| 代码风格不统一 | 🟡 中 | 5 | GetPagedAsync 返回形状不一致、DateTime.Now/Today 混用、无 .editorconfig |
| 死代码 / 风险 | 🟡 中 | 3 | 事务方法空定义、CreatedAt 手设冗余、时钟未用 UTC |

---

## 二、分层逻辑混乱（高优先级）

### 2.1 🔴 IUnitOfWork 是"上帝对象"——服务定位器反模式

**位置**: `Repository.cs:55-128` / `IUnitOfWork.cs:12-46`

```csharp
// IUnitOfWork.cs — 硬编码了 37 个强类型仓储属性
public interface IUnitOfWork {
    IRepository<SysUser>     Users       { get; }
    IRepository<SysRole>     Roles       { get; }
    IRepository<Employee>    Employees   { get; }
    IRepository<Project>     Projects    { get; }
    // ... 共 37 个
}
```

几乎所有 Service 构造函数都是:

```csharp
public XxxService(IUnitOfWork uow, ...) { _uow = uow; }
// 调用时: _uow.Users.GetByIdAsync(id)
```

**危害**:
- 新增任何实体要改 IUnitOfWork 接口 + UnitOfWork 实现 + 字段缓存三处
- 接口膨胀,违反接口隔离原则
- 调用方被迫依赖全部仓储,而非所需的那个

**整改方向**: 废除 IUnitOfWork 上的 37 个属性,让每个 Service 直接注入所需的 `IRepository<T>`:

```csharp
// 改造前
public DeptService(IUnitOfWork uow) { _uow = uow; }
await _uow.Depts.GetByIdAsync(id);

// 改造后（参考 BidService 已有套路）
public DeptService(IRepository<SysDept> depts) { _depts = depts; }
await _depts.GetByIdAsync(id);
```

IUnitOfWork 仅保留事务相关方法(`SaveChangesAsync`/`BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`)。

---

### 2.2 🔴 两套数据访问套路并存

**问题 A**: `BidService` 直接注入 `IRepository<T>`（BidService.cs:19-38）

```csharp
public BidService(
    IRepository<BidProject> bidProjectRepo,
    IRepository<BidRequirement> requirementRepo,
    IRepository<BidDocument> documentRepo,
    ...
)
```

而其他所有 Service 用的是 `IUnitOfWork uow; _uow.Users.GetByIdAsync()` 的服务定位器式。

**问题 B**: `BidService` 跨域直连 Employee/Certificate 表,绕开了 `IEmployeeService`/`ICertificateService`

```csharp
// BidService.cs:23-24
private readonly IRepository<Employee> _employeeRepo;
private readonly IRepository<EmployeeCertificate> _certRepo;
```

---

### 2.3 🔴 ConfigService / OperLogService 绕过仓储直接注入 AppDbContext

**位置**: `ConfigService.cs:13-14,41` / `OperLogService.cs:12,36`

```csharp
// ConfigService.cs
public class ConfigService : IConfigService {
    private readonly AppDbContext _db;          // ← 未走仓储
    public async Task SaveAsync(List<SysConfigDto> configs) {
        await _db.SysConfigs.AddAsync(...);
        await _db.SaveChangesAsync();          // ← 未走 IUnitOfWork
    }
}
```

与"全项目走 IUnitOfWork"的约定不一致,绕过了软删除/审计字段等 DbContext 统一拦截逻辑。

---

### 2.4 🔴 Controller 越层直接持久化与查询

| 文件 | 行号 | 问题 |
|------|------|------|
| `KbController.cs` | 117,134,165,178 | `_uow.SaveChangesAsync()` 直接在 Controller 调用 |
| `ProjectImportController.cs` | 237 | `_db.SaveChangesAsync()` |
| `DebugController.cs` | 174,208,229,300 | `_db.SaveChangesAsync()` |
| `ProjectController.cs` | 33 | 同时注入 `AppDbContext db` + `IUnitOfWork uow`,在 Controller 里内联 `Skip/Take` 分页(:77 等) |
| `ReportController.cs` | 18,20,73 等 | 直接注入 `AppDbContext`,在 Controller 写 EF 查询 |

**整改**: 将所有持久化逻辑下沉到 Service;Controller 仅做参数校验 + 路由分发。

---

### 2.5 🟡 UnitOfWork 内 `new Repository<>` 而非走 DI

**位置**: `Repository.cs:97-127`

```csharp
public IRepository<SysUser> Users => _users ??= new Repository<SysUser>(_db);
```

Program.cs 已注册:
```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

但 UoW 手动 `new`,与 DI 体系割裂——无法享受 DI 的拦截/装饰器等扩展能力。

---

## 三、代码冗余 / 重复方法

### 3.1 🟡 `BuildTree` 递归建树在两个服务原样复制

**位置**: `DeptService.cs:19-22` 与 `MenuService.cs:18-21`（一字不差）

```csharp
private List<TDto> BuildTree(List<TDto> all, long parentId)
    => all.Where(d => d.ParentId == parentId)
          .Select(d => { d.Children = BuildTree(all, d.Id); return d; }).ToList();
```

**整改**: 抽成泛型扩展方法:

```csharp
public static List<T> BuildTree<T>(this List<T> all, long parentId,
    Func<T, long> getId, Func<T, long> getParentId,
    Action<T, List<T>> setChildren) {
    return all.Where(e => getParentId(e) == parentId)
        .Select(e => { setChildren(e, all.BuildTree(parentId, ...)); return e; }).ToList();
}
```

---

### 3.2 🟡 `GetProgressText(int)` 在两个文件重复实现（11 分支逐字一致）

**位置**: `ProjectService.cs:860` 与 `ReportController.cs:297`

```csharp
public static string GetProgressText(int status) => status switch {
    0 => "前期商机", 1 => "预计启动", 2 => "标书制作中",
    3 => "投标/竞价中", 4 => "已中标签约中", 5 => "已签合同",
    6 => "执行中", 7 => "成果提交", 8 => "已完工", 9 => "已终止",
    _ => "未知",
};
```

**整改**: 移入 `Domain/Enums/ProjectStatus.cs` 或 `Common/CommonClasses.cs`,所有引用统一调用。

---

### 3.3 🟡 `DeleteFileAsync` 物理删除文件+置空字段样板重复三份

**位置**: `CertificateService.cs:115` / `ContractService.cs:114` / `ProjectService.cs:623`

Certificate 与 Contract 两份几乎完全一致（仅实体类型不同）:

```csharp
// CertificateService.cs:115
public async Task DeleteFileAsync(long id, string operBy) {
    var cert = await _uow.Certificates.GetByIdAsync(id);
    if (cert == null) throw new NotFoundException("证书不存在");
    if (cert.FilePath != null && System.IO.File.Exists(cert.FilePath))
        System.IO.File.Delete(cert.FilePath);
    cert.FilePath = null; cert.FileName = null; cert.UpdatedBy = operBy;
    _uow.Certificates.Update(cert);
    await _uow.SaveChangesAsync();
}
```

**整改**: 抽 `IFileManageable<T>` 接口,泛型实现:

```csharp
public interface IFileManageable { string? FilePath { get; set; } string? FileName { get; set; } }
public static async Task DeleteFileAsync<T>(IRepository<T> repo, long id, string operBy)
    where T : class, IFileManageable, BaseEntity {
    var entity = await repo.GetByIdAsync(id);
    if (entity == null) throw new NotFoundException("记录不存在");
    if (entity.FilePath != null && File.Exists(entity.FilePath)) File.Delete(entity.FilePath);
    entity.FilePath = null; entity.FileName = null; entity.UpdatedBy = operBy;
    repo.Update(entity);
}
```

---

### 3.4 🟡 分页三套实现并存（最该统一）

| 实现 | 位置 | 产出 |
|------|------|------|
| `Repository<T>.GetPagedAsync` | `Repository.cs:28-46` | `PagedResult<T>` |
| `PagingExtensions.ToPagedAsync` | `PagingExtensions.cs:13-19` | `PagedResult<T>` |
| 内联 `CountAsync+Skip+Take` | ProjectService(8处)、ReportController(3处)等 | `PagedResult<T>` |

三者逻辑完全相同,却分散三处。

**整改**: 删除 `Repository<T>.GetPagedAsync`（或改为调用 `ToPagedAsync`）,统一走 `IQueryable<T>.ToPagedAsync(page, size)` 扩展方法。

---

### 3.5 🟡 `GetEmployeesAsync` 透传重复

**位置**: `CertificateService.cs:43` / `ContractService.cs:42`

```csharp
public Task<List<EmployeeSimpleDto>> GetEmployeesAsync()
    => _empQrySvc.GetAllOnJobAsync();
```

只是透传,无任何自身逻辑,接口层 `ICertificateService` / `IContractService` 也定义了这份透传方法——职责不清,Employee 的列表本就该归 `IEmployeeService`。

---

### 3.6 🟡 `GetPagedAsync` 返回形状不一致

| Service | 返回类型 | 位置 |
|---------|---------|------|
| Certificate | `Task<(List<EmployeeCertificate>, int Total, int WarnCount)>` | CertificateService.cs:27 |
| Contract | `Task<(List<EmployeeContract>, int Total, int WarnCount)>` | ContractService.cs:27 |
| 其他 9 个 | `Task<PagedResult<T>>` | User/Role/Kb/Project/Bid 等 |

调用方无法用统一契约,前端接口响应结构也不统一。

---

## 四、代码风格不统一

### 4.1 🟡 无 `.editorconfig`,风格漂移无约束

根目录不存在 `.editorconfig`,导致:
- `var` vs 显式类型混用
- 空判断有的用 `!` 有的显式 `== null`
- 文件头注释有无不一致
- 命名大小写不规范

---

### 4.2 🟡 `DateTime.Now` / `DateTime.Today` / `DateTimeOffset` 混用

| 出现位置 | 用法 |
|---------|------|
| `AppDbContext.cs:176,179` | `DateTime.Now`（审计字段自动填充） |
| `OperLogService.cs:32` | `DateTime.Now`（OperTime） |
| `ProjectService.cs:149,152,766` | `DateTime.Now` |
| `BidService.cs:276,502,561` | `DateTime.Now` |
| `EmployeeService.cs:79` | `DateTime.Now.Year` |
| `HangfireService.cs:24,33,41` | `DateTime.Today` |
| `NotificationService.cs:75` | `DateTime.Today` |
| `ContractService.cs:36` | `DateTime.Today.AddDays(30)` |
| `CertificateService.cs:36` | `DateTime.Today.AddDays(60)` |

全部用本地时间而非 UTC,多实例部署或跨时区场景会有时间错位。

**建议**: 统一策略,全局用 `DateTime.UtcNow`;前端展示层统一转本地时区。

---

### 4.3 🟡 `CreatedAt = DateTime.Now` 手设冗余

多处手设 `CreatedAt`,但 `AppDbContext.SaveChangesAsync()` 重写已自动填充:

```csharp
// AppDbContext.cs:175-179
if (entry.Entity.CreatedAt == default)
    entry.Entity.CreatedAt = DateTime.Now;
```

手设反而导致两次赋值的混乱,且格式不统一(有的是 `DateTime.Now`,有的是 `DateTime.Now.ToString(...)`)。

**出现位置**: `DictService.cs:43,84`、`EducationService.cs:45`、`WorkExpService.cs:41`、`ProjectService.cs:152,766` 等。

---

### 4.4 🟡 魔法字符串——字典类型无常量管理

```csharp
// CertificateService.cs:44
=> _dictSvc.GetDataByTypeAsync("cert_type");

// ContractService.cs:43
=> _dictSvc.GetDataByTypeAsync("contract_type");
```

`"cert_type"`、`"contract_type"` 应定义为 `public const string` 枚举或 `DictType` 枚举类。

---

## 五、死代码与风险项

### 5.1 🔴 事务方法定义后从未调用（一致性风险）

**位置**: `IUnitOfWork.cs:53-55` / `Repository.cs:131-136`

```csharp
// IUnitOfWork.cs
Task BeginTransactionAsync();
Task CommitAsync();
Task RollbackAsync();

// Repository.cs
public async Task BeginTransactionAsync() => _tx = await _db.Database.BeginTransactionAsync();
public async Task CommitAsync() { await _db.SaveChangesAsync(); if (_tx != null) await _tx.CommitAsync(); }
public async Task RollbackAsync() { if (_tx != null) await _tx.RollbackAsync(); }
```

**grep 结果**: 全项目没有任何 Service 或 Controller 调用过这三个方法。

多步写入(创建实体+创建关联记录+写日志)没有任何原子性保证,一旦中间步骤失败,数据会停留在不一致状态。

**整改**: 要么实际使用事务(批量操作加 `await _uow.BeginTransactionAsync()` 包裹),要么删除这三个死方法。

---

### 5.2 🟡 `AppDbContext.SeedAsync()` 空实现

**位置**: `AppDbContext.cs:230-235`

```csharp
public async Task SeedAsync() {
    await Task.CompletedTask; // 注释承认:HasData 由 MigrateAsync() 自动写入
}
```

注释说"保留方法签名以兼容 Program.cs 调用",但逻辑上是死代码——真正的种子逻辑已在 `SeedData(mb)` 中通过 `HasData()` 实现。

---

## 六、亮点（值得保持）

以下实践做得正确,重构时不要破坏:

| 实践 | 位置 | 说明 |
|------|------|------|
| 文件上传集中管理 | `FileUploadHelper.cs` | 非 Web 根存储、扩展名白名单、杜绝存储型 XSS |
| 文件下载集中管理 | `FileServingHelper.cs` | MIME 推导、中文文件名 RFC 5987 编码、流式返回避免大文件撑内存 |
| 全局软删除过滤器 | `AppDbContext.cs:189-197` | EF Core 全局 QueryFilter,所有查询自动过滤 IsDeleted |
| 审计字段自动填充 | `AppDbContext.cs:167-181` | SaveChangesAsync 重写,CreatedAt/UpdatedAt/Id 自动维护 |
| CSRF 全局强制 | `Program.cs:146` | antiforgery token 全局校验,防止跨站请求 |
| 三层 Filter 体系 | `Filters.cs` | GlobalExceptionFilter/OperationLogFilter/ValidationFilter |
| Hangfire 后台任务 | `HangfireService.cs` | 定时检查合同/证书/里程碑到期 |
| Redis 降级内存缓存 | `Program.cs:85-101` | Redis 不可用时自动降级,不影响功能 |
| EF Core 外键索引 | `AppDbContext.cs:141-156` | 频繁查询外键建索引,提升 JOIN 性能 |

---

## 七、整改优先级路线图

### P0（立即修复）
1. 废除 IUnitOfWork 的 37 个属性,统一改为 Service 直接注入 `IRepository<T>`
2. 统一分页:全部走 `ToPagedAsync` 扩展,删除 `Repository<T>.GetPagedAsync`
3. 收敛 Controller 越层:把所有 `SaveChangesAsync` 调用下沉到 Service

### P1（本月内）
4. 抽公共方法:`BuildTree<T>`、`GetProgressText` 移入共享类
5. 统一 `GetPagedAsync` 返回形状为 `PagedResult<T>`
6. 删除死代码:`BeginTransactionAsync/CommitAsync/RollbackAsync` 要么用要么删
7. 创建根级 `.editorconfig`

### P2（下季度）
8. 统一时间策略:全局用 `DateTime.UtcNow`
9. 消除手设 `CreatedAt`,统一走 DbContext 拦截
10. 字典类型常量化(`DictType` 枚举)
11. `DeleteFileAsync` 泛型抽象

---

*本报告由 AI 代码审计工具生成,所有结论基于源码静态分析,建议结合实际运行验证。*
