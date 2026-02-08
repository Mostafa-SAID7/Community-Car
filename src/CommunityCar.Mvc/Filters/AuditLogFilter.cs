using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CommunityCar.Web.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly ILogger<AuditLogFilter> _logger;

    public AuditLogFilter(ILogger<AuditLogFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Log before action execution
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userName = context.HttpContext.User.Identity?.Name ?? "Anonymous";
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation(
            "User {UserName} (ID: {UserId}) from IP {IpAddress} is executing {Method} {Path} - Controller: {Controller}, Action: {Action}",
            userName, userId, ipAddress, method, path, controller, action);

        // Execute the action
        var executedContext = await next();

        // Log after action execution
        if (executedContext.Exception == null)
        {
            var statusCode = context.HttpContext.Response.StatusCode;
            _logger.LogInformation(
                "User {UserName} (ID: {UserId}) completed {Method} {Path} with status code {StatusCode}",
                userName, userId, method, path, statusCode);
        }
        else
        {
            _logger.LogError(
                executedContext.Exception,
                "User {UserName} (ID: {UserId}) encountered an error executing {Method} {Path}",
                userName, userId, method, path);
        }
    }
}
