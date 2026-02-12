using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Domain.Enums.Community.Feed;

public enum DateFilterType
{
    [Display(Name = "All Time", Description = "All content")]
    All = 0,

    [Display(Name = "Today", Description = "Content from today")]
    Today = 1,

    [Display(Name = "This Week", Description = "Content from this week")]
    ThisWeek = 2,

    [Display(Name = "This Month", Description = "Content from this month")]
    ThisMonth = 3,

    [Display(Name = "Last 24 Hours", Description = "Content from last 24 hours")]
    Last24Hours = 4,

    [Display(Name = "Last 7 Days", Description = "Content from last 7 days")]
    Last7Days = 5,

    [Display(Name = "Last 30 Days", Description = "Content from last 30 days")]
    Last30Days = 6
}
