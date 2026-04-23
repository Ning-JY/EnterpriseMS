using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hangfire;
using Hangfire.MemoryStorage;
using FluentValidation.AspNetCore;
using EnterpriseMS.Filters;
using EnterpriseMS.Infrastructure.Cache;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Infrastructure.Repositories;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Middlewares;
using EnterpriseMS.Services.Impl;
using EnterpriseMS.Services.Interfaces;
using EnterpriseMS.Services.Mappings;
using Hangfire.Dashboard;
using StackExchange.Redis;
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

    // ── 数据库 ────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
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
    builder.Services.AddScoped<IProjectService,       ProjectService>();
    builder.Services.AddScoped<IEmployeeQueryService, EmployeeQueryService>();

    // ── 缓存：Redis 可用则 Redis，否则自动降级内存缓存 ────────
    var redisConn = builder.Configuration["Redis:Connection"] ?? "";
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

    // ── 请求体大小限制（无限制，支持大文件上传）────────────────
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opt =>
    {
        opt.MultipartBodyLengthLimit = long.MaxValue;
        opt.ValueLengthLimit = int.MaxValue;
        opt.MultipartHeadersLengthLimit = int.MaxValue;
    });
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestBodySize = null; // 无限制
    });

    // ── MVC ───────────────────────────────────────────────────
    builder.Services.AddControllersWithViews(opt =>
    {
        opt.Filters.Add<GlobalExceptionFilter>();
        opt.Filters.Add<OperationLogFilter>();
    }).AddJsonOptions(opt =>
        opt.JsonSerializerOptions.PropertyNamingPolicy = null); // 保持字段名原样（小写由ApiResult控制）

    // ── AutoMapper 12.x ───────────────────────────────────────
    builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

    // ── FluentValidation ──────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation();

    // ── Hangfire：Redis 可用则 Redis 存储，否则内存存储 ───────
    builder.Services.AddHangfire(cfg =>
    {
        cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings();
        if (redisOk)
            cfg.UseRedisStorage(redisConn, new Hangfire.Redis.StackExchange.RedisStorageOptions
            {
                Prefix = "EMS:Hangfire:", Db = 1,
            });
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

    app.MapControllerRoute("public",  "pub/{controller=Info}/{action=Index}/{id?}");
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
        }
        catch (Exception ex)
        {
            // 迁移失败只记录日志，不阻止服务启动（避免迁移脚本问题导致服务不可用）
            Log.Error(ex, "数据库迁移失败，请检查连接字符串和数据库权限：{Msg}", ex.Message);
        }
    }

    // ── Hangfire 定时任务 ─────────────────────────────────────
    try
    {
        RecurringJob.AddOrUpdate<HangfireJobs>("check-contract-expire",
            j => j.CheckContractExpireAsync(), Cron.Daily(9));
        RecurringJob.AddOrUpdate<HangfireJobs>("check-cert-expire",
            j => j.CheckCertExpireAsync(), Cron.Daily(9));
        RecurringJob.AddOrUpdate<HangfireJobs>("check-milestone-overdue",
            j => j.CheckMilestoneOverdueAsync(), Cron.Daily(8));
    }
    catch (Exception ex)
    {
        Log.Warning("Hangfire 定时任务注册失败（不影响主功能）：{Msg}", ex.Message);
    }

    Log.Information("EnterpriseMS 启动成功 → http://localhost:5080");
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

// ── Hangfire 定时任务 ─────────────────────────────────────────
public class HangfireJobs
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly ILogger<HangfireJobs> _log;

    public HangfireJobs(AppDbContext db, IConfiguration cfg, ILogger<HangfireJobs> log)
    { _db = db; _cfg = cfg; _log = log; }

    public async Task CheckContractExpireAsync()
    {
        var days = _cfg.GetValue<int>("Hangfire:ContractWarningDays", 30);
        var n    = await _db.Contracts
            .CountAsync(c => !c.IsDeleted && c.Status == 0
                          && c.EndDate <= DateTime.Today.AddDays(days));
        if (n > 0) _log.LogWarning("【合同到期预警】{Count} 份将在 {Days} 天内到期", n, days);
    }

    public async Task CheckCertExpireAsync()
    {
        var days = _cfg.GetValue<int>("Hangfire:CertWarningDays", 60);
        var n    = await _db.Certificates
            .CountAsync(c => !c.IsDeleted && c.Status == 0
                          && c.ExpireDate.HasValue && c.ExpireDate <= DateTime.Today.AddDays(days));
        if (n > 0) _log.LogWarning("【证书到期预警】{Count} 张将在 {Days} 天内到期", n, days);
    }

    public async Task CheckMilestoneOverdueAsync()
    {
        var list = await _db.Milestones
            .Where(m => !m.IsDeleted && m.Status != 2
                     && m.PlanDate < DateTime.Today && !m.IsOverdue)
            .ToListAsync();
        if (!list.Any()) return;
        foreach (var m in list) m.IsOverdue = true;
        await _db.SaveChangesAsync();
        _log.LogWarning("【里程碑逾期】标记 {Count} 个", list.Count);
    }
}
