using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.guides;

namespace CommunityCar.Infrastructure.Mappings.Community.Guides;

public class GuideProfile : Profile
{
    public GuideProfile()
    {
        CreateMap<Guide, GuideDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DifficultyName, opt => opt.MapFrom(src => src.Difficulty.ToString()))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : "Unknown"))
            .ForMember(dest => dest.AuthorAvatar, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePictureUrl : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore())
            .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
            .ForMember(dest => dest.IsBookmarked, opt => opt.Ignore());

        CreateMap<GuideStep, GuideStepDto>();

        CreateMap<GuideComment, GuideCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePictureUrl : null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore());
    }
}
