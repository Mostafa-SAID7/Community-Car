using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class QuestionReactionConfiguration : IEntityTypeConfiguration<QuestionReaction>
{
    public void Configure(EntityTypeBuilder<QuestionReaction> builder)
    {
        builder.ToTable("QuestionReactions");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Question)
            .WithMany(q => q.Reactions)
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.QuestionId, r.UserId })
            .IsUnique();
    }
}
