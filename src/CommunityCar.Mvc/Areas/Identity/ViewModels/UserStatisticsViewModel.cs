using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class UserStatisticsViewModel
{
    public ApplicationUser User { get; set; } = new();
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public int TotalVotes { get; set; }
    public int TotalFriends { get; set; }
    public int ReputationPoints { get; set; }
    public int BadgesCount { get; set; }
    public DateTime MemberSince { get; set; }
}
