# EnterpriseMS 代码审计报告（合并版）

**审计日期**: 2026年7月24日  
**合并来源**: 深度复核 + 修复后复查 两版交叉核实  
**审计范围**: 代码冗余、重复方法、代码风格一致性、分层逻辑  
**代码规模**: 122 个 .cs 文件、44 个 .cshtml、约 25,164 行源码

---

## 一、已修复确认 ✅

| # | 问题 | 说明 |
|---|------|------|
| 1 | `GetErrors()` 多处重复定义 | ✅ 已提取到 `BaseAuthController` 基类 |
| 2 | `ViewBag.DictCertType` 与 `ViewBag.CertTypes` 重复加载 | ✅ 已移除重复项 |
| 3 | ProjectController 末尾冗余 `GetErrors()` 私有方法 | ✅ 已删除 |
| 4 | 文件下载逻辑散落多处 | ✅ 已统一为 `FileServingHelper.ServePhysicalFile()` |
| 5 | 整文件 `ReadAllBytes` 撑内存 | ✅ 已改为 `PhysicalFileResult` 流式返回 |

---

## 二、严重问题（🔴 高优先级）

### 2.1 🔴 IUnitOfWork 是"上帝对象"——服务定位器反模式

**位置**: `Infrastructure/Repositories/Repository.cs:55-128` / `Domain/Interfaces/IUnitOfWork.cs:12-46`

IUnitOfWork 硬编码了 **37 个强类型仓储属性**,几乎所有 Service 都通过它做服务定位器式调用:

```csharp
// IUnitOfWork.cs — 共 37 个属性
public interface IUnitOfWork {
    IRepository<SysUser>     Users       { get; }
    IRepository<SysRole>     Roles       { get; }
    IRepository<Employee>    Employees   { get; }
    IRepository<Project>     Projects    { get; }
    // ... 共 37 个
}

// Service 中调用方式: _uow.Users.GetByIdAsync(id)
```

**危害**:
- 新增任何实体要同时改 `IUnitOfWork` 接口 + `UnitOfWork` 实现 + 字段缓存三处
- 调用方被迫依赖全部仓储,违反接口隔离原则
- `BidService` 为例外:它直接注入 `IRepository<T>`,与其他 Service 风格不一致

**整改**: 让每个 Service 直接注入所需的 `IRepository<T>`(参考 `BidService` 已有套路),IUnitOfWork 仅保留事务相关方法。

---

### 2.2 🔴 事务方法定义后全项目从未调用——一致性风险

**位置**: `IUnitOfWork.cs:53-55` / `Repository.cs:131-136`

```csharp
// 定义了但从未被任何 Service/Controller 调用
Task BeginTransactionAsync();
Task CommitAsync();
Task RollbackAsync();
```

多步写入(创建实体+创建关联记录+写日志)没有任何原子性保证,中间步骤失败会导致数据不一致。

**整改**: 要么在需要事务的批量操作中实际使用(`await _uow.BeginTransactionAsync()` 包裹),要么删除这三个死方法。

---

### 2.3 🔴 Controller 越层直接持久化与查询

| 文件 | 行号 | 问题 |
|------|------|------|
| `Controllers/Kb/KbController.cs` | 117,134,165,178 | `_uow.SaveChangesAsync()` 在 Controller 直接调用 |
| `Controllers/Project/ProjectImportController.cs` | 237 | `_db.SaveChangesAsync()` |
| `Controllers/System/DebugController.cs` | 174,208,229,300 | `_db.SaveChangesAsync()` |
| `Controllers/Report/ReportController.cs` | 18,20,73等 | 直接注入 `AppDbContext`,在 Controller 写 EF 查询 |
| `Controllers/Account/AccountController.cs` | 17,55-58 | 直接注入 `AppDbContext`,登录时直查用户角色 |
| `Controllers/Project/ProjectController.cs` | 33,111-114 | 同时注入 `AppDbContext` + `IUnitOfWork`,Controller 内联分页查询 |

**整改**: 将所有持久化逻辑下沉到 Service,Controller 仅做参数校验 + 路由分发。ReportController 中的临时聚合查询如需保留,应在 Service 层封装为独立查询方法。

---

### 2.4 🟡 Service 层直接注入 AppDbContext,绕过仓储抽象

| 文件 | 行号 | 说明 |
|------|------|------|
| `Services/Impl/ConfigService.cs` | 13-14,41 | 直接 `_db.SaveChangesAsync()` |
| `Services/Impl/OperLogService.cs` | 12,36 | 直接 `_db.SaveChangesAsync()` |

与全项目"通过 IUnitOfWork 持久化"的约定不一致,且绕过了软删除/审计字段等 DbContext 统一拦截逻辑。

---

## 三、代码冗余 / 重复方法（🟡 中优先级）

### 3.1 🟡 `BuildTree` 递归建树原样复制

**位置**: `DeptService.cs:19-22` 与 `MenuService.cs:18-21`（一字不差）

```csharp
private List<TDto> BuildTree(List<TDto> all, long parentId)
    => all.Where(d => d.ParentId == parentId)
          .Select(d => { d.Children = BuildTree(all, d.Id); return d; }).ToList();
```

**整改**: 抽成泛型扩展方法 `List<T>.BuildTree(parentId, getId, getParentId, setChildren)`。

---

### 3.2 🟡 `GetProgressText(int)` 两个文件逐字重复

**位置**: `ProjectService.cs:860` 与 `ReportController.cs:297`

11 个 switch 分支完全一致,应移入 `Common/CommonClasses.cs` 或 `Domain/Enums/` 共享。

---

### 3.3 🟡 分页实现三套并存

