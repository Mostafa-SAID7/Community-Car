using CommunityCar.Domain.Entities.Dashboard.health;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("HealthChecks");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.Description)
            .HasMaxLength(500);

        builder.Property(h => h.ResponseTime)
            .IsRequired();

        builder.Property(h => h.CheckedAt)
            .IsRequired();

        builder.Property(h => h.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(h => h.CheckedAt);
        builder.HasIndex(h => new { h.Name, h.CheckedAt });
    }
}
