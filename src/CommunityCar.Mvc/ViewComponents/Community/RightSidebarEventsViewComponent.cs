using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Community;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Community;

public class RightSidebarEventsViewComponent : ViewComponent
{
    private readonly IEventService _eventService;

    public RightSidebarEventsViewComponent(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarEventsViewModel
        {
            UpcomingEvents = new List<CommunityCar.Domain.DTOs.Community.EventDto>(),
            PopularEvents = new List<CommunityCar.Domain.DTOs.Community.EventDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
