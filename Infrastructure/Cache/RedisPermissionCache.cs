using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EnterpriseMS.Infrastructure.Cache;

public interface IPermissionCache
{
    Task<List<string>?> GetUserPermsAsync(long userId);
    Task SetUserPermsAsync(long userId, List<string> perms);
    Task RemoveUserPermsAsync(long userId);
    Task<List<long>?> GetUserMenuIdsAsync(long userId);
    Task SetUserMenuIdsAsync(long userId, List<long> menuIds);
    Task RemoveUserMenuIdsAsync(long userId);
}

public class RedisPermissionCache : IPermissionCache
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisPermissionCache> _logger;
    private static readonly TimeSpan TTL = TimeSpan.FromHours(8);

    public RedisPermissionCache(IDistributedCache cache,
        ILogger<RedisPermissionCache> logger)
    { _cache = cache; _logger = logger; }

    private string PermKey(long uid) => $"perm:{uid}";
    private string MenuKey(long uid) => $"menu:{uid}";

    public async Task<List<string>?> GetUserPermsAsync(long userId)
    {
        try
        {
            var json = await _cache.GetStringAsync(PermKey(userId));
            return json == null ? null : JsonSerializer.Deserialize<List<string>>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("读取权限缓存失败（将重新查库）：{Msg}", ex.Message);
            return null; // 缓存失败时返回 null，触发重新查库
        }
    }

    public async Task SetUserPermsAsync(long userId, List<string> perms)
    {
        try
        {
            await _cache.SetStringAsync(PermKey(userId),
                JsonSerializer.Serialize(perms),
                new DistributedCacheEntryOptions
                    { AbsoluteExpirationRelativeToNow = TTL });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("写入权限缓存失败（不影响功能）：{Msg}", ex.Message);
        }
    }

    public async Task RemoveUserPermsAsync(long userId)
    {
        try { await _cache.RemoveAsync(PermKey(userId)); }
        catch (Exception ex)
        { _logger.LogWarning("删除权限缓存失败：{Msg}", ex.Message); }
    }

    public async Task<List<long>?> GetUserMenuIdsAsync(long userId)
    {
        try
        {
            var json = await _cache.GetStringAsync(MenuKey(userId));
            return json == null ? null : JsonSerializer.Deserialize<List<long>>(json);
        }
        catch { return null; }
    }

    public async Task SetUserMenuIdsAsync(long userId, List<long> menuIds)
    {
        try
        {
            await _cache.SetStringAsync(MenuKey(userId),
                JsonSerializer.Serialize(menuIds),
                new DistributedCacheEntryOptions
                    { AbsoluteExpirationRelativeToNow = TTL });
        }
        catch (Exception ex)
        { _logger.LogWarning("写入菜单缓存失败：{Msg}", ex.Message); }
    }

    public async Task RemoveUserMenuIdsAsync(long userId)
    {
        try { await _cache.RemoveAsync(MenuKey(userId)); }
        catch (Exception ex)
        { _logger.LogWarning("删除菜单缓存失败：{Msg}", ex.Message); }
    }
}
