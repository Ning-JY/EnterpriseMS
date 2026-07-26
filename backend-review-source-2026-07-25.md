# EnterpriseMS 后端复查报告（正本 source/repos）

> 复查对象：`C:\Users\ningj\source\repos\EnterpriseMS`（含 `.git`、AI 模块、`Infrastructure/`、`Middlewares/`、`Migrations/`、`Common/FileUploadHelper` 已硬化）
> 日期：2026-07-25
> 结论：整体架构健康，安全基线达标；剩余问题集中在「匿名越权」「韧性/重试」「AI 模块健壮性」三处。

---

## ✅ 已做得好的（确认无误，无需改）

- **Hangfire 降级**：`Program.cs` 已实现 `redisOk ? UseRedisStorage(redisConn) : UseMemoryStorage()`，且 `redisOk` 来自启动时 2s 超时连接探针（`AbortOnConnectFail=false`），缓存同理。
- **文件上传已硬化**（正本比之前看的副本强）：`FileUploadHelper` 同时具备
  - 统一扩展名白名单 `DefaultAllowedExts`（排除 `.html/.svg/.js/.exe` 等）；
  - 落地到 **非 Web 根目录** `uploads/{folder}/`（在 `wwwroot` 之外，静态文件中间件不渲染，从根上杜绝存储型 XSS）。
- **密码哈希**：BCrypt cost 12（`UserService`/`SystemSeeds`），无明文落库。
- **全局 CSRF**：`AutoValidateAntiforgeryTokenAttribute` 全局注册 + Antiforgery 头 `RequestVerificationToken`，匿名 POST 也强制校验。
- **异常过滤器**：`GlobalExceptionFilter` 仅记录完整堆栈、对外只返回「服务器内部错误」，**无信息泄露**。
- **RBAC**：`[HasPermission]` + `superadmin` 旁路健全；`DebugController` 每 action 校验 superadmin；Hangfire 仪表盘仅 superadmin。
- **Repository 读取普遍 `AsNoTracking()`**，UoW 同请求内复用仓储实例。
- **开放重定向防护**：`Login` 用 `Url.IsLocalUrl(returnUrl)` 校验回跳地址。
- **HSTS / HTTPS 重定向**仅在非 Development 环境启用，符合预期。
- **`EnableSensitiveDataLogging`** 仅 Development 开启。

---

## 🔴 P0 严重：匿名越权删除 / 上传

`Controllers/Tool/TemplateReportController.cs` 类上 `[AllowAnonymous]`，却暴露：
- `POST /templatereport/delete` → `DeleteTemplate`（**任意匿名删除**模板）
- `POST /templatereport/configure-template` → `ConfigureTemplate`（**匿名文件上传**）
- `GET /templatereport/templates`、`/template/{id}`、`/download/{id}`（匿名读）

虽上传本身已被白名单 + 非 Web 根存储缓解（不会变 RCE），但**匿名删除 + 任意上传占用存储**仍是不该存在的攻击面。报告模板属于内部资产，不应匿名。

**修复**：去掉 `[AllowAnonymous]`，改为 `[Authorize]`（或 `[HasPermission("report:template:manage")]`）。若确要对外提供报告生成，至少把 `delete`/`configure-template` 收口到鉴权。

---

## 🟠 P1 高优先级

### 1. EF Core 未启用连接重试（远程 MySQL/Redis 韧性缺口）
`Program.cs` 第 49-52 行 `AddDbContext` 只设了 `CommandTimeout(60)`，**没有 `EnableRetryOnFailure`**。MySQL/Redis 在独立主机（192.168.1.100），瞬时网络抖动会直接 5xx。

```csharp
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 36)),
        x => x.MigrationsAssembly("EnterpriseMS")
              .EnableRetryOnFailure(           // ← 加上
                  maxRetryCount: 3,
                  maxRetryDelay: TimeSpan.FromSeconds(5),
                  errorNumbersToAdd: null)
              .CommandTimeout(60))
       .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
```

### 2. 缺少全局默认拒绝授权策略（P0 的根因）
全项目无 `AddAuthorization(opt => opt.FallbackPolicy = ...)`。**任何新 Controller 只要忘记贴 `[Authorize]` 就完全匿名**——`TemplateReportController` 正是此漏洞的实例。

**修复（架构级）**：注册默认拒绝策略，再把确实需要匿名的端点显式标 `[AllowAnonymous]`（Login GET/POST、Home `Forbidden`/`Error`、`ToolController` 计算器、`/pub` 资讯）：

