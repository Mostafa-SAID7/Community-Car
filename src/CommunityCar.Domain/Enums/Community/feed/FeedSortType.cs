using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Domain.Enums.Community.Feed;

public enum FeedSortType
{
    [Display(Name = "Recent", Description = "Most recent first")]
    Recent = 1,

    [Display(Name = "Popular", Description = "Most popular")]
    Popular = 2,

    [Display(Name = "Trending", Description = "Currently trending")]
    Trending = 3,

    [Display(Name = "Most Viewed", Description = "Most viewed content")]
    MostViewed = 4,

    [Display(Name = "Most Liked", Description = "Most liked content")]
    MostLiked = 5,

    [Display(Name = "Most Discussed", Description = "Most commented content")]
    MostDiscussed = 6,

    [Display(Name = "Relevant", Description = "Most relevant to you")]
    Relevant = 7
}
