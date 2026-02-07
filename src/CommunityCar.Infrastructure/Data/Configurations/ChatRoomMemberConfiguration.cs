using CommunityCar.Domain.Entities.Communications.chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ChatRoomMemberConfiguration : IEntityTypeConfiguration<ChatRoomMember>
{
    public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
    {
        builder.ToTable("ChatRoomMembers");

        builder.HasKey(crm => crm.Id);

        builder.Property(crm => crm.JoinedAt)
            .IsRequired();

        builder.Property(crm => crm.IsMuted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(crm => crm.ChatRoom)
            .WithMany(cr => cr.Members)
            .HasForeignKey(crm => crm.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(crm => crm.User)
            .WithMany()
            .HasForeignKey(crm => crm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(crm => new { crm.ChatRoomId, crm.UserId })
            .IsUnique();

        builder.HasQueryFilter(crm => !crm.IsDeleted);
    }
}
