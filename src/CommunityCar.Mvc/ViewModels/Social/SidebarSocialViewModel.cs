using CommunityCar.Domain.DTOs.Identity;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Social;

public class SidebarSocialViewModel
{
    public IEnumerable<UserDto> SuggestedFriends { get; set; } = new List<UserDto>();
    public IEnumerable<GroupDto> PopularGroups { get; set; } = new List<GroupDto>();
}