| 实现 | 位置 | 产出 |
|------|------|------|
| `Repository<T>.GetPagedAsync` | `Repository.cs:28-46` | `PagedResult<T>` |
| `PagingExtensions.ToPagedAsync` | `Common/PagingExtensions.cs:13-19` | `PagedResult<T>` |
| 内联 `CountAsync + Skip + Take` | ProjectService(8处)、ReportController(3处)等 | `PagedResult<T>` |

三者逻辑完全相同,分散三处。**整改**: 统一走 `IQueryable<T>.ToPagedAsync(page, size)` 扩展方法。

---

### 3.4 🟡 `DeleteFileAsync` 物理删除文件样板重复三份

**位置**: `CertificateService.cs:115` / `ContractService.cs:114` / `ProjectService.cs:623`

Certificate 与 Contract 两份几乎完全一致(仅类型名不同),应抽泛型接口 `IFileManageable<T>`。

---

### 3.5 🟡 `GetPagedAsync` 返回形状不一致

| Service | 返回类型 |
|---------|---------|
| Certificate / Contract | `Task<(List<T> Items, int Total, int WarnCount)>` |
| 其他 9 个 Service | `Task<PagedResult<T>>` |

调用方无法用统一契约,前端接口响应结构也不统一。

---

### 3.6 🟢 EducationController 与 WorkExpController 结构相似

两者 CRUD 模板代码高度相似,但实际字段不同。建议引入 `CrudControllerBase<TEntity, TDto>` 抽象基类,将通用的分页/列表/删除逻辑提取复用。

---

## 四、代码风格不统一（🟡 中优先级）

### 4.1 🟡 无 `.editorconfig`,风格漂移无约束

根目录不存在 `.editorconfig`,导致:
- `var` vs 显式类型混用
- 空判断有的用 `!` 有的显式 `== null`
- 文件头注释有无不一致

---

### 4.2 🟡 `DateTime.Now` / `DateTime.Today` / UTC 混用,全为本地时间

| 用法 | 出现次数 | 位置 |
|------|---------|------|
| `DateTime.Now` | ~25 | AppDbContext, OperLogService, ProjectService, BidService 等 |
| `DateTime.Today` | ~8 | HangfireService, ContractService, CertificateService 等 |
| `DateTime.UtcNow` | 0 | 整个项目未使用 |

全部本地时间,多实例/跨时区部署会有时间错位。

---

### 4.3 🟢 `CreatedAt = DateTime.Now` 多处手设冗余

`AppDbContext.SaveChangesAsync()` 重写已自动填充 `CreatedAt`/`UpdatedAt`:

```csharp
// AppDbContext.cs:175-179
if (entry.Entity.CreatedAt == default)
    entry.Entity.CreatedAt = DateTime.Now;
```

手设反而导致双重赋值,且格式不统一(有的是 `DateTime.Now`,有的是 `DateTime.Now.ToString(...)`)。

出现位置:`DictService.cs:43,84`、`EducationService.cs:45`、`WorkExpService.cs:41`、`ProjectService.cs:152,766` 等。

---

### 4.4 🟢 魔法字符串无常量管理

```csharp
// CertificateService.cs
=> _dictSvc.GetDataByTypeAsync("cert_type");

// ContractService.cs
=> _dictSvc.GetDataByTypeAsync("contract_type");
```

应定义为 `public static class DictType { public const string CertType = "cert_type"; ... }`。

---

## 五、死代码（🟢 轻微）

| 位置 | 说明 |
|------|------|
| `IUnitOfWork` 的事务三方法 | 定义后从未被调用,见 2.2 |
| `AppDbContext.SeedAsync()` | 空实现(`await Task.CompletedTask`),种子逻辑已在 `HasData()` 中实现 |

---

## 六、亮点（保持不变）

| 实践 | 位置 | 说明 |
|------|------|------|
| 文件上传集中管理 | `Common/FileUploadHelper.cs` | 非 Web 根存储、扩展名白名单 |
| 文件下载集中管理 | `Common/FileServingHelper.cs` | MIME 推导、中文 RFC 5987 编码、流式返回 |
| 全局软删除过滤器 | `AppDbContext.cs:189-197` | EF Core QueryFilter 全局生效 |
| 审计字段自动填充 | `AppDbContext.cs:167-181` | SaveChangesAsync 重写 |
| CSRF 全局强制 | `Program.cs` | antiforgery token 全局校验 |
| 三层 Filter 体系 | `Filters/Filters.cs` | GlobalExceptionFilter/OperationLogFilter/ValidationFilter |
| Redis 降级策略 | `Program.cs` | Redis 不可用时自动降级内存 |
| EF Core 外键索引 | `AppDbContext.cs` | 高频外键建索引 |

---

## 七、整改优先级路线图

### P0（立即修复）
1. IUnitOfWork 的 37 个属性拆分,各 Service 改为直接注入 `IRepository<T>`
2. 事务方法(`BeginTransactionAsync` 等)要么使用要么删除——当前是死代码
3. 所有 Controller 的 `SaveChangesAsync` 下沉到 Service 层

### P1（本月内）
4. 统一分页:全部走 `ToPagedAsync`,删除 `Repository<T>.GetPagedAsync`
5. `BuildTree<T>` 泛型抽象、`GetProgressText` 移入共享类
6. 统一 `GetPagedAsync` 返回形状为 `PagedResult<T>`
7. 创建根级 `.editorconfig`
8. `DeleteFileAsync` 泛型抽象

### P2（下季度）
9. 全局时间策略统一为 `DateTime.UtcNow`
10. 消除手设 `CreatedAt`,统一走 DbContext 拦截
11. 字典类型常量化(`DictType` 枚举)
12. ReportController 临时聚合查询整理(移入 Service 或标注为轻量临时查询)

---

*本报告由 AI 代码审计工具生成(合并版),结合深度静态分析与已有审计结果交叉核实。*
