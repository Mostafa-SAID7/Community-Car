using System;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Enums.Identity.Users;
using CommunityCar.Domain.Entities.Community.qa;

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
    
    // Additional profile fields
    public string? Location { get; set; }
    public string? Website { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int ReputationPoints { get; set; }
    
    public int Points { get; set; }
    
    public UserRank Rank => Points switch
    {
        >= 10000 => UserRank.Master,
        >= 8000 => UserRank.Moderator,
        >= 5000 => UserRank.Author,
        >= 3000 => UserRank.Reviewer,
        >= 1000 => UserRank.Expert,
        _ => UserRank.Standard
    };

    public bool IsExpert => Points >= 1000;
    public bool CanPostReviews => Rank >= UserRank.Reviewer;
    public bool CanPostNews => Rank >= UserRank.Author;
    public bool CanModerate => Rank >= UserRank.Moderator;
    public bool IsMaster => Rank == UserRank.Master;
    
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

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public virtual ICollection<QuestionBookmark> Bookmarks { get; set; } = new List<QuestionBookmark>();

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
