using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.analytics;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Analytics/UserActivity")]
public class UserAnalyticsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQuestionService _questionService;
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<UserAnalyticsController> _logger;

    public UserAnalyticsController(
        UserManager<ApplicationUser> userManager,
        IQuestionService questionService,
        IPostService postService,
        IEventService eventService,
        IReviewService reviewService,
        ILogger<UserAnalyticsController> logger)
    {
        _userManager = userManager;
        _questionService = questionService;
        _postService = postService;
        _eventService = eventService;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string period = "30days")
    {
        try
        {
            var viewModel = new UserAnalyticsViewModel
            {
                Period = period
            };

            // Get all users
            var allUsers = _userManager.Users.Where(u => !u.IsDeleted).ToList();
            viewModel.TotalUsers = allUsers.Count;

            // Calculate date range based on period
            var startDate = period switch
            {
                "7days" => DateTimeOffset.UtcNow.AddDays(-7),
                "30days" => DateTimeOffset.UtcNow.AddDays(-30),
                "90days" => DateTimeOffset.UtcNow.AddDays(-90),
                "1year" => DateTimeOffset.UtcNow.AddYears(-1),
                _ => DateTimeOffset.UtcNow.AddDays(-30)
            };

            // New users in period
            viewModel.NewUsers = allUsers.Count(u => u.CreatedAt >= startDate);

            // Active users (users who created content in period)
            var activeUserIds = new HashSet<Guid>();
            
            var questions = await _questionService.GetQuestionsAsync(new QueryParameters { PageNumber = 1, PageSize = 10000 });
            foreach (var q in questions.Items.Where(q => q.CreatedAt >= startDate))
            {
                activeUserIds.Add(q.AuthorId);
            }

            var posts = await _postService.GetPostsAsync(new QueryParameters { PageNumber = 1, PageSize = 10000 });
            foreach (var p in posts.Items.Where(p => p.CreatedAt >= startDate))
            {
                activeUserIds.Add(p.AuthorId);
            }

            viewModel.ActiveUsers = activeUserIds.Count;

            // User growth data for chart (last 12 months)
            var monthlyData = new Dictionary<string, int>();
            for (int i = 11; i >= 0; i--)
            {
                var monthStart = DateTimeOffset.UtcNow.AddMonths(-i).Date;
                var monthEnd = monthStart.AddMonths(1);
                var monthName = monthStart.ToString("MMM yyyy");
                var count = allUsers.Count(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                monthlyData[monthName] = count;
            }
            viewModel.UserGrowthData = monthlyData;

            // Top contributors
            var userContributions = new Dictionary<Guid, (string Name, int Count)>();
            
            foreach (var q in questions.Items)
            {
                if (!userContributions.ContainsKey(q.AuthorId))
                {
                    userContributions[q.AuthorId] = (q.AuthorName, 0);
                }
                userContributions[q.AuthorId] = (userContributions[q.AuthorId].Name, userContributions[q.AuthorId].Count + 1);
            }

            foreach (var p in posts.Items)
            {
                if (!userContributions.ContainsKey(p.AuthorId))
                {
                    userContributions[p.AuthorId] = (p.AuthorName, 0);
                }
                userContributions[p.AuthorId] = (userContributions[p.AuthorId].Name, userContributions[p.AuthorId].Count + 1);
            }

            viewModel.TopContributors = userContributions
                .OrderByDescending(x => x.Value.Count)
                .Take(10)
                .Select(x => new UserContributionDto
                {
                    UserId = x.Key,
                    UserName = x.Value.Name,
                    ContributionCount = x.Value.Count
                })
                .ToList();

            // User role distribution
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var moderatorUsers = await _userManager.GetUsersInRoleAsync("Moderator");
            
            viewModel.AdminCount = adminUsers.Count;
            viewModel.SuperAdminCount = superAdminUsers.Count;
            viewModel.ModeratorCount = moderatorUsers.Count;
            viewModel.RegularUserCount = viewModel.TotalUsers - viewModel.AdminCount - viewModel.SuperAdminCount - viewModel.ModeratorCount;

            // Recent registrations
            viewModel.RecentUsers = allUsers
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "Unknown",
                    Email = u.Email ?? "",
                    CreatedAt = u.CreatedAt,
                    EmailConfirmed = u.EmailConfirmed
                })
                .ToList();

            // Engagement metrics
            var totalQuestions = questions.TotalCount;
            var totalPosts = posts.TotalCount;
            var events = await _eventService.GetEventsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
            var reviews = await _reviewService.GetReviewsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });

            viewModel.AverageContentPerUser = viewModel.TotalUsers > 0 
                ? (totalQuestions + totalPosts + events.TotalCount + reviews.TotalCount) / (double)viewModel.TotalUsers 
                : 0;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user analytics");
            TempData["Error"] = "Failed to load user analytics. Please try again.";
            return View(new UserAnalyticsViewModel());
        }
    }

    [HttpGet]
    public IActionResult GetUserActivityData(string period = "30days")
    {
        try
        {
            var startDate = period switch
            {
                "7days" => DateTimeOffset.UtcNow.AddDays(-7),
                "30days" => DateTimeOffset.UtcNow.AddDays(-30),
                "90days" => DateTimeOffset.UtcNow.AddDays(-90),
                _ => DateTimeOffset.UtcNow.AddDays(-30)
            };

            var allUsers = _userManager.Users.Where(u => !u.IsDeleted).ToList();
            var dailyRegistrations = new Dictionary<string, int>();
            var days = (DateTimeOffset.UtcNow - startDate).Days;

            for (int i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i).Date;
                var dateStr = date.ToString("MMM dd");
                var count = allUsers.Count(u => u.CreatedAt.Date == date);
                dailyRegistrations[dateStr] = count;
            }

            return Json(new { success = true, data = dailyRegistrations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity data");
            return Json(new { success = false, error = "Failed to load activity data" });
        }
    }
}
