using CommunityCar.Domain.Entities.Community.events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class EventCommentConfiguration : IEntityTypeConfiguration<EventComment>
{
    public void Configure(EntityTypeBuilder<EventComment> builder)
    {
        builder.ToTable("EventComments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(c => c.Event)
            .WithMany(e => e.Comments)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.EventId);
        builder.HasIndex(c => c.UserId);

        // Soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
