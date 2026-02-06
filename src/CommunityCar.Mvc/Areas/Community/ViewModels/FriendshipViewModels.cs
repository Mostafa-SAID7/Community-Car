using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.Enums.Community.friends;

namespace CommunityCar.Web.Areas.Community.ViewModels;

public class FriendshipViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid FriendId { get; set; }
    public string? Slug { get; set; }

    [Required]
    [StringLength(100)]
    public string FriendName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    [Required]
    public FriendshipStatus Status { get; set; }

    [Required]
    public DateTimeOffset Since { get; set; }
}

public class FriendRequestViewModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }
    public string? Slug { get; set; }

    [Required]
    public DateTimeOffset ReceivedAt { get; set; }
}
