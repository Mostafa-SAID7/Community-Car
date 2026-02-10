using CommunityCar.Domain.Entities.Community.reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.EntityId)
            .IsRequired();

        builder.Property(r => r.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(r => r.Pros)
            .HasMaxLength(2000);

        builder.Property(r => r.Cons)
            .HasMaxLength(2000);

        builder.Property(r => r.ImageUrls)
            .HasMaxLength(2000);

        builder.Property(r => r.ModerationNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.Slug)
            .HasMaxLength(250);

        // Configure Rating value object with backing field
        builder.Property("_rating")
            .HasColumnName("Rating")
            .HasColumnType("decimal(3,1)")
            .IsRequired();

        builder.Ignore(r => r.Rating);

        builder.HasIndex(r => r.Slug)
            .IsUnique();

        builder.HasIndex(r => new { r.EntityId, r.EntityType });
        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Type);
        builder.HasIndex("_rating");

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Reactions)
            .WithOne(rr => rr.Review)
            .HasForeignKey(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Comments)
            .WithOne(rc => rc.Review)
            .HasForeignKey(rc => rc.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
