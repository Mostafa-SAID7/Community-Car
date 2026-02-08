using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Mvc.Controllers.Gamification;

[Route("{culture:alpha}/Badges")]
public class BadgesController : Controller
{
    private readonly IStringLocalizer<BadgesController> _localizer;

    public BadgesController(IStringLocalizer<BadgesController> localizer)
    {
        _localizer = localizer;
    }

    // GET: Badges
    [HttpGet("")]
    public IActionResult Index()
    {
        // Mock data for now - replace with actual service call
        var badges = GetMockBadges();
        return View(badges);
    }
    
    // GET: Badges/MyBadges
    [HttpGet("MyBadges")]
    public IActionResult MyBadges()
    {
        // Mock data for now - replace with actual service call
        // In a real scenario, this would filter for badges earned by the current user
        var badges = GetMockBadges().Where(b => b.IsEarned).ToList();
        return View(badges);
    }

    private List<BadgeViewModel> GetMockBadges()
    {
        return new List<BadgeViewModel>
        {
            new() { Name = _localizer["FirstPostName"], Description = _localizer["FirstPostDesc"], IconUrl = "fa-edit", Category = _localizer["GettingStartedCat"], IsEarned = true, Progress = 100 },
            new() { Name = _localizer["HelpfulMemberName"], Description = _localizer["HelpfulMemberDesc"], IconUrl = "fa-thumbs-up", Category = _localizer["CommunityCat"], IsEarned = true, Progress = 100 },
            new() { Name = _localizer["ConversationStarterName"], Description = _localizer["ConversationStarterDesc"], IconUrl = "fa-comments", Category = _localizer["EngagementCat"], IsEarned = false, Progress = 60 },
            new() { Name = _localizer["ExpertReviewerName"], Description = _localizer["ExpertReviewerDesc"], IconUrl = "fa-star", Category = _localizer["ContentCat"], IsEarned = false, Progress = 40 },
            new() { Name = _localizer["GuideMasterName"], Description = _localizer["GuideMasterDesc"], IconUrl = "fa-book", Category = _localizer["ContentCat"], IsEarned = false, Progress = 20 },
            new() { Name = _localizer["SocialButterflyName"], Description = _localizer["SocialButterflyDesc"], IconUrl = "fa-user-friends", Category = _localizer["SocialCat"], IsEarned = false, Progress = 75 },
            new() { Name = _localizer["EventOrganizerName"], Description = _localizer["EventOrganizerDesc"], IconUrl = "fa-calendar", Category = _localizer["EventsCat"], IsEarned = false, Progress = 33 },
            new() { Name = _localizer["TopContributorName"], Description = _localizer["TopContributorDesc"], IconUrl = "fa-trophy", Category = _localizer["AchievementCat"], IsEarned = false, Progress = 45 },
            new() { Name = _localizer["EarlyAdopterName"], Description = _localizer["EarlyAdopterDesc"], IconUrl = "fa-rocket", Category = _localizer["SpecialCat"], IsEarned = true, Progress = 100 },
            new() { Name = _localizer["VerifiedExpertName"], Description = _localizer["VerifiedExpertDesc"], IconUrl = "fa-check-circle", Category = _localizer["SpecialCat"], IsEarned = false, Progress = 0 },
            new() { Name = _localizer["NewsReporterName"], Description = _localizer["NewsReporterDesc"], IconUrl = "fa-newspaper", Category = _localizer["ContentCat"], IsEarned = false, Progress = 50 },
            new() { Name = _localizer["ProblemSolverName"], Description = _localizer["ProblemSolverDesc"], IconUrl = "fa-lightbulb", Category = _localizer["CommunityCat"], IsEarned = false, Progress = 35 }
        };
    }
}

public class BadgeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEarned { get; set; }
    public int Progress { get; set; }
}
