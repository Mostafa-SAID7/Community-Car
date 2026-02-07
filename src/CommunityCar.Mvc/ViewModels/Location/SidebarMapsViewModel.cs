using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Location;

public class SidebarMapsViewModel
{
    public IEnumerable<MapPointDto> RecentPoints { get; set; } = new List<MapPointDto>();
    public IEnumerable<MapPointDto> TopRatedPoints { get; set; } = new List<MapPointDto>();
}
