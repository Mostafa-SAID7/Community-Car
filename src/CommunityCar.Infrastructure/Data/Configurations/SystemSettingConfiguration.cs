using CommunityCar.Domain.Entities.Dashboard.settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.DataType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.DefaultValue)
            .HasMaxLength(2000);

        builder.Property(s => s.ValidationRegex)
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.ModifiedAt);

        // Indexes
        builder.HasIndex(s => s.Key)
            .IsUnique()
            .HasDatabaseName("IX_SystemSettings_Key");

        builder.HasIndex(s => s.Category)
            .HasDatabaseName("IX_SystemSettings_Category");

        builder.HasIndex(s => s.DisplayOrder)
            .HasDatabaseName("IX_SystemSettings_DisplayOrder");
    }
}
