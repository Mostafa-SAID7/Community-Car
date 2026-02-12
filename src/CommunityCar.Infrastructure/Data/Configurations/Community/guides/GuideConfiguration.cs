using CommunityCar.Domain.Entities.Community.guides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GuideConfiguration : IEntityTypeConfiguration<Guide>
{
    public void Configure(EntityTypeBuilder<Guide> builder)
    {
        builder.ToTable("Guides");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Content)
            .IsRequired();

        builder.Property(g => g.Summary)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(g => g.Status)
            .IsRequired();

        builder.Property(g => g.Difficulty)
            .IsRequired();

        builder.Property(g => g.Tags)
            .HasMaxLength(1000);

        builder.Property(g => g.ImageUrl)
            .HasMaxLength(500);

        builder.Property(g => g.VideoUrl)
            .HasMaxLength(500);

        builder.Property(g => g.MetaDescription)
            .HasMaxLength(500);

        builder.Property(g => g.MetaKeywords)
            .HasMaxLength(500);

        builder.HasOne(g => g.Author)
            .WithMany()
            .HasForeignKey(g => g.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Steps)
            .WithOne(s => s.Guide)
            .HasForeignKey(s => s.GuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Comments)
            .WithOne(c => c.Guide)
            .HasForeignKey(c => c.GuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.Slug).IsUnique();
        builder.HasIndex(g => g.Category);
        builder.HasIndex(g => g.Status);
        builder.HasIndex(g => g.AuthorId);
        builder.HasIndex(g => g.CreatedAt);
    }
}
