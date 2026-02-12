using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Mvc.ViewModels.Feed;
using CommunityCar.Domain.Enums.Community.Feed;
using CommunityCar.Domain.Interfaces.Community;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture}/[controller]")]
public class FeedController : Controller
{
    private readonly IFeedService _feedService;
    private readonly IMapper _mapper;
    private readonly ILogger<FeedController> _logger;

    public FeedController(
        IFeedService feedService,
        IMapper mapper,
        ILogger<FeedController> logger)
    {
        _feedService = feedService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        FeedItemType? type = null,
        string? search = null,
        DateFilterType dateFilter = DateFilterType.All,
        FeedSortType sortBy = FeedSortType.Recent)
    {
        var result = await _feedService.GetFeedAsync(
            page: page,
            pageSize: 20,
            contentType: type,
            searchTerm: search,
            dateFilter: dateFilter,
            sortBy: sortBy);

        var viewModel = new FeedViewModel
        {
            Items = _mapper.Map<List<FeedItemViewModel>>(result.Items),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            Filters = new FeedFilterViewModel
            {
                ContentType = type,
                SearchTerm = search,
                DateFilter = dateFilter,
                SortBy = sortBy
            }
        };

        return View(viewModel);
    }

    [HttpGet("Popular")]
    public async Task<IActionResult> Popular(int page = 1)
    {
        var result = await _feedService.GetPopularFeedAsync(page);

        var viewModel = new FeedViewModel
        {
            Items = _mapper.Map<List<FeedItemViewModel>>(result.Items),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            Filters = new FeedFilterViewModel
            {
                SortBy = FeedSortType.Popular
            }
        };

        return View("Index", viewModel);
    }

    [HttpGet("Trending")]
    public async Task<IActionResult> Trending(int page = 1)
    {
        var result = await _feedService.GetTrendingFeedAsync(page);

        var viewModel = new FeedViewModel
        {
            Items = _mapper.Map<List<FeedItemViewModel>>(result.Items),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            Filters = new FeedFilterViewModel
            {
                SortBy = FeedSortType.Trending
            }
        };

        return View(viewModel);
    }

    [HttpGet("Following")]
    public async Task<IActionResult> Following(int page = 1)
    {
        var result = await _feedService.GetFeedAsync(
            page: page,
            pageSize: 20,
            followingOnly: true,
            sortBy: FeedSortType.Recent);

        var viewModel = new FeedViewModel
        {
            Items = _mapper.Map<List<FeedItemViewModel>>(result.Items),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            Filters = new FeedFilterViewModel
            {
                FollowingOnly = true,
                SortBy = FeedSortType.Recent
            }
        };

        return View(viewModel);
    }

    [HttpGet("Featured")]
    public async Task<IActionResult> Featured(int page = 1)
    {
        var result = await _feedService.GetFeedAsync(
            page: page,
            pageSize: 20,
            isFeatured: true,
            sortBy: FeedSortType.Recent);

        var viewModel = new FeedViewModel
        {
            Items = _mapper.Map<List<FeedItemViewModel>>(result.Items),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
            Filters = new FeedFilterViewModel
            {
                IsFeatured = true,
                SortBy = FeedSortType.Recent
            }
        };

        return View(viewModel);
    }

    [HttpGet("Bookmarks")]
    public async Task<IActionResult> Bookmarks(int page = 1)
    {
        // TODO: Implement bookmarks functionality
        // This will require a BookmarkService and user authentication
        var viewModel = new FeedViewModel
        {
            Items = new List<FeedItemViewModel>(),
            CurrentPage = page,
            TotalPages = 0,
            TotalCount = 0,
            Filters = new FeedFilterViewModel
            {
                SortBy = FeedSortType.Recent
            }
        };

        return View(viewModel);
    }
}
