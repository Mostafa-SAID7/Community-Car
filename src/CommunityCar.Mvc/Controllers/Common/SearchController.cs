using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Common;

public class SearchController : BaseController
{
    private readonly IPostService _postService;
    private readonly IQuestionService _questionService;
    private readonly IGuideService _guideService;
    private readonly INewsService _newsService;
    private readonly IEventService _eventService;
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IPostService postService,
        IQuestionService questionService,
        IGuideService guideService,
        INewsService newsService,
        IEventService eventService,
        IGroupService groupService,
        IMapper mapper,
        ILogger<SearchController> logger)
    {
        _postService = postService;
        _questionService = questionService;
        _guideService = guideService;
        _newsService = newsService;
        _eventService = eventService;
        _groupService = groupService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q = null,
        string? type = null,
        int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            ViewBag.Query = q;
            ViewBag.Type = type;
            return View();
        }

        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = 20, SearchTerm = q };

            ViewBag.Query = q;
            ViewBag.Type = type;
            ViewBag.PageNumber = page;

            switch (type?.ToLower())
            {
                case "posts":
                    var posts = await _postService.GetPostsAsync(parameters, Domain.Enums.Community.post.PostStatus.Published, null, null, userId);
                    ViewBag.Posts = posts;
                    ViewBag.TotalCount = posts.TotalCount;
                    ViewBag.TotalPages = posts.TotalPages;
                    break;

                case "questions":
                    var questions = await _questionService.GetQuestionsAsync(parameters, q, null, null, null, null, userId);
                    ViewBag.Questions = questions;
                    ViewBag.TotalCount = questions.TotalCount;
                    ViewBag.TotalPages = questions.TotalPages;
                    break;

                case "guides":
                    var guides = await _guideService.SearchGuidesAsync(q, parameters, userId);
                    ViewBag.Guides = guides;
                    ViewBag.TotalCount = guides.TotalCount;
                    ViewBag.TotalPages = guides.TotalPages;
                    break;

                case "news":
                    var news = await _newsService.GetNewsArticlesAsync(parameters, Domain.Enums.Community.news.NewsStatus.Published, null, null, userId);
                    ViewBag.News = news;
                    ViewBag.TotalCount = news.TotalCount;
                    ViewBag.TotalPages = news.TotalPages;
                    break;

                case "events":
                    var events = await _eventService.GetEventsAsync(parameters, null, Domain.Enums.Community.events.EventStatus.Published, null, null, userId);
                    ViewBag.Events = events;
                    ViewBag.TotalCount = events.TotalCount;
                    ViewBag.TotalPages = events.TotalPages;
                    break;

                case "groups":
                    var groups = await _groupService.SearchGroupsAsync(q, parameters, userId);
                    ViewBag.Groups = groups;
                    ViewBag.TotalCount = groups.TotalCount;
                    ViewBag.TotalPages = groups.TotalPages;
                    break;

                default:
                    // Search all types
                    var allParams = new QueryParameters { PageNumber = 1, PageSize = 5, SearchTerm = q };
                    
                    var allPosts = await _postService.GetPostsAsync(allParams, Domain.Enums.Community.post.PostStatus.Published, null, null, userId);
                    ViewBag.Posts = allPosts;

                    var allQuestions = await _questionService.GetQuestionsAsync(allParams, q, null, null, null, null, userId);
                    ViewBag.Questions = allQuestions;

                    var allGuides = await _guideService.SearchGuidesAsync(q, allParams, userId);
                    ViewBag.Guides = allGuides;

                    var allGroups = await _groupService.SearchGroupsAsync(q, allParams, userId);
                    ViewBag.Groups = allGroups;

                    ViewBag.TotalCount = allPosts.TotalCount + allQuestions.TotalCount + 
                                          allGuides.TotalCount + allGroups.TotalCount;
                    break;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", q);
            ViewBag.Query = q;
            ViewBag.Error = "An error occurred while searching.";
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Autocomplete(string q, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(new { suggestions = new List<object>() });
        }

        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = 1, PageSize = limit, SearchTerm = q };

            var posts = await _postService.GetPostsAsync(parameters, Domain.Enums.Community.post.PostStatus.Published, null, null, userId);
            var questions = await _questionService.GetQuestionsAsync(parameters, q, null, null, null, null, userId);

            var suggestions = new List<object>();
            
            suggestions.AddRange(posts.Items.Take(3).Select(p => new
            {
                type = "post",
                title = p.Title,
                url = Url.Action("Details", "Posts", new { id = p.Id })
            }));

            suggestions.AddRange(questions.Items.Take(3).Select(q => new
            {
                type = "question",
                title = q.Title,
                url = Url.Action("Details", "Questions", new { slug = q.Slug })
            }));

            return Json(new { suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in autocomplete for query: {Query}", q);
            return Json(new { suggestions = new List<object>() });
        }
    }
}
