using CommunityCar.Domain.Entities.Community.events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<CommunityEvent>
{
    public void Configure(EntityTypeBuilder<CommunityEvent> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(e => e.Slug)
            .IsUnique();

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Address)
            .HasMaxLength(1000);

        builder.Property(e => e.Latitude)
            .HasPrecision(10, 7);

        builder.Property(e => e.Longitude)
            .HasPrecision(10, 7);

        builder.Property(e => e.OnlineUrl)
            .HasMaxLength(500);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Attendees)
            .WithOne(a => a.Event)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Event)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.StartTime);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.OrganizerId);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
