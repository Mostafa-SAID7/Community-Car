using CommunityCar.Domain.Entities.Community.reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ReviewReactionConfiguration : IEntityTypeConfiguration<ReviewReaction>
{
    public void Configure(EntityTypeBuilder<ReviewReaction> builder)
    {
        builder.ToTable("ReviewReactions");

        builder.HasKey(rr => rr.Id);

        builder.HasIndex(rr => new { rr.ReviewId, rr.UserId })
            .IsUnique();

        builder.HasOne(rr => rr.Review)
            .WithMany(r => r.Reactions)
            .HasForeignKey(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.User)
            .WithMany()
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
