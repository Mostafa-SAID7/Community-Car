using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Community;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Community;

public class RightSidebarReviewsViewComponent : ViewComponent
{
    private readonly IReviewService _reviewService;

    public RightSidebarReviewsViewComponent(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarReviewsViewModel
        {
            RecentReviews = new List<CommunityCar.Domain.DTOs.Community.ReviewDto>(),
            TopReviews = new List<CommunityCar.Domain.DTOs.Community.ReviewDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
