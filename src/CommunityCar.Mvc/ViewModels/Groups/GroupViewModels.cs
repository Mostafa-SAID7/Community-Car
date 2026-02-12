using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Groups;

public class GroupViewModel
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public string? Slug { get; set; }
    public bool IsPrivate { get; set; }
    public Guid CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsMember { get; set; }
    public bool IsAdmin { get; set; }
}

public class CreateGroupViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Group Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Private Group")]
    public bool IsPrivate { get; set; }
}

public class EditGroupViewModel
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Group Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Private Group")]
    public bool IsPrivate { get; set; }
}

public class GroupMemberViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? UserSlug { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}
