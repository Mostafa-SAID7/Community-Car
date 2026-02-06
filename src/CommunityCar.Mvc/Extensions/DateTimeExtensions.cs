using System;

namespace CommunityCar.Mvc.Extensions;

public static class DateTimeExtensions
{
    public static string ToTimeAgo(this DateTimeOffset dateTimeOffset)
    {
        var timeSpan = DateTimeOffset.Now.Subtract(dateTimeOffset);
        
        if (timeSpan <= TimeSpan.FromSeconds(60))
            return "just now";

        if (timeSpan <= TimeSpan.FromMinutes(60))
            return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "a minute ago";

        if (timeSpan <= TimeSpan.FromHours(24))
            return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "an hour ago";

        if (timeSpan <= TimeSpan.FromDays(30))
            return timeSpan.Days > 1 ? $"{timeSpan.Days} days ago" : "yesterday";

        if (timeSpan <= TimeSpan.FromDays(365))
            return timeSpan.Days > 30 ? $"{timeSpan.Days / 30} months ago" : "a month ago";

        return timeSpan.Days > 365 ? $"{timeSpan.Days / 365} years ago" : "a year ago";
    }

    public static string ToTimeAgo(this DateTime dateTime)
    {
        return ToTimeAgo((DateTimeOffset)dateTime);
    }
}
