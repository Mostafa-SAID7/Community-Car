using CommunityCar.Domain.Entities.Community.news;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("NewsArticles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.Summary)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Source)
            .HasMaxLength(200);

        builder.Property(a => a.ExternalUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Tags)
            .HasMaxLength(1000);

        builder.Property(a => a.MetaDescription)
            .HasMaxLength(500);

        builder.Property(a => a.MetaKeywords)
            .HasMaxLength(500);

        builder.HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Comments)
            .WithOne(c => c.NewsArticle)
            .HasForeignKey(c => c.NewsArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Category);
        builder.HasIndex(a => a.IsFeatured);
        builder.HasIndex(a => a.PublishedAt);
        builder.HasIndex(a => a.AuthorId);
    }
}
