using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class AnswerReactionConfiguration : IEntityTypeConfiguration<AnswerReaction>
{
    public void Configure(EntityTypeBuilder<AnswerReaction> builder)
    {
        builder.ToTable("AnswerReactions");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Answer)
            .WithMany(a => a.Reactions)
            .HasForeignKey(r => r.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.AnswerId, r.UserId })
            .IsUnique();
    }
}
