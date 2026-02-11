using CommunityCar.Domain.Models;

namespace CommunityCar.Infrastructure.Services.Dashboard.Exporters;

public class PdfExporter
{
    private readonly CsvExporter _csvExporter;

    public PdfExporter()
    {
        _csvExporter = new CsvExporter();
    }

    public byte[] Export(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        // TODO: Implement PDF export using a library like:
        // - QuestPDF (recommended, modern, fluent API)
        // - iTextSharp / iText7
        // - PdfSharp
        // - DinkToPdf (HTML to PDF)
        
        // For now, return CSV as fallback
        // When implementing, create a proper PDF with:
        // - Header with logo and title
        // - Summary statistics table
        // - Charts/graphs for activity data
        // - Footer with generation date
        
        return _csvExporter.Export(summary, activity);
    }
}
