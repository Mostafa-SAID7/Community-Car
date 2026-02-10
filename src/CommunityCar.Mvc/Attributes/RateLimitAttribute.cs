using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace CommunityCar.Mvc.Attributes;

/// <summary>
/// Rate limiting attribute to prevent spam
/// Tracks requests per user within a time window
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private readonly int _maxRequests;
    private readonly int _timeWindowSeconds;
    private readonly string _action;

    public RateLimitAttribute(string action, int maxRequests = 5, int timeWindowSeconds = 60)
    {
        _action = action;
        _maxRequests = maxRequests;
        _timeWindowSeconds = timeWindowSeconds;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            await next();
            return;
        }

        var cacheKey = $"RateLimit_{_action}_{userId}";
        var requestCount = cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_timeWindowSeconds);
            return 0;
        });

        if (requestCount >= _maxRequests)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                message = $"Rate limit exceeded. Please wait {_timeWindowSeconds} seconds before trying again."
            })
            {
                StatusCode = 429 // Too Many Requests
            };
            return;
        }

        cache.Set(cacheKey, requestCount + 1, TimeSpan.FromSeconds(_timeWindowSeconds));
        await next();
    }
}
