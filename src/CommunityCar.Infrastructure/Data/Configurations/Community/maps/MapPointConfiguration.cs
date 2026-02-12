using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointConfiguration : IEntityTypeConfiguration<MapPoint>
{
    public void Configure(EntityTypeBuilder<MapPoint> builder)
    {
        builder.ToTable("MapPoints");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mp => mp.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(mp => mp.Slug)
            .IsUnique();

        builder.Property(mp => mp.Description)
            .HasMaxLength(2000);

        // Configure Location as owned entity (value object)
        builder.OwnsOne(mp => mp.Location, location =>
        {
            location.Property(l => l.Latitude)
                .IsRequired()
                .HasPrecision(10, 7);

            location.Property(l => l.Longitude)
                .IsRequired()
                .HasPrecision(10, 7);

            location.Property(l => l.Address)
                .HasMaxLength(500);
        });

        builder.Property(mp => mp.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(mp => mp.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(mp => mp.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(mp => mp.Website)
            .HasMaxLength(500);

        builder.Property(mp => mp.ImageUrl)
            .HasMaxLength(500);

        builder.Property(mp => mp.Tags)
            .HasMaxLength(1000);

        builder.Property(mp => mp.AverageRating)
            .HasPrecision(3, 2);

        builder.HasOne(mp => mp.Owner)
            .WithMany()
            .HasForeignKey(mp => mp.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(mp => mp.Comments)
            .WithOne(c => c.MapPoint)
            .HasForeignKey(c => c.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mp => mp.Ratings)
            .WithOne(r => r.MapPoint)
            .HasForeignKey(r => r.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mp => mp.Type);
        builder.HasIndex(mp => mp.Status);
        builder.HasIndex(mp => mp.OwnerId);
        builder.HasIndex(mp => mp.CreatedAt);

        builder.HasQueryFilter(mp => !mp.IsDeleted);
    }
}
