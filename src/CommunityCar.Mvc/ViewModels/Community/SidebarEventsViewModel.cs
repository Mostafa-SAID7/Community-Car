using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Community;

public class SidebarEventsViewModel
{
    public IEnumerable<EventDto> UpcomingEvents { get; set; } = new List<EventDto>();
    public IEnumerable<EventDto> PopularEvents { get; set; } = new List<EventDto>();
}
