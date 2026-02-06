using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Web.ViewModels.Community.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.SignalR;
using CommunityCar.Web.Hubs;

namespace CommunityCar.Web.Controllers;

[Route("Questions")]
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<QuestionHub> _hubContext;

    public QuestionsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService,
        IHubContext<QuestionHub> hubContext)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
        _hubContext = hubContext;
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
            "bookmarks" when userId.HasValue => await _questionService.GetBookmarkedQuestionsAsync(userId.Value, parameters),
            "answered" => await _questionService.GetQuestionsAsync(parameters, search, tag, hasAnswers: true, currentUserId: userId),
            "unanswered" => await _questionService.GetQuestionsAsync(parameters, search, tag, hasAnswers: false, currentUserId: userId),
            "trending" => await _questionService.GetTrendingQuestionsAsync(parameters, search, tag, currentUserId: userId),
            "recent" => await _questionService.GetRecentQuestionsAsync(parameters, search, tag, currentUserId: userId),
            _ => await _questionService.GetQuestionsAsync(parameters, search, tag, isResolved, currentUserId: userId)
        };
        
        ViewBag.SearchTerm = search;
        ViewBag.CurrentTag = tag;
        ViewBag.IsResolved = isResolved;
        ViewBag.SortBy = sortBy ?? "created";
        ViewBag.SortDesc = sortDesc;
        ViewBag.CurrentTab = tab;
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_QuestionList", result);
        }
        
        return View(result);
    }

    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> Details(string idOrSlug)
    {
        QuestionDto? question = null;
        
        if (Guid.TryParse(idOrSlug, out var id))
        {
            question = await _questionService.GetQuestionByIdAsync(id, _currentUserService.UserId);
            // If found by ID and has a slug, redirect to slug URL for better SEO
            if (question != null && !string.IsNullOrEmpty(question.Slug))
            {
                return RedirectToActionPermanent(nameof(Details), new { idOrSlug = question.Slug });
            }
        }
        else
        {
            question = await _questionService.GetQuestionBySlugAsync(idOrSlug, _currentUserService.UserId);
        }

        if (question == null)
            return NotFound();

        await _questionService.IncrementViewCountAsync(question.Id);
        
        var answers = await _questionService.GetAnswersAsync(question.Id, _currentUserService.UserId);
        var relatedQuestions = await _questionService.GetRelatedQuestionsAsync(question.Id, 4);
        
        ViewBag.Answers = answers;
        ViewBag.RelatedQuestions = relatedQuestions;
        return View(question);
    }



    [Authorize]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateQuestionViewModel
        {
            Categories = await _questionService.GetCategoriesAsync()
        };
        return View(model);
    }

    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _questionService.GetCategoriesAsync();
            return View(model);
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var question = await _questionService.CreateQuestionAsync(
            model.Title, 
            model.Content, 
            userId.Value, 
            model.CategoryId,
            model.Tags);

        // Real-time broadcast
        await _hubContext.Clients.All.SendAsync("ReceiveQuestion", question);

        TempData["SuccessToast"] = "Question published successfully!";
        return RedirectToAction(nameof(Details), new { idOrSlug = question.Slug ?? question.Id.ToString() });
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
            CategoryId = question.CategoryId,
            Tags = question.Tags,
            Categories = await _questionService.GetCategoriesAsync()
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
        {
            model.Categories = await _questionService.GetCategoriesAsync();
            return View(model);
        }

        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (question.AuthorId != userId)
            return Forbid();

        var updatedQuestion = await _questionService.UpdateQuestionAsync(id, model.Title, model.Content, model.CategoryId, model.Tags);

        TempData["SuccessToast"] = "Question updated successfully!";
        return RedirectToAction(nameof(Details), new { idOrSlug = updatedQuestion.Slug ?? id.ToString() });
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

        TempData["SuccessToast"] = "Question deleted successfully!";
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

        var question = await _questionService.GetQuestionByIdAsync(id, userId.Value);
        return Ok(new { 
            voteCount = question?.VoteCount ?? 0,
            userVote = question?.CurrentUserVote ?? 0
        });
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

    // Bookmark actions
    [Authorize]
    [HttpPost("Bookmark/{id}")]
    public async Task<IActionResult> Bookmark(Guid id, string? notes = null)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.BookmarkQuestionAsync(id, userId.Value, notes);

        var isBookmarked = await _questionService.GetBookmarkAsync(id, userId.Value) != null;
        var bookmarkCount = (await _questionService.GetQuestionByIdAsync(id))?.BookmarkCount ?? 0;

        return Ok(new { 
            message = isBookmarked ? "Question bookmarked successfully" : "Bookmark removed successfully",
            isBookmarked,
            bookmarkCount
        });
    }

    [Authorize]
    [HttpPost("RemoveBookmark/{id}")]
    public async Task<IActionResult> RemoveBookmark(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveBookmarkAsync(id, userId.Value);

        var bookmarkCount = (await _questionService.GetQuestionByIdAsync(id))?.BookmarkCount ?? 0;

        return Ok(new { 
            message = "Bookmark removed successfully",
            isBookmarked = false,
            bookmarkCount
        });
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

    [HttpGet("GetQuestionCard/{id}")]
    public async Task<IActionResult> GetQuestionCard(Guid id)
    {
        var question = await _questionService.GetQuestionByIdAsync(id, _currentUserService.UserId);
        if (question == null) return NotFound();

        // Wrap in PagedResult to reuse _QuestionList
        var result = new PagedResult<QuestionDto>(new List<QuestionDto> { question }, 1, 1, 1);
        return PartialView("_QuestionList", result);
    }
}
