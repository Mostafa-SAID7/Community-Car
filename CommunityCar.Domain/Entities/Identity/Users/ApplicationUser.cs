using System;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Helpers;

namespace CommunityCar.Domain.Entities.Identity.Users;

/// <summary>
/// Represents a user within the personal community system.
/// Inherits from IdentityUser<Guid> for ASP.NET Core Identity integration.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IEntity, IAuditable, ISoftDelete
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
    
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Slug { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ICollection<CommunityCar.Domain.Entities.Community.friends.Friendship> SentFriendships { get; set; } = new List<CommunityCar.Domain.Entities.Community.friends.Friendship>();
    public virtual ICollection<CommunityCar.Domain.Entities.Community.friends.Friendship> ReceivedFriendships { get; set; } = new List<CommunityCar.Domain.Entities.Community.friends.Friendship>();

    // Domain logic examples
    public bool IsProfileComplete => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName);

    public void UpdateProfile(string firstName, string lastName, string? bio)
    {
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
        Slug = SlugHelper.GenerateSlug(FullName ?? "user");
    }
}
