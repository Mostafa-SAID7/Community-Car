using System.Globalization;
using System.Linq;

namespace CommunityCar.Web.Middleware;

public class CultureRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _supportedCultures = { "en", "ar" };
    private readonly string _defaultCulture = "en";

    public CultureRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var lowerPath = path.ToLowerInvariant();

        // Skip static files, SignalR hubs, and special paths
        // Robust check: if it has an extension (contains a dot), it's a static file
        if (path.Contains(".") || 
            lowerPath.Contains("/_content/") ||
            lowerPath.Contains("/css/") || 
            lowerPath.Contains("/js/") || 
            lowerPath.Contains("/lib/") || 
            lowerPath.Contains("/images/") ||
            lowerPath.Contains("/uploads/") ||
            lowerPath.Contains("/favicon") ||
            lowerPath.StartsWith("/questionhub") ||
            lowerPath.StartsWith("/notificationhub") ||
            lowerPath.StartsWith("/chathub") ||
            lowerPath.StartsWith("/friendhub") ||
            lowerPath.StartsWith("/culture/setlanguage") ||
            lowerPath.StartsWith("/error/"))
        {
            await _next(context);
            return;
        }

        // Check if path starts with a culture code
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
        {
            // Root path - redirect to default culture
            context.Response.Redirect($"/{_defaultCulture.ToLowerInvariant()}/");
            return;
        }

        var firstSegment = segments[0].ToLowerInvariant();
        
        // If first segment is a supported culture, continue
        if (_supportedCultures.Contains(firstSegment))
        {
            await _next(context);
            return;
        }

        // If first segment is NOT a culture, prepend default culture
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var newPath = $"/{_defaultCulture.ToLowerInvariant()}{path}{queryString}";
        
        context.Response.Redirect(newPath);
    }
}
