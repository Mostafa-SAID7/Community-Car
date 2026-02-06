using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class QuestionBookmarkConfiguration : IEntityTypeConfiguration<QuestionBookmark>
{
    public void Configure(EntityTypeBuilder<QuestionBookmark> builder)
    {
        builder.ToTable("QuestionBookmarks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.HasOne(b => b.Question)
            .WithMany(q => q.Bookmarks)
            .HasForeignKey(b => b.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.QuestionId, b.UserId })
            .IsUnique();
    }
}
