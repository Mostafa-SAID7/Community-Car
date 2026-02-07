using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class UserActivityViewModel
{
    public ApplicationUser User { get; set; } = new();
    public IEnumerable<QuestionDto> RecentQuestions { get; set; } = new List<QuestionDto>();
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public int TotalVotes { get; set; }
}
