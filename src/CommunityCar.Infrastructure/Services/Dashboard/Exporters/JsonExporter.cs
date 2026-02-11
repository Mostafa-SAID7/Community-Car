using CommunityCar.Domain.Models;
using System.Text;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services.Dashboard.Exporters;

public class JsonExporter
{
    public byte[] Export(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        var report = BuildReportObject(summary, activity);
        
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(json);
    }

    private object BuildReportObject(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        return new
        {
            GeneratedAt = DateTime.UtcNow,
            Summary = new
            {
                summary.TotalUsers,
                summary.TotalFriendships,
                summary.ActiveEvents,
                summary.SystemHealth,
                summary.TotalPosts,
                summary.TotalQuestions,
                summary.TotalGroups,
                summary.TotalReviews,
                summary.ActiveUsersToday,
                summary.NewUsersThisWeek,
                summary.NewUsersThisMonth,
                summary.EngagementRate
            },
            WeeklyActivity = activity.Select(a => new 
            { 
                a.Label, 
                a.Value 
            })
        };
    }
}
