using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class QuestionShareConfiguration : IEntityTypeConfiguration<QuestionShare>
{
    public void Configure(EntityTypeBuilder<QuestionShare> builder)
    {
        builder.ToTable("QuestionShares");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Platform)
            .HasMaxLength(50);

        builder.Property(s => s.SharedUrl)
            .HasMaxLength(500);

        builder.HasOne(s => s.Question)
            .WithMany(q => q.Shares)
            .HasForeignKey(s => s.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SharedByUser)
            .WithMany()
            .HasForeignKey(s => s.SharedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.QuestionId);
        builder.HasIndex(s => s.SharedByUserId);
        builder.HasIndex(s => s.CreatedAt);
    }
}
