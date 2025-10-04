using POS.Application.Common.Constants;
using POS.Application.Common.Exceptions;
using POS.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace POS.WebAPI.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        _logger.LogError(exception, "An unhandled exception occurred");

        var errorResponse = exception switch
        {
            Application.Common.Exceptions.ValidationException validationEx => new ErrorResponse
            {
                ErrorCode = validationEx.ErrorCode,
                Message = validationEx.Message,
                Errors = validationEx.Errors,
                StackTrace = _env.IsDevelopment() ? validationEx.StackTrace : null
            },

            Application.Common.Exceptions.AuthenticationException authEx => new ErrorResponse
            {
                ErrorCode = authEx.ErrorCode,
                Message = authEx.Message,
                StackTrace = _env.IsDevelopment() ? authEx.StackTrace : null
            },

            NotFoundException notFoundEx => new ErrorResponse
            {
                ErrorCode = notFoundEx.ErrorCode,
                Message = notFoundEx.Message,
                StackTrace = _env.IsDevelopment() ? notFoundEx.StackTrace : null
            },

            BusinessRuleException businessEx => new ErrorResponse
            {
                ErrorCode = businessEx.ErrorCode,
                Message = businessEx.Message,
                StackTrace = _env.IsDevelopment() ? businessEx.StackTrace : null
            },

            Application.Common.Exceptions.ApplicationException appEx => new ErrorResponse
            {
                ErrorCode = appEx.ErrorCode,
                Message = appEx.Message,
                StackTrace = _env.IsDevelopment() ? appEx.StackTrace : null
            },

            _ => new ErrorResponse
            {
                ErrorCode = ErrorCodes.SYS_INTERNAL_ERROR,
                Message = _env.IsDevelopment() ? exception.Message : ErrorMessages.InternalServerError,
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            }
        };

        var statusCode = exception switch
        {
            Application.Common.Exceptions.ApplicationException appEx => appEx.StatusCode,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.ErrorResponse(errorResponse);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
