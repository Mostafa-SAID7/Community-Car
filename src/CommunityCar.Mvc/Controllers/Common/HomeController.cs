using Microsoft.AspNetCore.Mvc;
using CommunityCar.Mvc.Controllers.Base;
using CommunityCar.Domain.Interfaces.Community;
using AutoMapper;

namespace CommunityCar.Mvc.Controllers.Common;

public class HomeController : BaseController
{
    private readonly INewsService _newsService;
    private readonly IEventService _eventService;
    private readonly IMapper _mapper;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        INewsService newsService,
        IEventService eventService,
        IMapper mapper,
        ILogger<HomeController> logger)
    {
        _newsService = newsService;
        _eventService = eventService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var featuredNews = await _newsService.GetFeaturedNewsAsync(5);
            var latestNews = await _newsService.GetLatestNewsAsync(10);
            
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(
                new Domain.Base.QueryParameters { PageNumber = 1, PageSize = 6 },
                userId);

            ViewBag.FeaturedNews = featuredNews;
            ViewBag.LatestNews = latestNews;
            ViewBag.UpcomingEvents = upcomingEvents.Items;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View();
        }
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string subject, string message)
    {
        // TODO: Implement contact form submission logic
        TempData["Success"] = "Your message has been sent successfully!";
        return RedirectToAction(nameof(Contact));
    }
}
