using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Dashboard.KPIs;

public class KPI : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public double Value { get; private set; }
    public double? PreviousValue { get; private set; }
    public double? Target { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    private KPI() { }

    public KPI(string name, string code, double value, string unit, string? category = null, string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(code, nameof(code));
        Guard.Against.NullOrWhiteSpace(unit, nameof(unit));

        Name = name;
        Code = code;
        Value = value;
        Unit = unit;
        Category = category;
        Description = description;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void UpdateValue(double newValue)
    {
        PreviousValue = Value;
        Value = newValue;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void Update(string name, double value, string unit, string? category = null, string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(unit, nameof(unit));

        Name = name;
        PreviousValue = Value;
        Value = value;
        Unit = unit;
        Category = category;
        Description = description;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void SetTarget(double target)
    {
        Guard.Against.Negative(target, nameof(target));
        Target = target;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public double GetChangePercentage()
    {
        if (!PreviousValue.HasValue || PreviousValue.Value == 0)
            return 0;

        return ((Value - PreviousValue.Value) / PreviousValue.Value) * 100;
    }

    public string GetTrend()
    {
        if (!PreviousValue.HasValue)
            return "stable";

        var change = GetChangePercentage();
        if (Math.Abs(change) < 1) return "stable";
        return change > 0 ? "up" : "down";
    }
}
