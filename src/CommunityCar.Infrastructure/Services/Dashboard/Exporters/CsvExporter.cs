using CommunityCar.Domain.Models;
using System.Text;

namespace CommunityCar.Infrastructure.Services.Dashboard.Exporters;

public class CsvExporter
{
    public byte[] Export(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Dashboard Report");
        csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        csv.AppendLine();
        
        // Summary Statistics
        AppendSummarySection(csv, summary);
        
        // Weekly Activity
        AppendActivitySection(csv, activity);

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private void AppendSummarySection(StringBuilder csv, DashboardSummary summary)
    {
        csv.AppendLine("Summary Statistics");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Users,{summary.TotalUsers}");
        csv.AppendLine($"Total Friendships,{summary.TotalFriendships}");
        csv.AppendLine($"Active Events,{summary.ActiveEvents}");
        csv.AppendLine($"System Health,{summary.SystemHealth}%");
        csv.AppendLine($"Total Posts,{summary.TotalPosts}");
        csv.AppendLine($"Total Questions,{summary.TotalQuestions}");
        csv.AppendLine($"Total Groups,{summary.TotalGroups}");
        csv.AppendLine($"Total Reviews,{summary.TotalReviews}");
        csv.AppendLine($"Active Users Today,{summary.ActiveUsersToday}");
        csv.AppendLine($"New Users This Week,{summary.NewUsersThisWeek}");
        csv.AppendLine($"New Users This Month,{summary.NewUsersThisMonth}");
        csv.AppendLine($"Engagement Rate,{summary.EngagementRate}%");
        csv.AppendLine();
    }

    private void AppendActivitySection(StringBuilder csv, IEnumerable<KPIValue> activity)
    {
        csv.AppendLine("Weekly Activity");
        csv.AppendLine("Date,Registrations");
        
        foreach (var item in activity)
        {
            csv.AppendLine($"{item.Label},{item.Value}");
        }
    }
}
