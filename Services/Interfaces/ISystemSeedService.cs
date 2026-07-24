using System.Collections.Generic;

namespace EnterpriseMS.Services.Interfaces;

/// <summary>数据库状态统计（Debug 首页用）</summary>
public class SeedStatsDto
{
    public Dictionary<string, int> Stats { get; set; } = new();
    public List<string> Pending { get; set; } = new();
    public List<string> Applied { get; set; } = new();
}

/// <summary>全量种子写入结果</summary>
public class SeedAllResult
{
    public int TotalAdded { get; set; }
    public List<string> Details { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 系统种子 / 维护服务 —— 将 DebugController 中直接写在 Controller 内的
/// AppDbContext 持久化（菜单/角色菜单/字典的种子写入、迁移、缓存刷新等）下沉到 Service 层，
/// 使 Controller 不再直连 DbContext。
/// </summary>
public interface ISystemSeedService
{
    bool IsSuperAdmin(long userId);
    Task<SeedStatsDto> GetStatsAsync();
    Task<SeedAllResult> SeedAllAsync();
    Task<List<string>> SeedMenuAndDictsAsync();
    Task<int> ClearAllUserCacheAsync();
    Task<(List<string> Pending, string? Error)> MigrateAsync();
}
