using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Controllers.Gamification;

[Route("Badges")]
public class BadgesController : Controller
{
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
            new() { Name = "First Post", Description = "Create your first post", IconUrl = "fa-edit", Category = "Getting Started", IsEarned = true, Progress = 100 },
            new() { Name = "Helpful Member", Description = "Receive 10 helpful votes", IconUrl = "fa-thumbs-up", Category = "Community", IsEarned = true, Progress = 100 },
            new() { Name = "Conversation Starter", Description = "Start 5 discussions", IconUrl = "fa-comments", Category = "Engagement", IsEarned = false, Progress = 60 },
            new() { Name = "Expert Reviewer", Description = "Write 10 detailed reviews", IconUrl = "fa-star", Category = "Content", IsEarned = false, Progress = 40 },
            new() { Name = "Guide Master", Description = "Create 5 comprehensive guides", IconUrl = "fa-book", Category = "Content", IsEarned = false, Progress = 20 },
            new() { Name = "Social Butterfly", Description = "Connect with 20 friends", IconUrl = "fa-user-friends", Category = "Social", IsEarned = false, Progress = 75 },
            new() { Name = "Event Organizer", Description = "Host 3 community events", IconUrl = "fa-calendar", Category = "Events", IsEarned = false, Progress = 33 },
            new() { Name = "Top Contributor", Description = "Earn 1000 reputation points", IconUrl = "fa-trophy", Category = "Achievement", IsEarned = false, Progress = 45 },
            new() { Name = "Early Adopter", Description = "Join in the first month", IconUrl = "fa-rocket", Category = "Special", IsEarned = true, Progress = 100 },
            new() { Name = "Verified Expert", Description = "Get verified by moderators", IconUrl = "fa-check-circle", Category = "Special", IsEarned = false, Progress = 0 },
            new() { Name = "News Reporter", Description = "Share 10 news articles", IconUrl = "fa-newspaper", Category = "Content", IsEarned = false, Progress = 50 },
            new() { Name = "Problem Solver", Description = "Answer 20 questions", IconUrl = "fa-lightbulb", Category = "Community", IsEarned = false, Progress = 35 }
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
