using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointFavoriteConfiguration : IEntityTypeConfiguration<MapPointFavorite>
{
    public void Configure(EntityTypeBuilder<MapPointFavorite> builder)
    {
        builder.ToTable("MapPointFavorites");

        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.MapPoint)
            .WithMany()
            .HasForeignKey(f => f.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.MapPointId, f.UserId })
            .IsUnique();

        builder.HasIndex(f => f.MapPointId);
        builder.HasIndex(f => f.UserId);

        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
