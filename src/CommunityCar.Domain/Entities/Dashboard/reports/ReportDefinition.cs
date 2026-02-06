using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.reports;

public class ReportDefinition : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QueryJson { get; set; } = string.Empty;
    public string ScheduleCron { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
