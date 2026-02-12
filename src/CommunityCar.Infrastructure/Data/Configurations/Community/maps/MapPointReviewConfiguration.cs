using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointReviewConfiguration : IEntityTypeConfiguration<MapPointReview>
{
    public void Configure(EntityTypeBuilder<MapPointReview> builder)
    {
        builder.ToTable("MapPointReviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.HelpfulCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(r => r.MapPoint)
            .WithMany()
            .HasForeignKey(r => r.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.MapPointId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
