using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Analytics;

public class UserReportsViewModel
{
    public List<UserReportItem> Users { get; set; } = new();
    public UserReportsFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public UserStatistics Statistics { get; set; } = new();

    public class UserReportItem
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int UnverifiedUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
    }
}

public class UserReportsFilterViewModel
{
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class UserDetailsReportViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalPosts { get; set; }
    public int TotalEvents { get; set; }
    public int TotalReviews { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public List<QuestionDto> RecentQuestions { get; set; } = new();
    public List<PostDto> RecentPosts { get; set; } = new();
    public List<EventDto> RecentEvents { get; set; } = new();
    public List<ReviewDto> RecentReviews { get; set; } = new();
}

public class UserActivityReportViewModel
{
    public List<UserActivity> Activities { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalActiveUsers { get; set; }
    public int TotalActivity { get; set; }
    public double AverageActivityPerUser { get; set; }
    public UserActivity? MostActiveUser { get; set; }

    public class UserActivity
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int QuestionsCreated { get; set; }
        public int PostsCreated { get; set; }
        public int EventsCreated { get; set; }
        public int TotalActivity { get; set; }
        public DateTime LastActive { get; set; }
    }
}

public class UserGrowthReportViewModel
{
    public string Period { get; set; } = "12months";
    public Dictionary<string, MonthlyGrowth> MonthlyGrowthData { get; set; } = new();
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewUsersLastMonth { get; set; }
    public double AverageNewUsersPerMonth { get; set; }
    public string? HighestGrowthMonth { get; set; }

    public class MonthlyGrowth
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
        public double GrowthRate { get; set; }
    }
}
