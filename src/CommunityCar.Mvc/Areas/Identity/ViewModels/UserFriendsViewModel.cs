using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class UserFriendsViewModel
{
    public ApplicationUser User { get; set; } = new();
    public List<FriendshipDto> Friends { get; set; } = new();
    public bool IsOwnProfile { get; set; }
}
