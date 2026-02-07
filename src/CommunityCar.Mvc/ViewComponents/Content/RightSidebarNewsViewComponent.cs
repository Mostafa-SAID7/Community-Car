using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Content;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Content;

public class RightSidebarNewsViewComponent : ViewComponent
{
    private readonly INewsService _newsService;

    public RightSidebarNewsViewComponent(INewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Simplified - return empty data for now
        var viewModel = new SidebarNewsViewModel
        {
            LatestNews = new List<CommunityCar.Domain.DTOs.Community.NewsArticleDto>(),
            TrendingNews = new List<CommunityCar.Domain.DTOs.Community.NewsArticleDto>()
        };

        return await Task.FromResult(View(viewModel));
    }
}
