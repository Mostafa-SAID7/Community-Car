using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Events;

public class EventDetailsViewModel
{
    public EventDto Event { get; set; } = null!;
    public PagedResult<EventAttendeeDto> Attendees { get; set; } = null!;
    public PagedResult<EventCommentDto> Comments { get; set; } = null!;
    public List<EventDto> RelatedEvents { get; set; } = new();
}
