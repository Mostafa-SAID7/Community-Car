using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.Areas.Community.ViewModels.qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.qa;

[Area("Community")]
[Route("Community/Questions")]
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;

    public QuestionsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1, 
        int pageSize = 20, 
        string? search = null, 
        string? tag = null, 
        bool? isResolved = null,
        string? sortBy = null,
        bool sortDesc = true,
        string tab = "all")
    {
        var parameters = new QueryParameters 
        { 
            PageNumber = page, 
            PageSize = pageSize,
            SearchTerm = search,
            SortBy = sortBy ?? "created",
            SortDescending = sortDesc
        };
        
        var userId = _currentUserService.UserId;
        
        var result = tab.ToLower() switch
        {
            "myquestions" when userId.HasValue => await _questionService.GetUserQuestionsAsync(userId.Value, parameters),
            "answered" => await _questionService.GetQuestionsAsync(parameters, search, tag, true),
            "unanswered" => await _questionService.GetQuestionsAsync(parameters, search, tag, false),
            "trending" => await _questionService.GetTrendingQuestionsAsync(parameters, search, tag),
            "recent" => await _questionService.GetRecentQuestionsAsync(parameters, search, tag),
            _ => await _questionService.GetQuestionsAsync(parameters, search, tag, isResolved)
        };
        
        ViewBag.SearchTerm = search;
        ViewBag.CurrentTag = tag;
        ViewBag.IsResolved = isResolved;
        ViewBag.SortBy = sortBy ?? "created";
        ViewBag.SortDesc = sortDesc;
        ViewBag.CurrentTab = tab;
        
        return View(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        await _questionService.IncrementViewCountAsync(id);
        
        var answers = await _questionService.GetAnswersAsync(id);
        
        ViewBag.Answers = answers;
        return View(question);
    }

    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var question = await _questionService.CreateQuestionAsync(
            model.Title, 
            model.Content, 
            userId.Value, 
            model.Tags);

        return RedirectToAction(nameof(Details), new { id = question.Id });
    }

    [Authorize]
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (question.AuthorId != userId)
            return Forbid();

        var model = new EditQuestionViewModel
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            Tags = question.Tags
        };

        return View(model);
    }

    [Authorize]
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditQuestionViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (question.AuthorId != userId)
            return Forbid();

        await _questionService.UpdateQuestionAsync(id, model.Title, model.Content, model.Tags);

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (question.AuthorId != userId)
            return Forbid();

        await _questionService.DeleteQuestionAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost("Vote/{id}")]
    public async Task<IActionResult> Vote(Guid id, bool isUpvote)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.VoteQuestionAsync(id, userId.Value, isUpvote);

        return Ok();
    }

    [Authorize]
    [HttpPost("RemoveVote/{id}")]
    public async Task<IActionResult> RemoveVote(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveQuestionVoteAsync(id, userId.Value);

        return Ok();
    }

    [Authorize]
    [HttpPost("AcceptAnswer")]
    public async Task<IActionResult> AcceptAnswer(Guid questionId, Guid answerId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.AcceptAnswerAsync(questionId, answerId, userId.Value);

        return Ok();
    }

    [Authorize]
    [HttpPost("UnacceptAnswer")]
    public async Task<IActionResult> UnacceptAnswer(Guid questionId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.UnacceptAnswerAsync(questionId, userId.Value);

        return Ok();
    }
}


    // Bookmark actions
    [Authorize]
    [HttpPost("Bookmark/{id}")]
    public async Task<IActionResult> Bookmark(Guid id, string? notes = null)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.BookmarkQuestionAsync(id, userId.Value, notes);

        return Ok(new { message = "Question bookmarked successfully" });
    }

    [Authorize]
    [HttpPost("RemoveBookmark/{id}")]
    public async Task<IActionResult> RemoveBookmark(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveBookmarkAsync(id, userId.Value);

        return Ok(new { message = "Bookmark removed successfully" });
    }

    [Authorize]
    [HttpGet("Bookmarks")]
    public async Task<IActionResult> Bookmarks(int page = 1, int pageSize = 20)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
        var result = await _questionService.GetUserBookmarksAsync(userId.Value, parameters);

        return View(result);
    }

    // Reaction actions
    [Authorize]
    [HttpPost("React/{id}")]
    public async Task<IActionResult> React(Guid id, int reactionType)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.ReactToQuestionAsync(id, userId.Value, (ReactionType)reactionType);

        var summary = await _questionService.GetQuestionReactionSummaryAsync(id, userId.Value);
        return Ok(summary);
    }

    [Authorize]
    [HttpPost("RemoveReaction/{id}")]
    public async Task<IActionResult> RemoveReaction(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveQuestionReactionAsync(id, userId.Value);

        var summary = await _questionService.GetQuestionReactionSummaryAsync(id, userId.Value);
        return Ok(summary);
    }

    [HttpGet("Reactions/{id}")]
    public async Task<IActionResult> GetReactions(Guid id)
    {
        var userId = _currentUserService.UserId;
        var summary = await _questionService.GetQuestionReactionSummaryAsync(id, userId);

        return Ok(summary);
    }

    // Share actions
    [Authorize]
    [HttpPost("Share/{id}")]
    public async Task<IActionResult> Share(Guid id, string? platform = null, string? sharedUrl = null)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.ShareQuestionAsync(id, userId.Value, platform, sharedUrl);

        var shareCount = await _questionService.GetQuestionShareCountAsync(id);
        return Ok(new { message = "Question shared successfully", shareCount });
    }
}
