using CommunityCar.Domain.Entities.Community.events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class EventAttendeeConfiguration : IEntityTypeConfiguration<EventAttendee>
{
    public void Configure(EntityTypeBuilder<EventAttendee> builder)
    {
        builder.ToTable("EventAttendees");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.HasOne(a => a.Event)
            .WithMany(e => e.Attendees)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EventId, a.UserId })
            .IsUnique();

        builder.HasIndex(a => a.Status);

        // Soft delete filter
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
