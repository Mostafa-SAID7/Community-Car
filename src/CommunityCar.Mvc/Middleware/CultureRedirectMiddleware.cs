using System.Globalization;
using System.Linq;

namespace CommunityCar.Web.Middleware;

public class CultureRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CultureRedirectMiddleware> _logger;
    private readonly string[] _supportedCultures = { "en", "ar" };
    private readonly string _defaultCulture = "en";

    public CultureRedirectMiddleware(RequestDelegate next, ILogger<CultureRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var lowerPath = path.ToLowerInvariant();

        // Skip static files, SignalR hubs, and special paths
        if (path.Contains(".") || 
            lowerPath.Contains("/_content/") ||
            lowerPath.Contains("/css/") || 
            lowerPath.Contains("/js/") || 
            lowerPath.Contains("/lib/") || 
            lowerPath.Contains("/images/") ||
            lowerPath.Contains("/uploads/") ||
            lowerPath.Contains("/favicon") ||
            lowerPath.Contains("/hub") ||
            lowerPath.Contains("/culture/setlanguage") ||
            lowerPath.Contains("/error/"))
        {
            await _next(context);
            return;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
        {
            // Check if response has already started
            if (!context.Response.HasStarted)
            {
                _logger.LogInformation("Redirecting empty path to default culture: {Culture}", _defaultCulture);
                context.Response.Redirect($"/{_defaultCulture.ToLowerInvariant()}/", permanent: false);
                return;
            }
            else
            {
                _logger.LogWarning("Cannot redirect - response already started");
                return;
            }
        }

        var firstSegment = segments[0].ToLowerInvariant();
        if (_supportedCultures.Contains(firstSegment))
        {
            await _next(context);
            return;
        }

        // Check if response has already started before redirecting
        if (!context.Response.HasStarted)
        {
            var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
            var newPath = $"/{_defaultCulture.ToLowerInvariant()}{path}{queryString}";
            _logger.LogInformation("Redirecting unlocalized path {Path} to {NewPath}", path, newPath);
            context.Response.Redirect(newPath, permanent: false);
            return;
        }
        else
        {
            _logger.LogWarning("Cannot redirect path {Path} - response already started", path);
            await _next(context);
        }
    }
}
