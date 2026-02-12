using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointCheckInConfiguration : IEntityTypeConfiguration<MapPointCheckIn>
{
    public void Configure(EntityTypeBuilder<MapPointCheckIn> builder)
    {
        builder.ToTable("MapPointCheckIns");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Note)
            .HasMaxLength(500);

        builder.HasOne(ci => ci.MapPoint)
            .WithMany()
            .HasForeignKey(ci => ci.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.User)
            .WithMany()
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ci => ci.MapPointId);
        builder.HasIndex(ci => ci.UserId);
        builder.HasIndex(ci => ci.CheckInTime);

        builder.HasQueryFilter(ci => !ci.IsDeleted);
    }
}
