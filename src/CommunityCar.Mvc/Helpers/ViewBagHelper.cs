using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CommunityCar.Web.Helpers;

public static class ViewBagHelper
{
    public static T? GetValue<T>(this ViewDataDictionary viewData, string key)
    {
        if (viewData.TryGetValue(key, out var value) && value is T result)
        {
            return result;
        }
        return default;
    }

    public static void SetValue<T>(this ViewDataDictionary viewData, string key, T value)
    {
        viewData[key] = value;
    }

    public static string GetPageTitle(this ViewDataDictionary viewData)
    {
        return viewData["Title"]?.ToString() ?? "Community Car";
    }

    public static void SetPageTitle(this ViewDataDictionary viewData, string title)
    {
        viewData["Title"] = title;
    }
}
