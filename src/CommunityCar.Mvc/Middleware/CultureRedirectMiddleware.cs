using System.Globalization;

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

        // Skip static files, SignalR hubs, and special paths
        if (path.StartsWith("/css/") || 
            path.StartsWith("/js/") || 
            path.StartsWith("/lib/") || 
            path.StartsWith("/images/") ||
            path.StartsWith("/uploads/") ||
            path.StartsWith("/fonts/") ||
            path.StartsWith("/favicon") ||
            path.StartsWith("/questionHub") ||
            path.StartsWith("/notificationHub") ||
            path.StartsWith("/chatHub") ||
            path.StartsWith("/friendHub") ||
            path.StartsWith("/Culture/SetLanguage") ||
            path.StartsWith("/Error/"))
        {
            await _next(context);
            return;
        }

        // Check if path starts with a culture code
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
        {
            // Root path - redirect to default culture
            context.Response.Redirect($"/{_defaultCulture}");
            return;
        }

        var firstSegment = segments[0];
        
        // If first segment is a supported culture, continue
        if (_supportedCultures.Contains(firstSegment.ToLower()))
        {
            await _next(context);
            return;
        }

        // If first segment is NOT a culture, prepend default culture
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var newPath = $"/{_defaultCulture}{path}{queryString}";
        context.Response.Redirect(newPath);
    }
}
