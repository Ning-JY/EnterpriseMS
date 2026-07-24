# EnterpriseMS 代码审计报告（修复后复查）

**审计日期**：2026年7月24日  
**审计对象**：C:\Users\ningj\source\repos\EnterpriseMS  
**审计范围**：代码冗余、重复方法、代码风格一致性、分层逻辑

---

## 一、项目概况

| 项目信息 | 详情 |
|---------|------|
| **技术栈** | ASP.NET Core (MVC) + EF Core + MySQL + Redis + Hangfire |
| **架构模式** | 分层架构：Controllers → Services → Infrastructure |
| **总文件数** | ~100+ C# 文件 |
| **主要模块** | 用户/角色/权限、员工管理、项目管理、投标管理、知识库、报表 |

---

## 二、已修复的问题 ✅

| # | 原始问题 | 状态 | 说明 |
|---|---------|------|------|
| 7 | GetErrors() 方法重复定义 | **已修复** | 提取到 `BaseAuthController` 基类（第46-48行） |
| 5 | EmployeeController 重复字典加载 | **已修复** | 移除了 `ViewBag.DictCertType`，只保留 `ViewBag.CertTypes` |
| 10 | ProjectController 代码格式问题 | **已修复** | 删除了末尾无用的 `GetErrors()` 私有方法，格式已规范 |

### 新增的改进 ✅

| # | 改进项 | 说明 |
|---|-------|------|
| 新增 | `FileServingHelper` 工具类 | 统一处理文件下载，解决了多处文件下载逻辑散落问题 |
| 新增 | `PhysicalFileResult` 流式返回 | 不再 `ReadAllBytes` 整块读入内存，支持 Range 请求 |
| 改进 | KB/Project/HR 下载方法 | 已统一使用 `FileServingHelper.ServePhysicalFile()` |

---

## 三、仍存在的问题

### 🔴 严重问题

#### 1. KbController 仍直接使用 IUnitOfWork

**文件**：`Controllers/Kb/KbController.cs:16-20, 26-48`

Controller 直接注入 `IUnitOfWork`，绕过 Service 层直接操作数据库：

```csharp
private readonly IUnitOfWork _uow;

// 直接使用 _uow 操作数据库
var categories = await _uow.KbCategories.Query()
    .Where(c => c.Status == 1).OrderBy(c => c.Sort).ToListAsync();
```

---

#### 2. ReportController 仍直接使用 AppDbContext

**文件**：`Controllers/Report/ReportController.cs:17-22, 35-55`

ReportController 注入 `AppDbContext`，包含约 200 行复杂的数据查询、聚合计算逻辑：

```csharp
private readonly AppDbContext _db;

// 直接查询数据库
var invoicesQ = _db.ProjInvoices
    .Include(i => i.Project).ThenInclude(p => p!.Dept)
    .Where(i => !i.IsDeleted && i.Project != null && !i.Project.IsDeleted);
```

---

#### 3. AccountController 仍直接使用 AppDbContext

**文件**：`Controllers/AccountController.cs:55-58`

登录逻辑直接查询数据库获取用户角色：

```csharp
var roleCodes = _db.SysUserRoles
    .Where(ur => ur.UserId == user.Id)
    .Join(_db.SysRoles, ur => ur.RoleId, r => r.Id, (ur, r) => r.RoleCode)
    .ToList();
```

---

### 🟡 中等问题

#### 4. API 返回风格仍不一致

UserController、RoleController、ProjectController 仍使用 `Json(ApiResult<object>.Ok/Fail(...))` 风格，而 HR 模块使用 `ApiOk/ApiFail` 风格。

**问题分布**：
- `UserController.cs` 全文（143行）
- `RoleController.cs` 全文（103行）
- `ProjectController.cs` 部分方法

---

#### 5. 异常处理模式仍不统一

以下方法仍只捕获 `BusinessException` 而不是同时捕获 `NotFoundException`：

