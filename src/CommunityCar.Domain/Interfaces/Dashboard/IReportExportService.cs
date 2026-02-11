using CommunityCar.Domain.Models;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IReportExportService
{
    Task<byte[]> ExportToCsvAsync(DashboardSummary summary, IEnumerable<KPIValue> activity);
    Task<byte[]> ExportToJsonAsync(DashboardSummary summary, IEnumerable<KPIValue> activity);
    Task<byte[]> ExportToPdfAsync(DashboardSummary summary, IEnumerable<KPIValue> activity);
    string GenerateFileName(string format);
}
