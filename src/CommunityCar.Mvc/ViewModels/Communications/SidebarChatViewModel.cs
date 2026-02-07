using CommunityCar.Domain.DTOs.Communications;

namespace CommunityCar.Mvc.ViewModels.Communications;

public class SidebarChatViewModel
{
    public IEnumerable<ChatRoomDto> ActiveChats { get; set; } = new List<ChatRoomDto>();
    public int OnlineFriends { get; set; }
}
