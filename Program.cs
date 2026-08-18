using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hangfire;
using Hangfire.MemoryStorage;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Filters;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Infrastructure.Repositories;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Middlewares;
using EnterpriseMS.Services.Impl;
using EnterpriseMS.Services.Interfaces;
using EnterpriseMS.Services.Impl.TemplateSources;
using EnterpriseMS.Services.Mappings;
using EnterpriseMS.Services.AI;
using EnterpriseMS.Services.Export;
using Hangfire.Dashboard;
using StackExchange.Redis;
using Hangfire.Redis;
using Hangfire.Redis.StackExchange;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File("logs/app-.log",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 30));

    var connStr = builder.Configuration.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("缺少数据库连接字符串 Default");

    // 数据库连接字符串各组件可由环境变量覆盖（未设置时沿用配置文件默认值）：
    //   DB_HOST      → Server  （地址）
    //   DB_PORT      → Port
    //   DB_NAME      → Database（库名）
    //   DB_USER      → Uid      （用户名）
    //   DB_PASSWORD  → Pwd      （密码）
    var dbEnvOverrides = new (string Key, string Env)[]
    {
        ("Server",   "DB_HOST"),
        ("Port",     "DB_PORT"),
        ("Database", "DB_NAME"),
        ("Uid",      "DB_USER"),
        ("Pwd",      "DB_PASSWORD"),
    };
    if (dbEnvOverrides.Any(o => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(o.Env))))
    {
        var dict = connStr.Split(';', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Split('=', 2))
                          .ToDictionary(a => a[0].Trim(), a => a.Length > 1 ? a[1] : "",
                                        StringComparer.OrdinalIgnoreCase);
        foreach (var (key, env) in dbEnvOverrides)
        {
            var val = Environment.GetEnvironmentVariable(env);
            if (!string.IsNullOrEmpty(val)) dict[key] = val;
        }
        connStr = string.Join(";", dict.Select(p => $"{p.Key}={p.Value}")) + ";";
        Log.Information("数据库连接字符串已由环境变量覆盖：{Keys}",
            string.Join(",", dbEnvOverrides
                .Where(o => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(o.Env)))
                .Select(o => o.Env)));
    }

    // ── 数据库 ────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 36)),
            x => x.MigrationsAssembly("EnterpriseMS").CommandTimeout(60))
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

    // ── 仓储 & UoW ────────────────────────────────────────────
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // ── 应用服务 ──────────────────────────────────────────────
    builder.Services.AddScoped<IUserService,          UserService>();
    builder.Services.AddScoped<IPermissionService,    PermissionService>();
    builder.Services.AddScoped<IRoleService,          RoleService>();
    builder.Services.AddScoped<IMenuService,          MenuService>();
    builder.Services.AddScoped<IDeptService,          DeptService>();
    builder.Services.AddScoped<IDictService,          DictService>();
    builder.Services.AddScoped<IOperLogService,       OperLogService>();
    builder.Services.AddScoped<IConfigService,        ConfigService>();
    builder.Services.AddScoped<IProjectService,       ProjectService>();
    builder.Services.AddScoped<IEmployeeQueryService, EmployeeQueryService>();
    builder.Services.AddScoped<IEmployeeService,      EmployeeService>();
    builder.Services.AddScoped<IContractService,       ContractService>();
    builder.Services.AddScoped<ICertificateService,    CertificateService>();
    builder.Services.AddScoped<IEducationService,      EducationService>();
    builder.Services.AddScoped<IWorkExpService,        WorkExpService>();
    builder.Services.AddScoped<IEmployeeAttachmentService, EmployeeAttachmentService>();
    builder.Services.AddScoped<IKbService,             KbService>();
    builder.Services.AddScoped<IHangfireService, HangfireService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // ── 投标管理 ──────────────────────────────────────────────
    builder.Services.AddSingleton<IAIService, OpenAIService>();
    builder.Services.AddSingleton<DocumentParser>();
    builder.Services.AddSingleton<IWordExportService, WordExportService>();
    builder.Services.AddScoped<IBidService, BidService>();

    // ── 模板化报告生成 ──────────────────────────────────────────
    builder.Services.AddScoped<IReportGeneratorService, ReportGeneratorService>();

    // ── 通用模板数据源（DI 自动发现，供配置器与填充向导枚举）──────────
    builder.Services.AddScoped<ITemplateDataSource, ManualDataSource>();
    builder.Services.AddScoped<ITemplateDataSource, ConfigDataSource>();
    builder.Services.AddScoped<ITemplateDataSource, ProjectDataSource>();
    builder.Services.AddScoped<ITemplateDataSource, EmployeeDataSource>();
    builder.Services.AddScoped<ITemplateDataSource, ProjectContractDataSource>();
    builder.Services.AddScoped<ITemplateDataSource, EmployeeContractDataSource>();

    // ── 报表查询（从 ReportController 下沉，避免 Controller 直连 DbContext）──
    builder.Services.AddScoped<IReportService, ReportService>();

    // ── 系统种子 / 维护（从 DebugController 下沉，避免 Controller 直连 DbContext）──
    builder.Services.AddScoped<ISystemSeedService, SystemSeedService>();

    // ── 缓存：Redis 可用则 Redis，否则自动降级内存缓存 ────────
    var redisConn = builder.Configuration["Redis:Connection"] ?? "";
    // Redis 连接各组件可由环境变量覆盖（未设置时沿用配置文件默认值）。
    // 注意：StackExchange.Redis 仅支持 host:port 简写，不支持 host=/port= 命名键，故占位符写为 ${REDIS_HOST}:${REDIS_PORT}：
    //   REDIS_HOST     → 主机地址（host 部分）
    //   REDIS_PORT     → 端口（port 部分）
    //   REDIS_PASSWORD → password（密码）
    var redisEnvMap = new (string Placeholder, string Env)[]
    {
        ("${REDIS_HOST}",     "REDIS_HOST"),
        ("${REDIS_PORT}",     "REDIS_PORT"),
        ("${REDIS_PASSWORD}", "REDIS_PASSWORD"),
    };
    if (redisEnvMap.Any(o => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(o.Env))))
    {
        foreach (var (ph, env) in redisEnvMap)
        {
            var val = Environment.GetEnvironmentVariable(env);
            if (!string.IsNullOrEmpty(val)) redisConn = redisConn.Replace(ph, val);
        }
        Log.Information("Redis 连接字符串已由环境变量覆盖：{Keys}",
            string.Join(",", redisEnvMap
                .Where(o => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(o.Env)))
                .Select(o => o.Env)));
    }
    var redisOk   = false;

    if (!string.IsNullOrWhiteSpace(redisConn))
    {
        try
        {
            var rCfg = ConfigurationOptions.Parse(redisConn);
            rCfg.ConnectTimeout     = 2000;
            rCfg.AbortOnConnectFail = false;
            using var probe = ConnectionMultiplexer.Connect(rCfg);
            redisOk = probe.IsConnected;
        }
        catch (Exception ex)
        {
            Log.Warning("Redis 连接失败，将使用内存缓存：{Msg}", ex.Message);
        }
    }

    if (redisOk)
    {
        builder.Services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConn;
            opt.InstanceName  = "EMS:";
        });
        Log.Information("Redis 缓存已启用（{Conn}）", redisConn);
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
        Log.Warning("Redis 不可用，使用内存缓存（重启后权限缓存清空，功能不受影响）");
    }
    builder.Services.AddMemoryCache(); // 通知同步等本地短缓存（与分布式缓存各自独立）
    builder.Services.AddScoped<IPermissionCache, RedisPermissionCache>();
    // ── Cookie 认证 ───────────────────────────────────────────
    // ── Cookie Secure 策略：
    // 读取配置项 Cookie:SecurePolicy，支持三种值：
    //   Always - 仅 HTTPS（适合纯 HTTPS 生产环境）
    //   None   - HTTP/HTTPS 均可（适合局域网 HTTP 访问）
    //   SameAsRequest - 跟随请求协议（最灵活，同时支持 HTTP 和 HTTPS）← 推荐
    var securePolicyStr = builder.Configuration["Cookie:SecurePolicy"] ?? "SameAsRequest";
    var securePolicy = securePolicyStr.ToLower() switch
    {
        "always" => CookieSecurePolicy.Always,
        "none" => CookieSecurePolicy.None,
        _ => CookieSecurePolicy.SameAsRequest, // 默认：跟随请求协议
    };
    // ── Cookie 认证 ───────────────────────────────────────────
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(opt =>
        {
            opt.LoginPath         = "/Account/Login";
            opt.LogoutPath        = "/Account/Logout";
            opt.AccessDeniedPath  = "/Account/AccessDenied";
            opt.ExpireTimeSpan    = TimeSpan.FromHours(8);
            opt.SlidingExpiration = true;
            opt.Cookie.HttpOnly   = true;
            opt.Cookie.Name       = "EMS.Auth";
            opt.Cookie.SameSite   = SameSiteMode.Lax;
            opt.Cookie.SecurePolicy = securePolicy;  // 由配置文件控制
        });

    // ── 请求体大小限制（统一 500MB，单一来源）─────────────────
    // 大小上限只在「此处」设定；各 Controller 不再单独限制（避免 Kestrel 缓冲期 DoS）。
    const long MaxUploadBytes = 500L * 1024 * 1024; // 500MB
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opt =>
    {
        opt.MultipartBodyLengthLimit = MaxUploadBytes;
        opt.ValueLengthLimit = int.MaxValue;
        opt.MultipartHeadersLengthLimit = int.MaxValue;
    });
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestBodySize = MaxUploadBytes;
    });

    // ── MVC ───────────────────────────────────────────────────
    // 全局 CSRF 自动校验：对所有非匿名、非安全方法（POST/PUT/DELETE/PATCH）
    // 强制校验 antiforgery token。前端通过 jQuery $.ajaxSetup（site.js）与
    // fetch 包装统一在 RequestVerificationToken 头中携带 token。
    builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
    builder.Services.AddControllersWithViews(opt =>
    {
        opt.Filters.Add<GlobalExceptionFilter>();
        opt.Filters.Add<OperationLogFilter>();
        opt.Filters.Add<ValidationFilter>();
        opt.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }).AddJsonOptions(opt =>
        opt.JsonSerializerOptions.PropertyNamingPolicy = null); // 保持字段名原样（小写由ApiResult控制）

    // ── AutoMapper 12.x ───────────────────────────────────────
    builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

    // ── FluentValidation ──────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // ── Hangfire：有 Redis 缓存则 Redis 存储，否则降级为内存存储 ─
    builder.Services.AddHangfire(cfg =>
    {
        cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings();
        if (redisOk)
            cfg.UseRedisStorage(redisConn);
        else
            cfg.UseMemoryStorage();
    });
    builder.Services.AddHangfireServer(opt =>
    {
        opt.WorkerCount = 2;
        opt.ServerName  = "EnterpriseMS-Worker";
    });

    // ── HttpContextAccessor ───────────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── 健康检查 ──────────────────────────────────────────────
    var hc = builder.Services.AddHealthChecks()
                    .AddMySql(connStr, name: "mysql");
    if (redisOk) hc.AddRedis(redisConn, name: "redis");

    // ═════════════════════════════════════════════════════════
    var app = builder.Build();
    // ═════════════════════════════════════════════════════════

    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseHangfireDashboard("/jobs", new DashboardOptions
    {
        Authorization  = new[] { new HangfireAuthFilter() },
        DashboardTitle = "企业管理系统 - 任务队列",
        AppPath        = "/",
    });

    app.MapControllerRoute("public",  "pub/{action=Index}/{id?}", new { controller = "Info" });
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    app.MapHealthChecks("/health");

    // ── 数据库迁移（所有环境）────────────────────────────────
    // MigrateAsync() 是幂等操作：
    //   - 首次启动：创建表结构 + 写入 HasData() 种子数据
    //   - 再次启动：检测到已是最新版本，直接跳过，不重复执行
    //   - 新版本发布：只执行新增的 Migration，不影响已有数据
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await db.Database.MigrateAsync();
            Log.Information("数据库迁移检查完成（无待执行迁移则直接跳过）");

            // 首次启动把现有 template-manifest.json 迁入模板表（幂等，已存在则跳过）
            try
            {
                var rpt = scope.ServiceProvider.GetService<IReportGeneratorService>();
                if (rpt != null) await rpt.SeedFromManifestIfEmptyAsync();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "模板种子数据同步跳过：{Msg}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            // 迁移失败只记录日志，不阻止服务启动（避免迁移脚本问题导致服务不可用）
            Log.Error(ex, "数据库迁移失败，请检查连接字符串和数据库权限：{Msg}", ex.Message);
        }

        // 启动即把证件/合同到期提醒聚合进通知中心（带 5 分钟缓存，幂等）
        try
        {
            var notifSvc = scope.ServiceProvider.GetService<INotificationService>();
            if (notifSvc != null) await notifSvc.SyncExpiryAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "通知中心到期提醒同步跳过：{Msg}", ex.Message);
        }
    }

    // ── Hangfire 定时任务 ─────────────────────────────────────
    try
    {
        RecurringJob.AddOrUpdate<IHangfireService>("check-contract-expire",
            j => j.CheckContractExpireAsync(), Cron.Daily(9));
        RecurringJob.AddOrUpdate<IHangfireService>("check-cert-expire",
            j => j.CheckCertExpireAsync(), Cron.Daily(9));
        RecurringJob.AddOrUpdate<IHangfireService>("check-milestone-overdue",
            j => j.CheckMilestoneOverdueAsync(), Cron.Daily(8));
        // 通知中心：每日刷新证件/合同到期提醒（与上面检查逻辑保持一致，写入通知表）
        RecurringJob.AddOrUpdate<INotificationService>("sync-notifications",
            j => j.SyncExpiryAsync(), Cron.Daily(9, 0));
    }
    catch (Exception ex)
    {
        Log.Warning("Hangfire 定时任务注册失败（不影响主功能）：{Msg}", ex.Message);
    }

    Log.Information("EnterpriseMS 启动成功 → http://localhost:5090");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败");
    throw;
}
finally { Log.CloseAndFlush(); }

// ── Hangfire 仪表盘鉴权 ────────────────────────────────────────
public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext ctx)
    {
        var http = ctx.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true
            && http.User.IsInRole("superadmin");
    }
}


