using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.post;

namespace CommunityCar.Infrastructure.Mappings.Community.Posts;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : "Unknown"))
            .ForMember(dest => dest.AuthorAvatar, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePictureUrl : null))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore())
            .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt));

        CreateMap<PostComment, PostCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePictureUrl : null))
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt));
    }
}
