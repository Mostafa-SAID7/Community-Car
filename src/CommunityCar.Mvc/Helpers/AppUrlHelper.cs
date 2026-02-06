using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Helpers;

public static class AppUrlHelper
{
    public static string? Content(this IUrlHelper urlHelper, string contentPath)
    {
        return urlHelper.Content(contentPath);
    }

    public static bool IsLocalUrl(this IUrlHelper urlHelper, string? url)
    {
        return urlHelper.IsLocalUrl(url);
    }

    public static string? ActionWithSlug(this IUrlHelper urlHelper, string action, string controller, string slug, object? values = null)
    {
        var routeValues = values != null ? new Microsoft.AspNetCore.Routing.RouteValueDictionary(values) : new Microsoft.AspNetCore.Routing.RouteValueDictionary();
        routeValues["slug"] = slug;
        return urlHelper.Action(action, controller, routeValues);
    }
}
