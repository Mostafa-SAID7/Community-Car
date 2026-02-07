using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointRatingConfiguration : IEntityTypeConfiguration<MapPointRating>
{
    public void Configure(EntityTypeBuilder<MapPointRating> builder)
    {
        builder.ToTable("MapPointRatings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(500);

        builder.HasOne(r => r.MapPoint)
            .WithMany(mp => mp.Ratings)
            .HasForeignKey(r => r.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.MapPointId, r.UserId })
            .IsUnique();

        builder.HasIndex(r => r.MapPointId);
        builder.HasIndex(r => r.UserId);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
