using AutoMapper;
using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Infrastructure.Mappings.Community.Friends;

public class FriendshipProfile : Profile
{
    public FriendshipProfile()
    {
        // Entity to DTO
        CreateMap<Friendship, FriendshipDto>()
            .ForMember(dest => dest.Since, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.FriendName, opt => opt.MapFrom(src => $"User {src.FriendId.ToString().Substring(0, 8)}"))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore());

        CreateMap<Friendship, FriendRequestDto>()
            .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"User {src.UserId.ToString().Substring(0, 8)}"))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore());
    }
}
