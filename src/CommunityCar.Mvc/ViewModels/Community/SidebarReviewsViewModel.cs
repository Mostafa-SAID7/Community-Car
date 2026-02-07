using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Community;

public class SidebarReviewsViewModel
{
    public IEnumerable<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
    public IEnumerable<ReviewDto> TopReviews { get; set; } = new List<ReviewDto>();
}
