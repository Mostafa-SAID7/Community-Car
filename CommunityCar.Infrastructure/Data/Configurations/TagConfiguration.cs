using CommunityCar.Domain.Entities.Community.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(200);

        builder.Property(t => t.UsageCount)
            .HasDefaultValue(0);

        builder.HasMany(t => t.QuestionTags)
            .WithOne(qt => qt.Tag)
            .HasForeignKey(qt => qt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
