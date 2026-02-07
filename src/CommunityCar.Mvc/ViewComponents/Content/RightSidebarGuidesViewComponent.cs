using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Content;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Content;

public class RightSidebarGuidesViewComponent : ViewComponent
{
    private readonly IGuideService _guideService;

    public RightSidebarGuidesViewComponent(IGuideService guideService)
    {
        _guideService = guideService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarGuidesViewModel
        {
            PopularGuides = new List<CommunityCar.Domain.DTOs.Community.GuideDto>(),
            RecentGuides = new List<CommunityCar.Domain.DTOs.Community.GuideDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
