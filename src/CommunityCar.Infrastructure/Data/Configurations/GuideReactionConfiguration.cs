using CommunityCar.Domain.Entities.Community.guides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GuideReactionConfiguration : IEntityTypeConfiguration<GuideReaction>
{
    public void Configure(EntityTypeBuilder<GuideReaction> builder)
    {
        builder.ToTable("GuideReactions");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Guide)
            .WithMany()
            .HasForeignKey(r => r.GuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.GuideId, r.UserId })
            .IsUnique();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
