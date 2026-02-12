using AutoMapper;
using CommunityCar.Domain.DTOs.Communications;
using CommunityCar.Domain.Entities.Communications.chats;

namespace CommunityCar.Infrastructure.Mappings.Communications.Chats;

public class ChatProfile : Profile
{
    public ChatProfile()
    {
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.UserName ?? "Unknown"))
            .ForMember(dest => dest.SenderProfilePicture, opt => opt.MapFrom(src => src.Sender.ProfilePictureUrl))
            .ForMember(dest => dest.ReceiverId, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiverName, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiverProfilePicture, opt => opt.Ignore())
            .ForMember(dest => dest.IsRead, opt => opt.Ignore());

        CreateMap<SendMessageDto, ChatMessage>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRoomId, opt => opt.Ignore())
            .ForMember(dest => dest.SenderId, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRoom, opt => opt.Ignore())
            .ForMember(dest => dest.Sender, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.IsEdited, opt => opt.Ignore())
            .ForMember(dest => dest.EditedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReplyToMessageId, opt => opt.Ignore())
            .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}