| 文件 | 方法 | 行号 |
|------|------|------|
| `DictController.cs` | CreateType | 43 |
| `DictController.cs` | CreateData | 81 |
| `DeptController.cs` | Create | 48 |
| `MenuController.cs` | Create | 43 |
| `UserController.cs` | Create | 62 |
| `UserController.cs` | SetStatus | 105 |
| `UserController.cs` | ChangePwd | 132 |
| `RoleController.cs` | Create | 63 |

---

#### 6. ConfigController 异常处理模式不同

**文件**：`Controllers/System/ConfigController.cs:41-44`

捕获所有 `Exception` 并加前缀，与项目其他地方不一致：

```csharp
catch (Exception ex)
{
    return ApiFail($"保存失败：{ex.Message}");
}
```

---

#### 7. ProjectController 仍直接使用 AppDbContext

**文件**：`Controllers/Project/ProjectController.cs:111-114`

仍直接查询 `_db.SysConfigs`：

```csharp
var prefix = await _db.SysConfigs
    .Where(c => c.ConfigKey == "project_no_prefix")
    .Select(c => c.ConfigValue)
    .FirstOrDefaultAsync() ?? "";
```

---

### 🟢 轻微问题

#### 8. 命名空间和 using 语句风格不一致

- `RoleController.cs:1` 有 `using EnterpriseMS.Controllers;`（自己引用自己的命名空间）
- 部分文件使用全局 using，部分使用完整 using

---

#### 9. ViewModel 模式混用

- **使用 ViewBag（弱类型）**：大部分 Controller（EmployeeController, CertificateController, ContractController 等）
- **使用强类型 DTO**：`ReportController.cs` 返回强类型 `View(byEmployee)`；`BidController.cs` 返回强类型 `View(bid)`

---

#### 10. EducationController 和 WorkExpController 代码几乎完全重复

`EducationController.cs` 和 `WorkExpController.cs` 的结构、方法签名、实现模式完全一致，仅类型名不同。

---

#### 11. CertificateController 和 ContractController 的文件操作重复

两个 Controller 都有几乎相同的 `Upload`、`Download`、`DeleteFile` 方法结构。

---

#### 12. UnitOfWork 中 Repository 属性过多

**文件**：`Infrastructure/Repositories/Repository.cs:56-122`

UnitOfWork 类定义了 **29 个** Repository 属性，职责过重。

---

#### 13. ProjectController 依赖注入过多

**文件**：`Controllers/Project/ProjectController.cs:21-37`

构造函数注入 **8 个** 服务依赖。

---

## 四、问题统计对比

| 严重级别 | 修复前 | 修复后 | 变化 |
|---------|-------|-------|------|
| 🔴 严重 | 2 | 3 | +1（AccountController 分层问题） |
| 🟡 中等 | 6 | 4 | -2（GetErrors/字典重复已修复） |
| 🟢 轻微 | 7 | 6 | -1（代码格式已修复） |
| **合计** | **15** | **13** | -2 |

---

## 五、优先级建议（更新）

### 立即修复（P0）
1. **KbController** - 移除 IUnitOfWork，通过 Service 层操作
2. **ReportController** - 将复杂查询逻辑移到 Service 层
3. **AccountController** - 通过 UserService 获取用户角色

### 短期改进（P1）
4. **统一 API 返回风格** - 将 `Json(ApiResult<object>...)` 替换为 `ApiOk/ApiFail`
5. **统一异常处理** - 所有 Create 方法添加 `NotFoundException` 捕获
6. **ConfigController** - 统一异常处理模式

### 长期优化（P2）
7. 拆分大型 Controller（ProjectController、BidController）
8. 引入子表 CRUD 基类（Education/WorkExp）
9. 拆分 UnitOfWork（29个属性过多）

---

## 六、代码优点

项目整体架构质量良好：

1. **已修复代码重复** - GetErrors() 提取到基类，字典加载去重
2. **文件服务统一** - 新增 FileServingHelper 统一文件下载逻辑
3. **依赖注入规范** - 使用接口注入，便于测试和维护
4. **权限控制完善** - HasPermission 特性 + Redis 缓存
5. **缓存降级策略** - Redis 不可用时自动降级内存缓存
6. **全局异常过滤器** - 统一处理异常和 CSRF
7. **日志记录** - 操作日志自动记录
