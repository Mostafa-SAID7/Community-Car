using CommunityCar.Domain.Entities.Community.maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MapPointCommentConfiguration : IEntityTypeConfiguration<MapPointComment>
{
    public void Configure(EntityTypeBuilder<MapPointComment> builder)
    {
        builder.ToTable("MapPointComments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(c => c.MapPoint)
            .WithMany(mp => mp.Comments)
            .HasForeignKey(c => c.MapPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.MapPointId);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
