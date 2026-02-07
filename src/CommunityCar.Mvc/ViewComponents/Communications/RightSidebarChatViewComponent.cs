using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Mvc.ViewModels.Communications;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.ViewComponents.Communications;

public class RightSidebarChatViewComponent : ViewComponent
{
    private readonly IChatService _chatService;

    public RightSidebarChatViewComponent(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarChatViewModel
        {
            ActiveChats = new List<CommunityCar.Domain.DTOs.Communications.ChatRoomDto>(),
            OnlineFriends = 0
        };

        return await Task.FromResult(View(viewModel));
    }
}
