using CommunityCar.Domain.Base;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Qa;

[Route("{culture:alpha}/Questions")]
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGroupService _groupService;
    private readonly IStringLocalizer<QuestionsController> _localizer;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService,
        IGroupService groupService,
        IStringLocalizer<QuestionsController> localizer,
        ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
        _groupService = groupService;
        _localizer = localizer;
        _logger = logger;
    }

    // GET: Questions
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? tag = null, bool? isResolved = null, string? searchTerm = null, Guid? categoryId = null, string sortBy = "recent")
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            
            var result = await _questionService.GetQuestionsAsync(parameters, searchTerm: searchTerm, tag: tag, isResolved: isResolved, categoryId: categoryId, currentUserId: currentUserId);
            
            ViewBag.CurrentTag = tag;
            ViewBag.IsResolved = isResolved;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentCategoryId = categoryId;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_QuestionList", result);
            }
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading questions");
            TempData["Error"] = _localizer["FailedToLoadQuestions"].Value;
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>(
                new List<Domain.DTOs.Community.QuestionDto>(), 0, page, pageSize));
        }
    }

    // GET: Questions/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var question = await _questionService.GetQuestionByIdAsync(id, currentUserId);
            if (question == null)
            {
                TempData["Error"] = _localizer["QuestionNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            await _questionService.IncrementViewCountAsync(id);
            
            var answers = await _questionService.GetAnswersAsync(id, currentUserId);
            
            ViewBag.Answers = answers;
            ViewBag.CurrentUserId = currentUserId;
            
            return View(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question {QuestionId}", id);
            TempData["Error"] = _localizer["FailedToLoadQuestion"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Questions/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var question = await _questionService.GetQuestionBySlugAsync(slug, currentUserId);
            if (question == null)
            {
                TempData["Error"] = _localizer["QuestionNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            await _questionService.IncrementViewCountAsync(question.Id);
            
            var answers = await _questionService.GetAnswersAsync(question.Id, currentUserId);
            
            ViewBag.Answers = answers;
            ViewBag.CurrentUserId = currentUserId;
            
            return View("Details", question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadQuestion"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Questions/Create
    [Authorize]
    [HttpGet("Create")]
    public async Task<IActionResult> Create(Guid? groupId = null)
    {
        try
        {
            var categories = await _questionService.GetCategoriesAsync();
            var viewModel = new CreateQuestionViewModel
            {
                Title = string.Empty,
                Content = string.Empty,
                Tags = string.Empty,
                CategoryId = null,
                Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
            
            // Load user groups for group selection
            var userId = GetCurrentUserId();
            var userGroups = await _groupService.GetUserGroupsAsync(userId, new QueryParameters { PageSize = 100 });
            ViewBag.UserGroups = userGroups.Items;
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create question page");
            TempData["Error"] = _localizer["FailedToLoadCreatePage"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Questions/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model, Guid? groupId = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Categories = (await _questionService.GetCategoriesAsync())
                    .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList();
                
                var userId = GetCurrentUserId();
                var userGroups = await _groupService.GetUserGroupsAsync(userId, new QueryParameters { PageSize = 100 });
                ViewBag.UserGroups = userGroups.Items;
                
                return View(model);
            }

            var currentUserId = GetCurrentUserId();

            var question = await _questionService.CreateQuestionAsync(
                model.Title, 
                model.Content, 
                currentUserId, 
                categoryId: model.CategoryId,
                groupId: groupId,
                tags: model.Tags);

            TempData["Success"] = _localizer["QuestionCreatedSuccessfully"].Value;
            return RedirectToAction(nameof(Details), new { slug = question.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            ModelState.AddModelError("", _localizer["FailedToCreateQuestion"].Value);
            model.Categories = (await _questionService.GetCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList();
            
            var userId = GetCurrentUserId();
            var userGroups = await _groupService.GetUserGroupsAsync(userId, new QueryParameters { PageSize = 100 });
            ViewBag.UserGroups = userGroups.Items;
            
            return View(model);
        }
    }

    // GET: Questions/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                TempData["Error"] = _localizer["QuestionNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
            {
                TempData["Error"] = _localizer["CannotEditOthersQuestion"].Value;
                return RedirectToAction(nameof(Details), new { id });
            }

            var categories = await _questionService.GetCategoriesAsync();
            var model = new EditQuestionViewModel
            {
                Id = question.Id,
                Title = question.Title,
                Content = question.Content,
                Tags = question.Tags,
                CategoryId = question.CategoryId,
                Categories = categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question for edit {QuestionId}", id);
            TempData["Error"] = _localizer["FailedToLoadQuestion"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Questions/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditQuestionViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Categories = (await _questionService.GetCategoriesAsync())
                    .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(model);
            }

            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                TempData["Error"] = _localizer["QuestionNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
            {
                TempData["Error"] = _localizer["CannotEditOthersQuestion"].Value;
                return RedirectToAction(nameof(Details), new { id });
            }

            await _questionService.UpdateQuestionAsync(id, model.Title, model.Content, categoryId: model.CategoryId, tags: model.Tags);

            TempData["Success"] = _localizer["QuestionUpdatedSuccessfully"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdateQuestion"].Value);
            model.Categories = (await _questionService.GetCategoriesAsync())
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.Name, c.Id.ToString())).ToList();
            return View(model);
        }
    }

    // POST: Questions/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
                return Json(new { success = false, message = _localizer["QuestionNotFound"].Value });

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
                return Json(new { success = false, message = _localizer["CannotDeleteOthersQuestion"].Value });

            await _questionService.DeleteQuestionAsync(id);

            return Json(new { success = true, message = _localizer["QuestionDeletedSuccessfully"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {QuestionId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeleteQuestion"].Value });
        }
    }

    // POST: Questions/Vote/{id}
    [Authorize]
    [HttpPost("Vote/{id:guid}")]
    public async Task<IActionResult> Vote(Guid id, bool isUpvote)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.VoteQuestionAsync(id, userId, isUpvote);

            return Json(new { success = true, message = _localizer["VoteRecorded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on question {QuestionId}", id);
            return Json(new { success = false, message = _localizer["FailedToVote"].Value });
        }
    }

    // POST: Questions/RemoveVote/{id}
    [Authorize]
    [HttpPost("RemoveVote/{id:guid}")]
    public async Task<IActionResult> RemoveVote(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.RemoveQuestionVoteAsync(id, userId);

            return Json(new { success = true, message = _localizer["VoteRemoved"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vote from question {QuestionId}", id);
            return Json(new { success = false, message = _localizer["FailedToRemoveVote"].Value });
        }
    }

    // POST: Questions/Bookmark/{id}
    [Authorize]
    [HttpPost("Bookmark/{id:guid}")]
    public async Task<IActionResult> Bookmark(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _questionService.BookmarkQuestionAsync(id, userId);

            return Json(new { success = true, message = result != null ? _localizer["QuestionBookmarked"].Value : _localizer["BookmarkRemoved"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling bookmark for question {QuestionId}", id);
            return Json(new { success = false, message = _localizer["FailedToToggleBookmark"].Value });
        }
    }

    // POST: Questions/AcceptAnswer
    [Authorize]
    [HttpPost("AcceptAnswer")]
    public async Task<IActionResult> AcceptAnswer(Guid questionId, Guid answerId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.AcceptAnswerAsync(questionId, answerId, userId);

            return Json(new { success = true, message = _localizer["AnswerAccepted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting answer {AnswerId} for question {QuestionId}", answerId, questionId);
            return Json(new { success = false, message = _localizer["FailedToAcceptAnswer"].Value });
        }
    }

    // POST: Questions/UnacceptAnswer
    [Authorize]
    [HttpPost("UnacceptAnswer")]
    public async Task<IActionResult> UnacceptAnswer(Guid questionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.UnacceptAnswerAsync(questionId, userId);

            return Json(new { success = true, message = _localizer["AnswerUnaccepted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unaccepting answer for question {QuestionId}", questionId);
            return Json(new { success = false, message = _localizer["FailedToUnacceptAnswer"].Value });
        }
    }

    // GET: Questions/MyQuestions
    [Authorize]
    [HttpGet("MyQuestions")]
    public async Task<IActionResult> MyQuestions(int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _questionService.GetUserQuestionsAsync(userId, parameters);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user questions");
            TempData["Error"] = _localizer["FailedToLoadMyQuestions"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Questions/Bookmarks
    [Authorize]
    [HttpGet("Bookmarks")]
    public async Task<IActionResult> Bookmarks(int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _questionService.GetBookmarkedQuestionsAsync(userId, parameters);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookmarked questions");
            TempData["Error"] = _localizer["FailedToLoadMyBookmarks"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Questions/Search
    [HttpGet("Search")]
    public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 20)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _questionService.GetQuestionsAsync(parameters, tag: query);
            
            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View("Index", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching questions with query {Query}", query);
            TempData["Error"] = _localizer["FailedToSearchQuestions"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(_localizer["UserNotAuthenticated"].Value);
        }

        return userId;
    }
}
