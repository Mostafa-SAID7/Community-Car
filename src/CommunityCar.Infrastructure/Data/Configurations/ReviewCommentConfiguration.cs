using CommunityCar.Domain.Entities.Community.reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ReviewCommentConfiguration : IEntityTypeConfiguration<ReviewComment>
{
    public void Configure(EntityTypeBuilder<ReviewComment> builder)
    {
        builder.ToTable("ReviewComments");

        builder.HasKey(rc => rc.Id);

        builder.Property(rc => rc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(rc => rc.ReviewId);
        builder.HasIndex(rc => rc.UserId);

        builder.HasOne(rc => rc.Review)
            .WithMany(r => r.Comments)
            .HasForeignKey(rc => rc.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.User)
            .WithMany()
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
