using CoherentMobile.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace CoherentMobile.API.Middleware;

/// <summary>
/// Global error handling middleware to catch and format exceptions
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var response = new ErrorResponse
        {
            Success = false,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                code = HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.ErrorCode = "VALIDATION_ERROR";
                response.Errors = validationEx.Errors;
                break;

            case AuthenticationException authEx:
                code = HttpStatusCode.Unauthorized;
                response.Message = authEx.Message;
                response.ErrorCode = authEx.ErrorCode;
                break;

            case OTPException otpEx:
                code = HttpStatusCode.BadRequest;
                response.Message = otpEx.Message;
                response.ErrorCode = otpEx.ErrorCode;
                break;

            case DatabaseException dbEx:
                code = HttpStatusCode.InternalServerError;
                response.Message = "A database error occurred. Please try again later.";
                response.ErrorCode = dbEx.ErrorCode;
                response.Details = "Contact support if the problem persists";
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                response.ErrorCode = "UNAUTHORIZED";
                break;

            case ArgumentException argEx:
                code = HttpStatusCode.BadRequest;
                response.Message = argEx.Message;
                response.ErrorCode = "INVALID_ARGUMENT";
                break;

            case InvalidOperationException invalidOpEx:
                code = HttpStatusCode.BadRequest;
                response.Message = invalidOpEx.Message;
                response.ErrorCode = "INVALID_OPERATION";
                break;

            case KeyNotFoundException notFoundEx:
                code = HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                response.ErrorCode = "NOT_FOUND";
                break;

            default:
                code = HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred. Please try again later.";
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                response.Details = "Contact support if the problem persists";
                break;
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }

    private class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string? Details { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
