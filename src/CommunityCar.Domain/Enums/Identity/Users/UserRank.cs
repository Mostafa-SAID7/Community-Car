using System.ComponentModel;

namespace CommunityCar.Domain.Enums.Identity.Users;

public enum UserRank
{
    [Description("Standard")]
    Standard = 0,

    [Description("Expert")]
    Expert = 1,

    [Description("Reviewer")]
    Reviewer = 2,

    [Description("Author")]
    Author = 3,

    [Description("Moderator")]
    Moderator = 4,

    [Description("Master")]
    Master = 5
}
