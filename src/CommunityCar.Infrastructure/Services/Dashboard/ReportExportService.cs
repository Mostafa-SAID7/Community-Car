using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Infrastructure.Services.Dashboard.Exporters;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class ReportExportService : IReportExportService
{
    private readonly CsvExporter _csvExporter;
    private readonly JsonExporter _jsonExporter;
    private readonly PdfExporter _pdfExporter;

    public ReportExportService()
    {
        _csvExporter = new CsvExporter();
        _jsonExporter = new JsonExporter();
        _pdfExporter = new PdfExporter();
    }

    public async Task<byte[]> ExportToCsvAsync(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        return await Task.FromResult(_csvExporter.Export(summary, activity));
    }

    public async Task<byte[]> ExportToJsonAsync(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        return await Task.FromResult(_jsonExporter.Export(summary, activity));
    }

    public async Task<byte[]> ExportToPdfAsync(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        return await Task.FromResult(_pdfExporter.Export(summary, activity));
    }

    public string GenerateFileName(string format)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var sanitizedFormat = format.ToLower().Trim();
        
        return $"dashboard-report-{timestamp}.{sanitizedFormat}";
    }
}
