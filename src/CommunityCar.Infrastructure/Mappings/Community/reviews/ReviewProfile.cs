using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.reviews;

namespace CommunityCar.Infrastructure.Mappings.Community.Reviews;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.UserName ?? "Unknown"))
            .ForMember(dest => dest.ReviewerAvatar, opt => opt.MapFrom(src => src.Reviewer.ProfilePictureUrl))
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : null))
            .ForMember(dest => dest.IsReviewer, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentUserReaction, opt => opt.Ignore());

        CreateMap<ReviewComment, ReviewCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? "Unknown"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore());
    }
}
