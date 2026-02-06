using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Identity.Profiles;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? SocialLinksJson { get; set; }
}
