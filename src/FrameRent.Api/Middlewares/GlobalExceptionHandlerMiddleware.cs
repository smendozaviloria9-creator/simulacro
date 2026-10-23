using FrameRent.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        // Order matters: specific subclasses first, then the DomainException base.
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Resource Not Found", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Business rule violation: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Business Rule Violation", ex.Message);
        }
        catch (Exception ex)
        {
            // Full details go to the server log only; the client gets a generic message
            // so internal information (SQL, connection strings, stack traces) is never leaked.
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An internal server error occurred",
                $"An unexpected error occurred. Reference: {context.TraceIdentifier}");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
