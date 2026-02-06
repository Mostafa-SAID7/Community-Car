using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class AnswerVoteConfiguration : IEntityTypeConfiguration<AnswerVote>
{
    public void Configure(EntityTypeBuilder<AnswerVote> builder)
    {
        builder.ToTable("AnswerVotes");

        builder.HasKey(v => v.Id);

        builder.HasOne(v => v.Answer)
            .WithMany(a => a.Votes)
            .HasForeignKey(v => v.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.AnswerId, v.UserId })
            .IsUnique();
    }
}
