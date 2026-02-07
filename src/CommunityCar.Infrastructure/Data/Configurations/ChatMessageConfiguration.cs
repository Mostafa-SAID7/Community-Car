using CommunityCar.Domain.Entities.Communications.chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cm => cm.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(cm => cm.AttachmentUrl)
            .HasMaxLength(500);

        builder.Property(cm => cm.IsEdited)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(cm => cm.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(cm => cm.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.ReplyToMessage)
            .WithMany()
            .HasForeignKey(cm => cm.ReplyToMessageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cm => cm.ChatRoomId);
        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.CreatedAt);

        builder.HasQueryFilter(cm => !cm.IsDeleted);
    }
}
