using CommunityCar.Domain.Entities.Community.guides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GuideStepConfiguration : IEntityTypeConfiguration<GuideStep>
{
    public void Configure(EntityTypeBuilder<GuideStep> builder)
    {
        builder.ToTable("GuideSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Content)
            .IsRequired();

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.VideoUrl)
            .HasMaxLength(500);

        builder.HasIndex(s => new { s.GuideId, s.StepNumber });
    }
}
