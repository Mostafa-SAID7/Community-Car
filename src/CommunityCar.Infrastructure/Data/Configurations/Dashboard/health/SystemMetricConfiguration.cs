using CommunityCar.Domain.Entities.Dashboard.health;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class SystemMetricConfiguration : IEntityTypeConfiguration<SystemMetric>
{
    public void Configure(EntityTypeBuilder<SystemMetric> builder)
    {
        builder.ToTable("SystemMetrics");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MetricName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Value)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(m => m.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.RecordedAt)
            .IsRequired();

        builder.Property(m => m.Category)
            .HasMaxLength(100);

        builder.HasIndex(m => m.RecordedAt);
        builder.HasIndex(m => new { m.MetricName, m.RecordedAt });
    }
}
