using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Enums.Community.friends;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = new();
    public bool IsOwnProfile { get; set; }
    public FriendshipStatus FriendshipStatus { get; set; }
    public int QuestionsCount { get; set; }
    public int FriendsCount { get; set; }
    public IEnumerable<QuestionDto> RecentQuestions { get; set; } = new List<QuestionDto>();
    public DateTime JoinedDate { get; set; }
}