```csharp
builder.Services.AddAuthorization(opt =>
    opt.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build());
```

### 3. AI 服务健壮性（新增模块，调用热点）
`Services/AI/OpenAIService.cs`：
- **每次请求都 `BuildKernel()` 新建 Kernel/Connector**（第 48-54 行），连接无法复用。`OpenAIService` 已注册为 `Singleton`，应**缓存 Kernel 单例**。
- **未透传 `CancellationToken`**：`GetChatMessageContentAsync(history)` / `GetStreamingChatMessageContentsAsync(history)` 都没传 token；`BidController`→`BidService`→`OpenAIService` 整条链路也无 token。客户端断开后仍在烧 token，流式 SSE 尤其明显。
- **无 LLM 超时 / 重试**：上游挂起则请求无限等待。建议给底层 `HttpClient` 设超时，或包一个带超时的 `CancellationToken`。
- **AI 密钥明文落盘** `App_Data/ai-config.json`（`AiConfigPath`）：与之前明文密码同类问题 → 改为环境变量 / 密钥管理（与 `DB_PASSWORD`/`REDIS_PASSWORD` 占位符思路一致）。

**建议改法（摘要）**：
```csharp
private Kernel? _kernelCache;
private readonly object _kernelLock = new();
private Kernel? GetOrBuildKernel(string apiKey, string baseUrl, string model)
{
    if (_kernelCache != null) return _kernelCache;
    lock (_kernelLock)
    {
        if (_kernelCache != null) return _kernelCache;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        _kernelCache = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, endpoint: new Uri(baseUrl))
            .Build();
        return _kernelCache;
    }
}
// 调用处：await chat.GetChatMessageContentAsync(history, cancellationToken);
//          await foreach (var c in chat.GetStreamingChatMessageContentsAsync(history, cancellationToken: ct))
```
并在 `IAIService` 接口的各方法增加 `CancellationToken ct = default`，从 `BidController` 传入 `HttpContext.RequestAborted`。

---

## 🟡 P2 中优先级

### 1. 登录无频率限制 / 账号锁定
`AccountController.Login` 仅做了空值校验 + BCrypt 校验，无失败计数、无锁定、无验证码/限流。BCrypt cost 12 能拖慢暴力破解，但**账号级锁定缺失**。建议加：失败 5 次锁定 N 分钟（用分布式缓存/Redis 计数器，因 `redisOk` 已探测）。

### 2. AI 接口无配额 / 成本防护
`BidController` 的解析/生成/审校均触发真实 LLM 调用，无调用频率或额度限制，恶意/误操作可快速产生费用。建议接入全局或用户级限流。

### 3. 种子账号弱口令
`SystemSeeds.cs` 6 个初始账号密码全是 `123456`。建议：首次登录强制改密，或生成随机初始密码并提示。

### 4. `Logout` 缺 `[Authorize]`
`AccountController.Logout` 仅有 `[ValidateAntiForgeryToken]`，无 `[Authorize]`；匿名 POST 可触发（无害但应规范）。补 `[Authorize]` 即可。

---

## 🟢 P3 建议

- **AI 分块循环调用成本**：`BidService.AnalyzeBidDocumentAsync` 对文档分块后逐个调 LLM（第 166 行），大文档费用可观；评估是否合并/降块数。
- **潜在 N+1**：项目成员列表（`ProjectMember`→`Employee`）等关联查询确认是否 `Include`/`AsSplitQuery`；高频列表接口建议排查。
- **迁移与 git**：按你的要求 `Migrations/` 不进 git，运行时 `MigrateAsync()` 仍依赖本地磁盘上的迁移文件——新机器部署时需确保 `Migrations/` 目录随发布包拷贝（或用 `dotnet ef migrations script` 生成 SQL / 迁移包），否则首次启动无迁移可跑。
- `OpenAIService` 使用 `pragma warning disable SKEXP0010`（`AddOpenAIChatCompletion` 实验 API），升级 SK 版本时注意破坏性变更。

---

## 建议整改顺序
1. **P0**：`TemplateReportController` 去掉 `[AllowAnonymous]` → `[Authorize]`。
2. **P1**：加 `EnableRetryOnFailure` → 加全局默认拒绝策略（并审计匿名端点）→ AI 模块缓存 Kernel + 透传 CancellationToken + 密钥去明文。
3. **P2**：登录锁定 / AI 限流 / 种子弱口令 / Logout 鉴权。

> 说明：本次仅复查，未改动源码。正本此前已包含前几轮整改（Hangfire 降级、密码占位符、前端 XSS），本次不重复。
