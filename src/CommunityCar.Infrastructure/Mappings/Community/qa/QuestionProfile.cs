using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Entities.Community.qa;

namespace CommunityCar.Infrastructure.Mappings.Community.Qa;

public class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null 
                    ? (!string.IsNullOrWhiteSpace(src.Author.FirstName) || !string.IsNullOrWhiteSpace(src.Author.LastName) 
                        ? (src.Author.FirstName + " " + src.Author.LastName).Trim() 
                        : src.Author.UserName)
                    : "Unknown User"))
            .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePictureUrl : null))
            .ForMember(dest => dest.AuthorIsExpert, opt => opt.MapFrom(src => src.Author != null ? src.Author.IsExpert : false))
            .ForMember(dest => dest.AuthorRankName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Rank.ToString() : "Newbie"))
            .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0))
            .ForMember(dest => dest.ReactionCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count : 0))
            .ForMember(dest => dest.BookmarkCount, opt => opt.MapFrom(src => src.Bookmarks != null ? src.Bookmarks.Count : 0))
            .ForMember(dest => dest.ShareCount, opt => opt.MapFrom(src => src.Shares != null ? src.Shares.Count : 0))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : null))
            .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.QuestionTags != null ? src.QuestionTags.Select(qt => qt.Tag) : new List<Tag>()))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug));

        CreateMap<Answer, AnswerDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null
                    ? (!string.IsNullOrWhiteSpace(src.Author.FirstName) || !string.IsNullOrWhiteSpace(src.Author.LastName) 
                        ? (src.Author.FirstName + " " + src.Author.LastName).Trim() 
                        : src.Author.UserName)
                    : "Unknown User"))
            .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePictureUrl : null))
            .ForMember(dest => dest.AuthorIsExpert, opt => opt.MapFrom(src => src.Author != null ? src.Author.IsExpert : false))
            .ForMember(dest => dest.AuthorRankName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Rank.ToString() : "Newbie"));

        CreateMap<AnswerComment, AnswerCommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null
                    ? (!string.IsNullOrWhiteSpace(src.Author.FirstName) || !string.IsNullOrWhiteSpace(src.Author.LastName) 
                        ? (src.Author.FirstName + " " + src.Author.LastName).Trim() 
                        : src.Author.UserName)
                    : "Unknown User"))
            .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author != null ? src.Author.ProfilePictureUrl : null))
            .ForMember(dest => dest.AuthorIsExpert, opt => opt.MapFrom(src => src.Author != null ? src.Author.IsExpert : false))
            .ForMember(dest => dest.AuthorRankName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Rank.ToString() : "Newbie"));

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));

        CreateMap<Tag, TagDto>();
    }
}
