using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CommunityCar.Web.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing the request
        var stopwatch = Stopwatch.StartNew();

        // Get correlation ID if available
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        // Log request details
        await LogRequest(context, correlationId);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Create a new memory stream to capture the response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Execute the next middleware
            await _next(context);

            // Stop timing
            stopwatch.Stop();

            // Log response details
            await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request failed - Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}, Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string correlationId)
    {
        var request = context.Request;

        var requestLog = new StringBuilder();
        requestLog.AppendLine($"HTTP Request Information:");
        requestLog.AppendLine($"  Method: {request.Method}");
        requestLog.AppendLine($"  Path: {request.Path}");
        requestLog.AppendLine($"  QueryString: {request.QueryString}");
        requestLog.AppendLine($"  CorrelationId: {correlationId}");
        requestLog.AppendLine($"  User: {context.User?.Identity?.Name ?? "Anonymous"}");
        requestLog.AppendLine($"  IP Address: {context.Connection.RemoteIpAddress}");
        requestLog.AppendLine($"  User Agent: {request.Headers["User-Agent"].FirstOrDefault()}");

        // Log request body for POST/PUT/PATCH (be careful with sensitive data)
        if (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH")
        {
            if (request.ContentLength.HasValue && request.ContentLength > 0)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyAsText = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;

                // Don't log sensitive data (passwords, tokens, etc.)
                if (!request.Path.ToString().Contains("login", StringComparison.OrdinalIgnoreCase) &&
                    !request.Path.ToString().Contains("password", StringComparison.OrdinalIgnoreCase))
                {
                    requestLog.AppendLine($"  Body: {bodyAsText}");
                }
                else
                {
                    requestLog.AppendLine($"  Body: [REDACTED - Sensitive Data]");
                }
            }
        }

        _logger.LogInformation(requestLog.ToString());
    }

    private async Task LogResponse(HttpContext context, string correlationId, long durationMs)
    {
        var response = context.Response;

        var responseLog = new StringBuilder();
        responseLog.AppendLine($"HTTP Response Information:");
        responseLog.AppendLine($"  StatusCode: {response.StatusCode}");
        responseLog.AppendLine($"  ContentType: {response.ContentType}");
        responseLog.AppendLine($"  CorrelationId: {correlationId}");
        responseLog.AppendLine($"  Duration: {durationMs}ms");

        // Log slow requests as warnings
        if (durationMs > 3000) // More than 3 seconds
        {
            _logger.LogWarning(
                "Slow request detected - Method: {Method}, Path: {Path}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                durationMs,
                correlationId);
        }

        _logger.LogInformation(responseLog.ToString());
    }
}

// Extension method for easy registration
public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }
}
