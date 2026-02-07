using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Identity;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Reports;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("Dashboard/Reports/Users")]
public class UserReportsController : Controller
{
    private readonly IUserService _userService;
    private readonly IQuestionService _questionService;
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<UserReportsController> _logger;

    public UserReportsController(
        IUserService userService,
        IQuestionService questionService,
        IPostService postService,
        IEventService eventService,
        IReviewService reviewService,
        ILogger<UserReportsController> logger)
    {
        _userService = userService;
        _questionService = questionService;
        _postService = postService;
        _eventService = eventService;
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy ?? "CreatedAt",
                SortDescending = sortDescending
            };

            var usersResult = await _userService.SearchUsersAsync(parameters);

            if (!usersResult.IsSuccess)
            {
                TempData["Error"] = usersResult.Error;
                return View(new UserReportsViewModel());
            }

            var users = usersResult.Value;

            // Calculate statistics
            var totalUsers = users.TotalCount;
            var activeUsers = users.Items.Count(u => u.IsActive);
            var verifiedUsers = users.Items.Count(u => u.EmailConfirmed);

            // Date filtering
            var filteredUsers = users.Items.AsEnumerable();
            if (startDate.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.CreatedAt <= endDate.Value);
            }

            var viewModel = new UserReportsViewModel
            {
                Users = filteredUsers.Select(u => new UserReportsViewModel.UserReportItem
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt.DateTime,
                    LastLoginAt = u.LastLoginAt
                }).ToList(),
                Filter = new UserReportsFilterViewModel
                {
                    SearchTerm = searchTerm,
                    StartDate = startDate,
                    EndDate = endDate,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = users.PageNumber,
                    TotalPages = users.TotalPages,
                    PageSize = users.PageSize,
                    TotalCount = users.TotalCount
                },
                Statistics = new UserReportsViewModel.UserStatistics
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    VerifiedUsers = verifiedUsers,
                    UnverifiedUsers = totalUsers - verifiedUsers,
                    NewUsersToday = users.Items.Count(u => u.CreatedAt.Date == DateTime.UtcNow.Date),
                    NewUsersThisWeek = users.Items.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                    NewUsersThisMonth = users.Items.Count(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                }
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user reports");
            TempData["Error"] = "Failed to load user reports. Please try again.";
            return View(new UserReportsViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var userResult = await _userService.GetUserByIdAsync(id);

            if (!userResult.IsSuccess)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var user = userResult.Value;

            // Get user's content
            // var parameters = new QueryParameters { PageNumber = 1, PageSize = 100 };
            // var questions = await _questionService.GetQuestionsByUserAsync(id, parameters);
            // var posts = await _postService.GetPostsByUserAsync(id, parameters);
            // var events = await _eventService.GetEventsByUserAsync(id, parameters);
            // var reviews = await _reviewService.GetReviewsByUserAsync(id, parameters);

            var viewModel = new UserDetailsReportViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Bio = user.Bio,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt.DateTime,
                LastLoginAt = user.LastLoginAt,
                TotalQuestions = 0, // questions.TotalCount,
                TotalPosts = 0, // posts.TotalCount,
                TotalEvents = 0, // events.TotalCount,
                TotalReviews = 0, // reviews.TotalCount,
                TotalViews = 0, // questions.Items.Sum(q => q.ViewCount) + posts.Items.Sum(p => p.ViewCount) + events.Items.Sum(e => e.ViewCount),
                TotalLikes = 0, // posts.Items.Sum(p => p.LikeCount),
                RecentQuestions = new List<QuestionDto>(), // questions.Items.Take(5).ToList(),
                RecentPosts = new List<PostDto>(), // posts.Items.Take(5).ToList(),
                RecentEvents = new List<EventDto>(), // events.Items.Take(5).ToList(),
                RecentReviews = new List<ReviewDto>() // reviews.Items.Take(5).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details for {UserId}", id);
            TempData["Error"] = "Failed to load user details.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Activity")]
    public async Task<IActionResult> Activity(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = pageSize
            };

            var usersResult = await _userService.SearchUsersAsync(parameters);

            if (!usersResult.IsSuccess)
            {
                TempData["Error"] = usersResult.Error;
                return View(new UserActivityReportViewModel());
            }

            var users = usersResult.Value;

            // Calculate date range
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-30);

            var activityData = new List<UserActivityReportViewModel.UserActivity>();

            foreach (var user in users.Items)
            {
                // var questions = await _questionService.GetQuestionsByUserAsync(user.Id, new QueryParameters { PageNumber = 1, PageSize = 1000 });
                // var posts = await _postService.GetPostsByUserAsync(user.Id, new QueryParameters { PageNumber = 1, PageSize = 1000 });
                // var events = await _eventService.GetEventsByUserAsync(user.Id, new QueryParameters { PageNumber = 1, PageSize = 1000 });

                // var questionsInPeriod = questions.Items.Count(q => q.CreatedAt >= start && q.CreatedAt <= end);
                // var postsInPeriod = posts.Items.Count(p => p.CreatedAt >= start && p.CreatedAt <= end);
                // var eventsInPeriod = events.Items.Count(e => e.CreatedAt >= start && e.CreatedAt <= end);

                activityData.Add(new UserActivityReportViewModel.UserActivity
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    FullName = $"{user.FirstName} {user.LastName}",
                    QuestionsCreated = 0, // questionsInPeriod,
                    PostsCreated = 0, // postsInPeriod,
                    EventsCreated = 0, // eventsInPeriod,
                    TotalActivity = 0, // questionsInPeriod + postsInPeriod + eventsInPeriod,
                    LastActive = user.LastLoginAt ?? user.CreatedAt.DateTime
                });
            }

            var viewModel = new UserActivityReportViewModel
            {
                Activities = activityData.OrderByDescending(a => a.TotalActivity).ToList(),
                StartDate = start,
                EndDate = end,
                TotalActiveUsers = activityData.Count(a => a.TotalActivity > 0),
                TotalActivity = activityData.Sum(a => a.TotalActivity),
                AverageActivityPerUser = activityData.Any() ? activityData.Average(a => a.TotalActivity) : 0,
                MostActiveUser = activityData.OrderByDescending(a => a.TotalActivity).FirstOrDefault()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity report");
            TempData["Error"] = "Failed to load activity report.";
            return View(new UserActivityReportViewModel());
        }
    }

    [HttpGet("Growth")]
    public async Task<IActionResult> Growth(string period = "12months")
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 10000 };
            var usersResult = await _userService.SearchUsersAsync(parameters);

            if (!usersResult.IsSuccess)
            {
                TempData["Error"] = usersResult.Error;
                return View(new UserGrowthReportViewModel());
            }

            var users = usersResult.Value.Items;

            // Calculate growth data
            var monthlyGrowth = new Dictionary<string, UserGrowthReportViewModel.MonthlyGrowth>();
            var months = period == "6months" ? 6 : 12;

            for (int i = months - 1; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthKey = monthDate.ToString("MMM yyyy");
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var newUsers = users.Count(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                var totalUsers = users.Count(u => u.CreatedAt < monthEnd);

                monthlyGrowth[monthKey] = new UserGrowthReportViewModel.MonthlyGrowth
                {
                    Month = monthKey,
                    NewUsers = newUsers,
                    TotalUsers = totalUsers,
                    GrowthRate = totalUsers > 0 && i < months - 1
                        ? ((double)newUsers / (totalUsers - newUsers)) * 100
                        : 0
                };
            }

            var viewModel = new UserGrowthReportViewModel
            {
                Period = period,
                MonthlyGrowthData = monthlyGrowth,
                TotalUsers = users.Count,
                NewUsersThisMonth = users.Count(u => u.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)),
                NewUsersLastMonth = users.Count(u =>
                {
                    var lastMonth = DateTime.UtcNow.AddMonths(-1);
                    var start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    var end = start.AddMonths(1);
                    return u.CreatedAt >= start && u.CreatedAt < end;
                }),
                AverageNewUsersPerMonth = monthlyGrowth.Any() ? monthlyGrowth.Values.Average(m => m.NewUsers) : 0,
                HighestGrowthMonth = monthlyGrowth.OrderByDescending(m => m.Value.NewUsers).FirstOrDefault().Key
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user growth report");
            TempData["Error"] = "Failed to load growth report.";
            return View(new UserGrowthReportViewModel());
        }
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export(string format = "csv")
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 10000 };
            var usersResult = await _userService.SearchUsersAsync(parameters);

            if (!usersResult.IsSuccess)
            {
                TempData["Error"] = usersResult.Error;
                return RedirectToAction(nameof(Index));
            }

            var users = usersResult.Value.Items;

            if (format.ToLower() == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Id,UserName,Email,FirstName,LastName,IsActive,EmailConfirmed,CreatedAt,LastLoginAt");

                foreach (var user in users)
                {
                    csv.AppendLine($"{user.Id},{user.UserName},{user.Email},{user.FirstName},{user.LastName},{user.IsActive},{user.EmailConfirmed},{user.CreatedAt:yyyy-MM-dd},{user.LastLoginAt:yyyy-MM-dd}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"user-report-{DateTime.UtcNow:yyyyMMdd}.csv");
            }

            TempData["Error"] = "Unsupported export format.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user report");
            TempData["Error"] = "Failed to export report.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Api/Statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 10000 };
            var usersResult = await _userService.SearchUsersAsync(parameters);

            if (!usersResult.IsSuccess)
            {
                return Json(new { error = usersResult.Error });
            }

            var users = usersResult.Value.Items;

            var statistics = new
            {
                totalUsers = users.Count,
                activeUsers = users.Count(u => u.IsActive),
                verifiedUsers = users.Count(u => u.EmailConfirmed),
                newUsersToday = users.Count(u => u.CreatedAt.Date == DateTime.UtcNow.Date),
                newUsersThisWeek = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                newUsersThisMonth = users.Count(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
            };

            return Json(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return Json(new { error = "Failed to load statistics" });
        }
    }
}
