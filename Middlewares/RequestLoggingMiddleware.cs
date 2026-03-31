namespace EnterpriseMS.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(ctx);
            sw.Stop();
            _logger.LogInformation("{Method} {Path} {StatusCode} {Elapsed}ms",
                ctx.Request.Method, ctx.Request.Path,
                ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "{Method} {Path} 异常 {Elapsed}ms",
                ctx.Request.Method, ctx.Request.Path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
