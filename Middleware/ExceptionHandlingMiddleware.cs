
using System.Net;
using System.Text.Json;
using MiniBlogAPI.Exceptions;
using MiniBlogAPI.Models;

namespace MiniBlogAPI.middleware;

// Middleware để handle tất cả exceptions trong application.
// Catch exceptions, log chúng, và return standardized error responses.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }
    // Invoke middleware - wrap request trong try-catch.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call next middleware trong pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Catch ALL exceptions và handle
            await HandleExceptionAsync(context, ex);
        }
    }
    // Handle exception và return appropriate HTTP response.
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Đã xảy ra ngoại lệ chưa được xử lý.");
        // Set response content type
        context.Response.ContentType = "application/json";

        // create error response 
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
        switch (exception)
        {
            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundException.Message;
                _logger.LogWarning(notFoundException, "Resource not found: {Message}", notFoundException.Message);
                break;

            case UnauthorizedException unauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = unauthorizedException.Message;
                _logger.LogWarning(unauthorizedException, "Unauthorized access: {Message}", unauthorizedException.Message);
                break;

            case BadRequestException badRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = badRequestException.Message;
                _logger.LogWarning(badRequestException, "Bad request: {Message}", badRequestException.Message);
                break;

            case InvalidOperationException invalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = invalidOperationException.Message;
                _logger.LogWarning(invalidOperationException, "Invalid operation: {Message}", invalidOperationException.Message);
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorizedAccessException.Message;
                _logger.LogWarning(unauthorizedAccessException, "Unauthorized: {Message}", unauthorizedAccessException.Message);
                break;

            default:
                // Generic 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An internal server error occurred. Please try again later.";

                // Log full exception details
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }
        // Include exception details chỉ trong Development environment
        if (_env.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }
        // Serialize error response to JSON
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment() // Pretty print only in Development
        };
        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);

        // Write response
        await context.Response.WriteAsync(jsonResponse);
    }
}