using System.Net;
using System.Text.Json;
using ExpenseTracker.Domain.Exceptions;

namespace ExpenseTracker.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Validation Error", validationEx.Message, validationEx.Errors)
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse("Not Found", notFoundEx.Message)
            ),
            UnauthorizedException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", "You are not authorized to access this resource")
            ),
            ForbiddenException => (
                HttpStatusCode.Forbidden,
                new ErrorResponse("Forbidden", "You do not have permission to perform this action")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("Server Error", "An unexpected error occurred")
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public record ErrorResponse(
    string Title,
    string Message,
    IDictionary<string, string[]>? Errors = null);
