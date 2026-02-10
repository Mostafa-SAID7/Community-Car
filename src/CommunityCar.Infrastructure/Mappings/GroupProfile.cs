using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.groups;

namespace CommunityCar.Infrastructure.Mappings;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<CommunityGroup, GroupDto>()
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => 
                src.Creator != null ? $"{src.Creator.FirstName} {src.Creator.LastName}" : "Unknown User"))
            .ForMember(dest => dest.IsUserMember, opt => opt.Ignore())
            .ForMember(dest => dest.UserRole, opt => opt.Ignore());

        CreateMap<GroupMember, GroupMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown User"))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePictureUrl : null))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.User != null ? src.User.Slug : null));
    }
}
