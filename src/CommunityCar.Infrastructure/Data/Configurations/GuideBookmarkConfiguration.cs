using CommunityCar.Domain.Entities.Community.guides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GuideBookmarkConfiguration : IEntityTypeConfiguration<GuideBookmark>
{
    public void Configure(EntityTypeBuilder<GuideBookmark> builder)
    {
        builder.ToTable("GuideBookmarks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.HasOne(b => b.Guide)
            .WithMany()
            .HasForeignKey(b => b.GuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.GuideId, b.UserId })
            .IsUnique();

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
