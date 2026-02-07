namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class ContentAnalyticsViewModel
{
    public string Period { get; set; } = "30days";
    
    // Total counts
    public int TotalQuestions { get; set; }
    public int TotalPosts { get; set; }
    public int TotalEvents { get; set; }
    public int TotalNews { get; set; }
    public int TotalGuides { get; set; }
    public int TotalReviews { get; set; }
    
    // New content in period
    public int NewQuestions { get; set; }
    public int NewPosts { get; set; }
    public int NewEvents { get; set; }
    public int NewNews { get; set; }
    public int NewGuides { get; set; }
    public int NewReviews { get; set; }
    
    // Engagement metrics
    public int TotalViews { get; set; }
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    
    // Content distribution percentages
    public double QuestionsPercentage { get; set; }
    public double PostsPercentage { get; set; }
    public double EventsPercentage { get; set; }
    public double NewsPercentage { get; set; }
    public double GuidesPercentage { get; set; }
    public double ReviewsPercentage { get; set; }
    
    // Average engagement
    public double AvgViewsPerQuestion { get; set; }
    public double AvgViewsPerPost { get; set; }
    
    // Most viewed content
    public List<ContentItem> MostViewedQuestions { get; set; } = new();
    public List<ContentItem> MostViewedPosts { get; set; } = new();
    
    // Monthly growth data
    public Dictionary<string, MonthlyContentData> MonthlyGrowth { get; set; } = new();
    
    public class ContentItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }
    
    public class MonthlyContentData
    {
        public int Questions { get; set; }
        public int Posts { get; set; }
        public int Events { get; set; }
        public int News { get; set; }
        public int Guides { get; set; }
        public int Reviews { get; set; }
        public int Total => Questions + Posts + Events + News + Guides + Reviews;
    }
}
