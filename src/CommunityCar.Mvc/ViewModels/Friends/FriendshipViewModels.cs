using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.Enums.Community.friends;

namespace CommunityCar.Mvc.ViewModels.Friends;

public class FriendViewModel
{
    public Guid Id { get; set; }
    public Guid FriendId { get; set; }
    public string? Slug { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTimeOffset Since { get; set; }
}

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

public class UserSearchViewModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }
    
    [Required]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public FriendshipStatus FriendshipStatus { get; set; }
}

public class BlockedUserViewModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }
    public string? Slug { get; set; }

    [Required]
    public DateTimeOffset BlockedAt { get; set; }
}
