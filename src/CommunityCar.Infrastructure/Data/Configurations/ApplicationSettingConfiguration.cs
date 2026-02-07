using CommunityCar.Domain.Entities.Dashboard.settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ApplicationSettingConfiguration : IEntityTypeConfiguration<ApplicationSetting>
{
    public void Configure(EntityTypeBuilder<ApplicationSetting> builder)
    {
        builder.ToTable("ApplicationSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.IsEncrypted)
            .IsRequired();

        builder.Property(s => s.LastModified)
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.HasIndex(s => s.Category);

        builder.HasIndex(s => s.LastModified);
    }
}
