using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Social;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.ViewComponents.Social;

public class RightSidebarSocialViewComponent : ViewComponent
{
    private readonly IFriendshipService _friendshipService;
    private readonly IGroupService _groupService;

    public RightSidebarSocialViewComponent(IFriendshipService friendshipService, IGroupService groupService)
    {
        _friendshipService = friendshipService;
        _groupService = groupService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarSocialViewModel
        {
            SuggestedFriends = new List<CommunityCar.Domain.DTOs.Identity.UserDto>(),
            PopularGroups = new List<CommunityCar.Domain.DTOs.Community.GroupDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
