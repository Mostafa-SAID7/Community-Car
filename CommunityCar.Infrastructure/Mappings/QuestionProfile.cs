using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Entities.Community.qa;

namespace CommunityCar.Infrastructure.Mappings;

public class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName ?? string.Empty))
            .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author.ProfilePictureUrl))
            .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers.Count))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.QuestionTags.Select(qt => qt.Tag)));

        CreateMap<Answer, AnswerDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName ?? string.Empty))
            .ForMember(dest => dest.AuthorProfilePicture, opt => opt.MapFrom(src => src.Author.ProfilePictureUrl));

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));

        CreateMap<Tag, TagDto>();
    }
}
