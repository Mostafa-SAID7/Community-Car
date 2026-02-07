using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Content;

public class SidebarGuidesViewModel
{
    public IEnumerable<GuideDto> PopularGuides { get; set; } = new List<GuideDto>();
    public IEnumerable<GuideDto> RecentGuides { get; set; } = new List<GuideDto>();
}
