using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Repos.Common;
using CommunityCar.Infrastructure.Uow.Common;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.friends.Friendship> _friendshipRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.post.Post> _postRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.qa.Question> _questionRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.groups.CommunityGroup> _groupRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.reviews.Review> _reviewRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.events.CommunityEvent> _eventRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.guides.Guide> _guideRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.news.NewsArticle> _newsRepository;
    private readonly IUnitOfWork _uow;

    public DashboardService(
        IRepository<ApplicationUser> userRepository,
        IRepository<CommunityCar.Domain.Entities.Community.friends.Friendship> friendshipRepository,
        IRepository<CommunityCar.Domain.Entities.Community.post.Post> postRepository,
        IRepository<CommunityCar.Domain.Entities.Community.qa.Question> questionRepository,
        IRepository<CommunityCar.Domain.Entities.Community.groups.CommunityGroup> groupRepository,
        IRepository<CommunityCar.Domain.Entities.Community.reviews.Review> reviewRepository,
        IRepository<CommunityCar.Domain.Entities.Community.events.CommunityEvent> eventRepository,
        IRepository<CommunityCar.Domain.Entities.Community.guides.Guide> guideRepository,
        IRepository<CommunityCar.Domain.Entities.Community.news.NewsArticle> newsRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _postRepository = postRepository;
        _questionRepository = questionRepository;
        _groupRepository = groupRepository;
        _reviewRepository = reviewRepository;
        _eventRepository = eventRepository;
        _guideRepository = guideRepository;
        _newsRepository = newsRepository;
        _uow = uow;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var totalUsers = await _userRepository.CountAsync();
        var totalFriendships = await _friendshipRepository.CountAsync(f => f.Status == CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.Accepted);
        var activeEvents = await _eventRepository.CountAsync();
        var totalPosts = await _postRepository.CountAsync();
        var totalQuestions = await _questionRepository.CountAsync();
        var totalGroups = await _groupRepository.CountAsync();
        var totalReviews = await _reviewRepository.CountAsync();
        var totalGuides = await _guideRepository.CountAsync();
        var totalNews = await _newsRepository.CountAsync();
        
        // Active users today (users who logged in today)
        var today = DateTime.UtcNow.Date;
        var activeUsersToday = await _userRepository.CountAsync(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value >= today);
        
        // New users this week
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var newUsersThisWeek = await _userRepository.CountAsync(u => u.CreatedAt >= weekAgo);
        
        // New users this month
        var monthAgo = DateTime.UtcNow.AddMonths(-1);
        var newUsersThisMonth = await _userRepository.CountAsync(u => u.CreatedAt >= monthAgo);
        
        // Calculate engagement rate (simple calculation based on active users)
        var engagementRate = totalUsers > 0 ? (double)activeUsersToday / totalUsers * 100 : 0;
        
        // Calculate user growth percentage
        var twoMonthsAgo = DateTime.UtcNow.AddMonths(-2);
        var usersLastMonth = await _userRepository.CountAsync(u => u.CreatedAt >= twoMonthsAgo && u.CreatedAt < monthAgo);
        var userGrowthPercentage = usersLastMonth > 0 ? ((double)(newUsersThisMonth - usersLastMonth) / usersLastMonth * 100) : 0;
        
        return new DashboardSummary(
            TotalUsers: totalUsers,
            Slug: "main-dashboard",
            TotalFriendships: totalFriendships,
            ActiveEvents: activeEvents,
            SystemHealth: 98.5,
            TotalPosts: totalPosts,
            TotalQuestions: totalQuestions,
            TotalGroups: totalGroups,
            TotalReviews: totalReviews,
            TotalGuides: totalGuides,
            TotalNews: totalNews,
            ActiveUsersToday: activeUsersToday,
            NewUsersThisWeek: newUsersThisWeek,
            NewUsersThisMonth: newUsersThisMonth,
            EngagementRate: Math.Round(engagementRate, 2),
            UserGrowthPercentage: Math.Round(userGrowthPercentage, 2)
        );
    }

    public async Task<IEnumerable<KPIValue>> GetWeeklyActivityAsync()
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var users = await _userRepository.GetAllAsync();
        
        var dailyRegistrations = users
            .Where(u => u.CreatedAt >= weekAgo)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new KPIValue(g.Key.ToString("ddd"), g.Count()))
            .ToList();
        
        // Fill in missing days with 0
        var result = new List<KPIValue>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).Date;
            var dayName = date.ToString("ddd");
            var count = dailyRegistrations.FirstOrDefault(d => d.Label == dayName)?.Value ?? 0;
            result.Add(new KPIValue(dayName, count));
        }
        
        return result;
    }

    public async Task<IEnumerable<KPIValue>> GetContentDistributionAsync()
    {
        var totalPosts = await _postRepository.CountAsync();
        var totalQuestions = await _questionRepository.CountAsync();
        var totalReviews = await _reviewRepository.CountAsync();
        var totalEvents = await _eventRepository.CountAsync();
        var totalGuides = await _guideRepository.CountAsync();
        
        return new List<KPIValue>
        {
            new("Posts", totalPosts),
            new("Questions", totalQuestions),
            new("Reviews", totalReviews),
            new("Events", totalEvents),
            new("Guides", totalGuides)
        };
    }

    public async Task<IEnumerable<KPIValue>> GetUserGrowthAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var users = await _userRepository.GetAllAsync();
        
        var monthlyGrowth = users
            .Where(u => u.CreatedAt >= sixMonthsAgo)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new KPIValue(
                $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM}",
                g.Count()
            ))
            .OrderBy(k => k.Label)
            .ToList();
        
        return monthlyGrowth;
    }

    public async Task<IEnumerable<KPIValue>> GetTopContentTypesAsync()
    {
        var posts = await _postRepository.CountAsync();
        var questions = await _questionRepository.CountAsync();
        var reviews = await _reviewRepository.CountAsync();
        var groups = await _groupRepository.CountAsync();
        
        var contentTypes = new List<KPIValue>
        {
            new("Posts", posts),
            new("Questions", questions),
            new("Reviews", reviews),
            new("Groups", groups)
        };
        
        return contentTypes.OrderByDescending(c => c.Value).Take(5);
    }

    public async Task<Dictionary<string, int>> GetEngagementMetricsAsync()
    {
        var totalPosts = await _postRepository.CountAsync();
        var totalQuestions = await _questionRepository.CountAsync();
        var totalReviews = await _reviewRepository.CountAsync();
        var totalGroups = await _groupRepository.CountAsync();
        var totalEvents = await _eventRepository.CountAsync();
        
        return new Dictionary<string, int>
        {
            { "Posts", totalPosts },
            { "Questions", totalQuestions },
            { "Reviews", totalReviews },
            { "Groups", totalGroups },
            { "Events", totalEvents }
        };
    }
}
