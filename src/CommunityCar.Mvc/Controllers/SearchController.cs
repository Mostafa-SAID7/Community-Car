using CommunityCar.Domain.Base;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Identity;
using CommunityCar.Mvc.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CommunityCar.Mvc.Controllers;

[Route("{culture:alpha}/[controller]")]
public class SearchController : Controller
{
    private record DashboardItem(string TitleEn, string TitleAr, string Action, string Controller, string Icon, string? SubArea = null);

    private static readonly List<DashboardItem> DashboardItems = new()
    {
        new DashboardItem("Audit Logs", "سجلات المراجعة", "Index", "AuditLogs", "fas fa-file-contract"),
        new DashboardItem("KPIs", "المؤشرات الرئيسية", "Index", "KPIs", "fas fa-chart-line"),
        new DashboardItem("Localization", "الترجمة", "Index", "Localization", "fas fa-language"),
        new DashboardItem("User Activity", "نشاط المستخدمين", "Index", "UserActivity", "fas fa-user-clock"),
        new DashboardItem("Analytics", "التحليلات", "Index", "ContentAnalytics", "fas fa-chart-bar", "analytics"),
        new DashboardItem("Health", "حالة النظام", "Index", "Health", "fas fa-heartbeat"),
        new DashboardItem("User Management", "إدارة المستخدمين", "Index", "UserManagement", "fas fa-users-cog", "management"),
        new DashboardItem("Overview", "نظرة عامة", "Index", "Overview", "fas fa-tachometer-alt"),
        new DashboardItem("Reports", "التقارير", "Index", "Reports", "fas fa-file-alt"),
        new DashboardItem("Security", "الأمان", "Index", "Security", "fas fa-shield-alt"),
        new DashboardItem("Settings", "الإعدادات", "Index", "Settings", "fas fa-cog"),
        new DashboardItem("System Status", "حالة النظام", "Index", "System", "fas fa-server"),
        new DashboardItem("Trends", "الاتجاهات", "Index", "Trends", "fas fa-chart-area"),
    };

    private readonly IPostService _postService;
    private readonly IQuestionService _questionService;
    private readonly IGroupService _groupService;
    private readonly IEventService _eventService;
    private readonly IUserService _userService;
    private readonly IFriendshipService _friendshipService;

    public SearchController(
        IPostService postService,
        IQuestionService questionService,
        IGroupService groupService,
        IEventService eventService,
        IUserService userService,
        IFriendshipService friendshipService)
    {
        _postService = postService;
        _questionService = questionService;
        _groupService = groupService;
        _eventService = eventService;
        _userService = userService;
        _friendshipService = friendshipService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string query)
    {
        var viewModel = new GlobalSearchViewModel { Query = query };
        if (string.IsNullOrWhiteSpace(query)) return View(viewModel);

        var parameters = new QueryParameters { SearchTerm = query, PageSize = 20 };

        // 1. Search Posts
        var posts = await _postService.GetPostsAsync(parameters);
        foreach (var p in posts.Items)
        {
            viewModel.Results.Add(new SearchResultItemViewModel
            {
                Title = p.Title ?? string.Empty,
                Description = StripHtml(p.Content ?? string.Empty),
                Url = Url.Action("Details", "Feed", new { culture = GetCulture(), slug = p.Slug }) ?? string.Empty,
                Type = "Post",
                Icon = "fas fa-newspaper",
                ImageUrl = string.Empty,
                CreatedAt = p.CreatedAt
            });
        }

        // 2. Search Questions
        var questions = await _questionService.GetQuestionsAsync(parameters, searchTerm: query);
        foreach (var q in questions.Items)
        {
            viewModel.Results.Add(new SearchResultItemViewModel
            {
                Title = q.Title ?? string.Empty,
                Description = StripHtml(q.Content ?? string.Empty),
                Url = Url.Action("Details", "Questions", new { culture = GetCulture(), slug = q.Slug }) ?? string.Empty,
                Type = "Question",
                Icon = "fas fa-question-circle",
                ImageUrl = string.Empty,
                CreatedAt = q.CreatedAt
            });
        }

        // 3. Search Groups
        var groups = await _groupService.SearchGroupsAsync(query, parameters);
        foreach (var g in groups.Items)
        {
            viewModel.Results.Add(new SearchResultItemViewModel
            {
                Title = g.Name ?? string.Empty,
                Description = g.Description ?? string.Empty,
                Url = Url.Action("Details", "Groups", new { culture = GetCulture(), slug = g.Slug }) ?? string.Empty,
                Type = "Group",
                Icon = "fas fa-users",
                ImageUrl = string.Empty,
                CreatedAt = g.CreatedAt
            });
        }

        // 4. Search Events
        var events = await _eventService.GetEventsAsync(parameters);
        foreach (var e in events.Items)
        {
            viewModel.Results.Add(new SearchResultItemViewModel
            {
                Title = e.Title ?? string.Empty,
                Description = e.Description ?? string.Empty,
                Url = Url.Action("Details", "Events", new { culture = GetCulture(), slug = e.Slug }) ?? string.Empty,
                Type = "Event",
                Icon = "fas fa-calendar-alt",
                ImageUrl = string.Empty,
                CreatedAt = e.CreatedAt
            });
        }

        // 5. Search Users & Friends
        var currentUserId = GetCurrentUserId();
        var friends = currentUserId.HasValue 
            ? await _friendshipService.GetFriendsAsync(currentUserId.Value)
            : new List<CommunityCar.Domain.Entities.Community.friends.Friendship>();
            
        var friendIds = friends.Select(f => f.UserId == currentUserId.Value ? f.FriendId : f.UserId).ToHashSet();

        var usersResult = await _userService.SearchUsersAsync(parameters);
        if (usersResult.IsSuccess)
        {
            foreach (var u in usersResult.Value.Items)
            {
                var isFriend = friendIds.Contains(u.Id);
                viewModel.Results.Add(new SearchResultItemViewModel
                {
                    Title = $"{u.FirstName} {u.LastName}",
                    Description = u.Bio ?? string.Empty,
                    Url = Url.Action("Index", "Profiles", new { culture = GetCulture(), area = "Identity", id = u.Id }) ?? string.Empty,
                    Type = isFriend ? "Friend" : "User",
                    Icon = isFriend ? "fas fa-user-friends" : "fas fa-user",
                    ImageUrl = u.ProfilePictureUrl ?? string.Empty,
                    CreatedAt = u.CreatedAt
                });
            }
        }

        // 6. Search Dashboard (Admin Only)
        if (User.IsInRole("Admin"))
        {
            var isArabic = GetCulture() == "ar";
            var dashboardMatches = DashboardItems.Where(d => 
                d.TitleEn.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                d.TitleAr.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var d in dashboardMatches)
            {
                viewModel.Results.Add(new SearchResultItemViewModel
                {
                    Title = isArabic ? d.TitleAr : d.TitleEn,
                    Description = isArabic ? "إدارة " + d.TitleAr : "Manage " + d.TitleEn,
                    Url = Url.Action(d.Action, d.Controller, new { area = "Dashboard", culture = GetCulture() }) ?? string.Empty,
                    Type = "Admin",
                    Icon = d.Icon,
                    ImageUrl = string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow // Priority items
                });
            }
        }

        viewModel.Results = viewModel.Results.OrderByDescending(r => r.CreatedAt).ToList();
        return View(viewModel);
    }

