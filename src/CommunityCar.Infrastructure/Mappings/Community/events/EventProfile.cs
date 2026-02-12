using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.events;

namespace CommunityCar.Infrastructure.Mappings.Community.Events;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<CommunityEvent, EventDto>()
            .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src => src.Organizer.UserName ?? "Unknown"))
            .ForMember(dest => dest.OrganizerAvatar, opt => opt.MapFrom(src => src.Organizer.ProfilePictureUrl))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AttendeeCount, opt => opt.MapFrom(src => src.AttendeeCount))
            .ForMember(dest => dest.InterestedCount, opt => opt.MapFrom(src => src.InterestedCount))
            .ForMember(dest => dest.IsOrganizer, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentUserStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CanJoin, opt => opt.MapFrom(src => src.CanAcceptMoreAttendees()));

        CreateMap<EventAttendee, EventAttendeeDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? "Unknown"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<EventComment, EventCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? "Unknown"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore());
    }
}
