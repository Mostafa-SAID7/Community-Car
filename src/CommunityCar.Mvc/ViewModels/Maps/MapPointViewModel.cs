using CommunityCar.Domain.Enums.Community.maps;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Maps;

public class CreateMapPointViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Type is required")]
    public MapPointType Type { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? Website { get; set; }

    public string? ImageUrl { get; set; }

    public string? Tags { get; set; }
}

public class EditMapPointViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Type is required")]
    public MapPointType Type { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? Website { get; set; }

    public string? ImageUrl { get; set; }

    public string? Tags { get; set; }
}

public class MapPointDetailsViewModel
{
    public Domain.DTOs.Community.MapPointDto MapPoint { get; set; } = null!;
    public Domain.Base.PagedResult<Domain.DTOs.Community.MapPointCommentDto> Comments { get; set; } = null!;
    public List<Domain.DTOs.Community.MapPointDto> NearbyPoints { get; set; } = new();
}

public class MapSearchViewModel
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double RadiusKm { get; set; } = 10;
    public MapPointType? Type { get; set; }
    public string? SearchTerm { get; set; }
}
