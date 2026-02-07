using CommunityCar.Domain.Entities.Dashboard.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class LocalizationResourceConfiguration : IEntityTypeConfiguration<LocalizationResource>
{
    public void Configure(EntityTypeBuilder<LocalizationResource> builder)
    {
        builder.ToTable("LocalizationResources");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.CultureCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.Category)
            .HasMaxLength(100);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        // Create unique index on Key + CultureCode combination
        builder.HasIndex(l => new { l.Key, l.CultureCode })
            .IsUnique()
            .HasDatabaseName("IX_LocalizationResources_Key_CultureCode");

        // Create index on CultureCode for faster lookups
        builder.HasIndex(l => l.CultureCode)
            .HasDatabaseName("IX_LocalizationResources_CultureCode");

        // Create index on Category for filtering
        builder.HasIndex(l => l.Category)
            .HasDatabaseName("IX_LocalizationResources_Category");

        // Create index on IsActive for filtering
        builder.HasIndex(l => l.IsActive)
            .HasDatabaseName("IX_LocalizationResources_IsActive");
    }
}
