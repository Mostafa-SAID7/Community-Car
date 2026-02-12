using CommunityCar.Domain.Entities.Dashboard.security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class SecurityAlertConfiguration : IEntityTypeConfiguration<SecurityAlert>
{
    public void Configure(EntityTypeBuilder<SecurityAlert> builder)
    {
        builder.ToTable("SecurityAlerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.AlertType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.Source)
            .HasMaxLength(200);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.AffectedUserName)
            .HasMaxLength(256);

        builder.Property(a => a.ResolvedByName)
            .HasMaxLength(256);

        builder.Property(a => a.ResolutionNotes)
            .HasMaxLength(1000);

        builder.Property(a => a.DetectedAt)
            .IsRequired();

        builder.Property(a => a.IsResolved)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.Severity);
        builder.HasIndex(a => a.AlertType);
        builder.HasIndex(a => a.IsResolved);
        builder.HasIndex(a => a.DetectedAt);
        builder.HasIndex(a => a.AffectedUserId);
        builder.HasIndex(a => new { a.Severity, a.IsResolved });
        builder.HasIndex(a => new { a.DetectedAt, a.IsResolved });
    }
}
