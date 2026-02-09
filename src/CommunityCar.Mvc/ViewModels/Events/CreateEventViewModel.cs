using CommunityCar.Domain.Enums.Community.events;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Events;

public class CreateEventViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start time is required")]
    public DateTime StartTime { get; set; } = DateTime.Now.AddDays(1);

    [Required(ErrorMessage = "End time is required")]
    public DateTime EndTime { get; set; } = DateTime.Now.AddDays(1).AddHours(2);

    [Required(ErrorMessage = "Location is required")]
    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
    public string Location { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public EventCategory Category { get; set; }

    [Range(0, 10000, ErrorMessage = "Max attendees must be between 0 and 10000")]
    public int MaxAttendees { get; set; }

    public bool IsOnline { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? OnlineUrl { get; set; }

    public IFormFile? ImageFile { get; set; }
    
    public string? ImageUrl { get; set; }
}
