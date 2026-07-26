using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EnterpriseMS.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var val = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(val, out var id) ? id : 0;
    }
    public static string GetRealName(this ClaimsPrincipal user)
        => WebUtility.HtmlDecode(user.FindFirstValue(ClaimTypes.Name) ?? "");
    public static string GetUsername(this ClaimsPrincipal user)
        => user.FindFirstValue("Username") ?? "";
}

public static class HttpRequestExtensions
{
    public static bool IsAjaxRequest(this HttpRequest req)
        => req.Headers["X-Requested-With"] == "XMLHttpRequest";
}

/// <summary>
/// 树形构建扩展（审计 3.1：原 DeptService / MenuService 各有一份逐字重复的 BuildTree）。
/// 通过委托读取 Id / ParentId 并写入 Children，泛型复用，避免复制递归逻辑。
/// </summary>
public static class TreeExtensions
{
    public static List<T> BuildTree<T>(this List<T> all, long parentId,
        Func<T, long> getId, Func<T, long> getParentId, Action<T, List<T>> setChildren)
        => all.Where(d => getParentId(d) == parentId)
              .Select(d =>
              {
                  setChildren(d, all.BuildTree(getId(d), getId, getParentId, setChildren));
                  return d;
              })
              .ToList();
}

public static class StringExtensions
{
    public static string MaskPhone(this string? phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 7) return phone ?? "";
        return phone[..3] + "****" + phone[^4..];
    }
    public static string MaskIdCard(this string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 10) return id ?? "";
        return id[..4] + "**********" + id[^4..];
    }
}
