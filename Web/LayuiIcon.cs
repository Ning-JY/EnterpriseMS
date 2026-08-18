namespace EnterpriseMS.Web;

/// <summary>
/// 菜单图标映射：数据库 SysMenu.Icon 存的是 FontAwesome 名（历史遗留，如 "fa-users"），
/// 前端已切换到 layui 原生图标体系，这里做一次集中翻译，避免改动菜单表数据。
/// 未收录的图标统一回退到 layui-icon-circle-dot。
/// </summary>
public static class LayuiIcon
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fa-book"]              = "layui-icon-read",
        ["fa-bug"]               = "layui-icon-console",
        ["fa-bullhorn"]          = "layui-icon-speaker",
        ["fa-calculator"]        = "layui-icon-util",
        ["fa-certificate"]       = "layui-icon-diamond",
        ["fa-chart-bar"]         = "layui-icon-chart",
        ["fa-clipboard-list"]    = "layui-icon-form",
        ["fa-cog"]               = "layui-icon-set",
        ["fa-cogs"]              = "layui-icon-set-fill",
        ["fa-coins"]             = "layui-icon-rmb",
        ["fa-database"]          = "layui-icon-table",
        ["fa-file-contract"]     = "layui-icon-file",
        ["fa-file-signature"]    = "layui-icon-note",
        ["fa-file-word"]         = "layui-icon-file-b",
        ["fa-folder-open"]       = "layui-icon-file",
        ["fa-hand-holding-usd"]  = "layui-icon-dollar",
        ["fa-history"]           = "layui-icon-log",
        ["fa-id-card"]           = "layui-icon-username",
        ["fa-layer-group"]       = "layui-icon-layouts",
        ["fa-project-diagram"]   = "layui-icon-template-1",
        ["fa-sitemap"]           = "layui-icon-tree",
        ["fa-sliders-h"]         = "layui-icon-slider",
        ["fa-tags"]              = "layui-icon-flag",
        ["fa-th-list"]           = "layui-icon-list",
        ["fa-user"]              = "layui-icon-user",
        ["fa-user-chart"]        = "layui-icon-chart-screen",
        ["fa-user-circle"]       = "layui-icon-username",
        ["fa-user-tag"]          = "layui-icon-auz",
        ["fa-users"]             = "layui-icon-group",
    };

    private const string Fallback = "layui-icon-circle-dot";

    /// <summary>把 FontAwesome 图标名翻译为 layui 图标类名。</summary>
    public static string From(string? faIcon)
    {
        if (string.IsNullOrWhiteSpace(faIcon)) return Fallback;

        // 已经是 layui 图标则原样返回，方便后续新菜单直接写 layui 名
        if (faIcon.StartsWith("layui-icon-", StringComparison.OrdinalIgnoreCase)) return faIcon;

        return Map.TryGetValue(faIcon.Trim(), out var layuiIcon) ? layuiIcon : Fallback;
    }
}