    [HttpGet("Live")]
    public async Task<IActionResult> Live(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2) 
            return Json(new { success = true, results = new List<object>() });

        var parameters = new QueryParameters { SearchTerm = query, PageSize = 3 };
        var results = new List<object>();

        // We only take a few from each for live search to keep it fast
        
        // Posts
        var posts = await _postService.GetPostsAsync(parameters);
        results.AddRange(posts.Items.Select(p => new {
            title = p.Title,
            url = Url.Action("Details", "Feed", new { culture = GetCulture(), slug = p.Slug }),
            type = "Post",
            icon = "fas fa-newspaper"
        }));

        // Questions
        var questions = await _questionService.GetQuestionsAsync(parameters, searchTerm: query);
        results.AddRange(questions.Items.Select(q => new {
            title = q.Title,
            url = Url.Action("Details", "Questions", new { culture = GetCulture(), slug = q.Slug }),
            type = "Question",
            icon = "fas fa-question-circle"
        }));

        // Users & Friends
        var currentUserId = GetCurrentUserId();
        var friendIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            var friends = await _friendshipService.GetFriendsAsync(currentUserId.Value);
            friendIds = friends.Select(f => f.UserId == currentUserId.Value ? f.FriendId : f.UserId).ToHashSet();
        }

        var usersResult = await _userService.SearchUsersAsync(parameters);
        if (usersResult.IsSuccess)
        {
            results.AddRange(usersResult.Value.Items.Select(u => {
                var isFriend = friendIds.Contains(u.Id);
                return new {
                    title = $"{u.FirstName} {u.LastName}",
                    url = Url.Action("Index", "Profiles", new { culture = GetCulture(), area = "Identity", id = u.Id }),
                    type = isFriend ? "Friend" : "User",
                    icon = isFriend ? "fas fa-user-friends" : "fas fa-user",
                    image = u.ProfilePictureUrl
                };
            }));
        }

        // Dashboard Suggestions (Admin Only)
        if (User.IsInRole("Admin"))
        {
            var isArabic = GetCulture() == "ar";
            var dashboardMatches = DashboardItems.Where(d => 
                d.TitleEn.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                d.TitleAr.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(3);

            results.AddRange(dashboardMatches.Select(d => new {
                title = isArabic ? d.TitleAr : d.TitleEn,
                url = Url.Action(d.Action, d.Controller, new { area = "Dashboard", culture = GetCulture() }),
                type = "Admin",
                icon = d.Icon
            }));
        }

        return Json(new { success = true, results = results.Take(10) });
    }

    private string GetCulture() => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    private Guid? GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return null;
        }
        return userId;
    }

    private string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var text = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        return text.Length > 150 ? text.Substring(0, 147) + "..." : text;
    }
}
