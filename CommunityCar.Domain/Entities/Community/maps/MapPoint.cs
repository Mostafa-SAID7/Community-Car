using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPoint : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = default!;
    public string Type { get; set; } = string.Empty; // e.g., "Park", "UserHome", "MeetingPoint"
    public Guid? OwnerId { get; set; }

    private MapPoint() { }

    public MapPoint(string name, Location location, string type, Guid? ownerId = null)
    {
        Name = name;
        Location = location;
        Type = type;
        OwnerId = ownerId;
    }
}
