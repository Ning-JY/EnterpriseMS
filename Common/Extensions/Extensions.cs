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
        => user.FindFirstValue(ClaimTypes.Name) ?? "";
    public static string GetUsername(this ClaimsPrincipal user)
        => user.FindFirstValue("Username") ?? "";
}

public static class HttpRequestExtensions
{
    public static bool IsAjaxRequest(this HttpRequest req)
        => req.Headers["X-Requested-With"] == "XMLHttpRequest";
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
