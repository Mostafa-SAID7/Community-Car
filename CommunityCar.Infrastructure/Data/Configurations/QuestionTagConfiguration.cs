using CommunityCar.Domain.Entities.Common;
using CommunityCar.Domain.Entities.Community.qa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class QuestionTagConfiguration : IEntityTypeConfiguration<QuestionTag>
{
    public void Configure(EntityTypeBuilder<QuestionTag> builder)
    {
        builder.ToTable("QuestionTags");

        builder.HasKey(qt => qt.Id);

        builder.HasIndex(qt => new { qt.QuestionId, qt.TagId })
            .IsUnique();

        builder.HasOne(qt => qt.Question)
            .WithMany(q => q.QuestionTags)
            .HasForeignKey(qt => qt.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(qt => qt.Tag)
            .WithMany(t => t.QuestionTags)
            .HasForeignKey(qt => qt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
