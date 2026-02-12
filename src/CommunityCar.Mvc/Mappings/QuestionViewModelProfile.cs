using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Mvc.ViewModels.Qa;

namespace CommunityCar.Mvc.Mappings;

public class QuestionViewModelProfile : Profile
{
    public QuestionViewModelProfile()
    {
        CreateMap<QuestionDto, QuestionViewModel>();
        CreateMap<AnswerDto, AnswerViewModel>();
    }
}
