using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Location;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Location;

public class RightSidebarMapsViewComponent : ViewComponent
{
    private readonly IMapService _mapService;

    public RightSidebarMapsViewComponent(IMapService mapService)
    {
        _mapService = mapService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarMapsViewModel
        {
            RecentPoints = new List<CommunityCar.Domain.DTOs.Community.MapPointDto>(),
            TopRatedPoints = new List<CommunityCar.Domain.DTOs.Community.MapPointDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
