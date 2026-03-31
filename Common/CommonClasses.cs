namespace EnterpriseMS.Common;

/// <summary>统一API响应，字段小写以匹配前端JS约定</summary>
public class ApiResult<T>
{
    public int    code    { get; set; } = 200;
    public string message { get; set; } = "操作成功";
    public T?     data    { get; set; }
    public bool   success => code == 200;

    public static ApiResult<T>      Ok(T data, string msg = "操作成功")
        => new() { data = data, message = msg };
    public static ApiResult<object> Fail(string msg, int code = 400)
        => new ApiResult<object> { code = code, message = msg };
    public static ApiResult<object> Forbidden()
        => new ApiResult<object> { code = 403, message = "无操作权限" };
    public static ApiResult<object> Unauthorized()
        => new ApiResult<object> { code = 401, message = "请先登录" };
}

public static class ApiResult
{
    public static ApiResult<object> Ok(string msg = "操作成功")
        => new() { message = msg };
    public static ApiResult<object> Fail(string msg, int code = 400)
        => new ApiResult<object> { code = code, message = msg };
}

public class PagedResult<T>
{
    public List<T> Items      { get; set; } = new();
    public int     Total      { get; set; }
    public int     Page       { get; set; } = 1;
    public int     PageSize   { get; set; } = 10;
    public int     TotalPages => PageSize > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;
    public bool    HasPrev    => Page > 1;
    public bool    HasNext    => Page < TotalPages;
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
