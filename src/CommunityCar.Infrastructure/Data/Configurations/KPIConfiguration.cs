using CommunityCar.Domain.Entities.Dashboard.KPIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class KPIConfiguration : IEntityTypeConfiguration<KPI>
{
    public void Configure(EntityTypeBuilder<KPI> builder)
    {
        builder.ToTable("KPIs");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(k => k.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(k => k.Category)
            .HasMaxLength(100);

        builder.Property(k => k.Description)
            .HasMaxLength(1000);

        builder.HasIndex(k => k.Code)
            .IsUnique();

        builder.HasIndex(k => k.Category);
        builder.HasIndex(k => k.LastUpdated);
    }
}
