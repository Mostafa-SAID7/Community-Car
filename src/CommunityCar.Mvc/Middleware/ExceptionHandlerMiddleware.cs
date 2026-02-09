using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using CommunityCar.Domain.Exceptions;

namespace CommunityCar.Web.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var originalPath = context.Request.Path.ToString();

        context.Response.StatusCode = statusCode;
        context.Items["OriginalPath"] = originalPath;

        // Check if the request expects JSON (API call)
        var isApiRequest = context.Request.Path.StartsWithSegments("/api") || 
                          context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isApiRequest)
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                StatusCode = statusCode,
                ErrorCode = GetErrorCode(exception),
                Message = GetErrorMessage(exception),
                Detailed = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
                    ? exception.Message 
                    : null,
                Errors = exception is CommunityCar.Domain.Exceptions.ValidationException validationException 
                    ? validationException.Errors 
                    : null
            });

            await context.Response.WriteAsync(result);
        }
        else
        {
            // Set the exception in features so ErrorController can find it
            context.Features.Set<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>(new CustomExceptionHandlerFeature
            {
                Error = exception
            });

            // Re-execute for MVC requests to preserve exception details
            context.Items["Exception"] = exception;
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Request.Path = $"/Error/{statusCode}";
            
            // Re-invoke the pipeline
            await _next(context);
        }
    }

    private class CustomExceptionHandlerFeature : Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature
    {
        public Exception Error { get; set; } = null!;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        NotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedException => StatusCodes.Status401Unauthorized,
        ForbiddenException => StatusCodes.Status403Forbidden,
        CommunityCar.Domain.Exceptions.ValidationException => StatusCodes.Status400BadRequest,
        ConflictException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetErrorMessage(Exception exception) => exception switch
    {
        NotFoundException => "The requested resource was not found.",
        UnauthorizedException => "Authentication is required to access this resource.",
        ForbiddenException => "You do not have permission to access this resource.",
        CommunityCar.Domain.Exceptions.ValidationException => "One or more validation errors occurred.",
        ConflictException => "The request conflicts with the current state of the resource.",
        _ => "An internal server error occurred."
    };

    private static string GetErrorCode(Exception exception) => exception switch
    {
        NotFoundException => CommunityCar.Domain.Constants.ErrorCodes.NOT_FOUND,
        InvalidCredentialsException => CommunityCar.Domain.Constants.ErrorCodes.INVALID_CREDENTIALS,
        TokenExpiredException => CommunityCar.Domain.Constants.ErrorCodes.TOKEN_EXPIRED,
        UnauthorizedException => CommunityCar.Domain.Constants.ErrorCodes.UNAUTHORIZED,
        ForbiddenException => CommunityCar.Domain.Constants.ErrorCodes.FORBIDDEN,
        CommunityCar.Domain.Exceptions.ValidationException => CommunityCar.Domain.Constants.ErrorCodes.VALIDATION_ERROR,
        ConflictException => CommunityCar.Domain.Constants.ErrorCodes.DUPLICATE_ENTRY,
        _ => CommunityCar.Domain.Constants.ErrorCodes.INTERNAL_ERROR
    };
}
