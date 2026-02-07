namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class ContentReportsViewModel
{
    public string ReportType { get; set; } = "summary";
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalContent { get; set; }
    public int TotalViews { get; set; }
    public int TotalEngagement { get; set; }
    
    // Content counts by type
    public int QuestionCount { get; set; }
    public int PostCount { get; set; }
    public int EventCount { get; set; }
    public int NewsCount { get; set; }
    public int GuideCount { get; set; }
    public int ReviewCount { get; set; }
    
    // Engagement metrics
    public double AverageViewsPerContent { get; set; }
    public double AverageEngagementPerContent { get; set; }
    
    // Quality metrics
    public int ResolvedQuestions { get; set; }
    public int UnresolvedQuestions { get; set; }
    public double AverageReviewRating { get; set; }
    
    // Top authors
    public List<AuthorStats> TopAuthors { get; set; } = new();
    
    // Daily breakdown
    public List<DailyStats> DailyBreakdown { get; set; } = new();
    
    // Popular tags
    public List<TagStats> PopularTags { get; set; } = new();
    
    public class AuthorStats
    {
        public string AuthorName { get; set; } = string.Empty;
        public int ContentCount { get; set; }
        public int TotalViews { get; set; }
        public int TotalEngagement { get; set; }
    }
    
    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int QuestionCount { get; set; }
        public int PostCount { get; set; }
        public int EventCount { get; set; }
        public int NewsCount { get; set; }
        public int GuideCount { get; set; }
        public int ReviewCount { get; set; }
        public int TotalCount => QuestionCount + PostCount + EventCount + NewsCount + GuideCount + ReviewCount;
    }
    
    public class TagStats
    {
        public string TagName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}
