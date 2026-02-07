using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Reviews;

public class ReviewDetailsViewModel
{
    public ReviewDto Review { get; set; } = null!;
    public PagedResult<ReviewCommentDto> Comments { get; set; } = null!;
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
