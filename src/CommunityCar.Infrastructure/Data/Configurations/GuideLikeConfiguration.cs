using CommunityCar.Domain.Entities.Community.guides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GuideLikeConfiguration : IEntityTypeConfiguration<GuideLike>
{
    public void Configure(EntityTypeBuilder<GuideLike> builder)
    {
        builder.ToTable("GuideLikes");

        builder.HasKey(gl => gl.Id);

        builder.HasIndex(gl => new { gl.GuideId, gl.UserId })
            .IsUnique()
            .HasDatabaseName("IX_GuideLikes_GuideId_UserId");

        builder.HasOne(gl => gl.Guide)
            .WithMany()
            .HasForeignKey(gl => gl.GuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gl => gl.User)
            .WithMany()
            .HasForeignKey(gl => gl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete query filter
        builder.HasQueryFilter(gl => !gl.IsDeleted);
    }
}
