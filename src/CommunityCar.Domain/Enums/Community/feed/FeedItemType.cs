using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Domain.Enums.Community.Feed;

public enum FeedItemType
{
    [Display(Name = "Post", Description = "Community post or update")]
    Post = 1,

    [Display(Name = "Question", Description = "Q&A question")]
    Question = 2,

    [Display(Name = "Event", Description = "Community event")]
    Event = 3,

    [Display(Name = "News", Description = "News article")]
    News = 4,

    [Display(Name = "Guide", Description = "Guide or tutorial")]
    Guide = 5,

    [Display(Name = "Review", Description = "Product or service review")]
    Review = 6,

    [Display(Name = "Group", Description = "Community group")]
    Group = 7
}
