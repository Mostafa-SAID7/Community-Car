using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.maps;

public class Location : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }
    public string? Address { get; }

    public Location(double latitude, double longitude, string? address = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
        yield return Address;
    }
}
