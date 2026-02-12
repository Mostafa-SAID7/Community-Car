using CommunityCar.Domain.Entities.Community.friends;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");

        builder.HasKey(f => f.Id);

        // Relationships
        builder.HasOne(f => f.User)
            .WithMany(u => u.SentFriendships)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Friend)
            .WithMany(u => u.ReceivedFriendships)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance optimization
        builder.HasIndex(f => new { f.FriendId, f.Status })
            .HasDatabaseName("IX_Friendships_FriendId_Status");

        builder.HasIndex(f => new { f.UserId, f.Status })
            .HasDatabaseName("IX_Friendships_UserId_Status");

        builder.HasIndex(f => new { f.UserId, f.FriendId })
            .HasDatabaseName("IX_Friendships_UserId_FriendId");

        // Soft delete filter
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
