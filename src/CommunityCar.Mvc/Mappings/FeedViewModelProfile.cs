using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Mvc.ViewModels.Feed;

namespace CommunityCar.Mvc.Mappings;

public class FeedViewModelProfile : Profile
{
    public FeedViewModelProfile()
    {
        CreateMap<FeedItemDto, FeedItemViewModel>();
        CreateMap<FeedResultDto, FeedViewModel>();
    }
}
