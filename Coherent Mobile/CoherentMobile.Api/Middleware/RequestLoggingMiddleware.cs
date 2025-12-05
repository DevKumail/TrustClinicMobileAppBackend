using System.Diagnostics;

namespace CoherentMobile.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with timing
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation("Incoming {Method} request to {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Completed {Method} request to {Path} with status {StatusCode} in {ElapsedMs}ms",
                requestMethod, requestPath, statusCode, elapsedMs);
        }
    }
}
