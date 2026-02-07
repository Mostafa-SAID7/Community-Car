using CommunityCar.Domain.Base;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Qa;

[Route("Questions")]
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService,
        ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // GET: Questions
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? tag = null, bool? isResolved = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _questionService.GetQuestionsAsync(parameters, searchTerm: null, tag: tag, isResolved: isResolved);
            
            ViewBag.CurrentTag = tag;
            ViewBag.IsResolved = isResolved;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading questions");
            TempData["Error"] = "Failed to load questions";
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
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                TempData["Error"] = "Question not found";
                return RedirectToAction(nameof(Index));
            }

            await _questionService.IncrementViewCountAsync(id);
            
            var answers = await _questionService.GetAnswersAsync(id);
            
            ViewBag.Answers = answers;
            ViewBag.CurrentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            
            return View(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question {QuestionId}", id);
            TempData["Error"] = "Failed to load question";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Questions/Create
    [Authorize]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = new CreateQuestionViewModel
            {
                Title = string.Empty,
                Content = string.Empty,
                Tags = string.Empty,
                CategoryId = null,
                Categories = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create question page");
            TempData["Error"] = "Failed to load create question page";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Questions/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Ensure Categories is initialized before returning to view
                model.Categories ??= new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                return View(model);
            }

            var userId = GetCurrentUserId();

            var question = await _questionService.CreateQuestionAsync(
                model.Title, 
                model.Content, 
                userId, 
                categoryId: null,
                tags: model.Tags);

            TempData["Success"] = "Question created successfully";
            return RedirectToAction(nameof(Details), new { id = question.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            ModelState.AddModelError("", "Failed to create question");
            model.Categories ??= new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
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
                TempData["Error"] = "Question not found";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
            {
                TempData["Error"] = "You can only edit your own questions";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new EditQuestionViewModel
            {
                Id = question.Id,
                Title = question.Title,
                Content = question.Content,
                Tags = question.Tags,
                CategoryId = null,
                Categories = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question for edit {QuestionId}", id);
            TempData["Error"] = "Failed to load question";
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
                // Ensure Categories is initialized before returning to view
                model.Categories ??= new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                return View(model);
            }

            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
            {
                TempData["Error"] = "Question not found";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
            {
                TempData["Error"] = "You can only edit your own questions";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _questionService.UpdateQuestionAsync(id, model.Title, model.Content, categoryId: null, tags: model.Tags);

            TempData["Success"] = "Question updated successfully";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId}", id);
            ModelState.AddModelError("", "Failed to update question");
            model.Categories ??= new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
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
                return Json(new { success = false, message = "Question not found" });

            var userId = GetCurrentUserId();
            if (question.AuthorId != userId)
                return Json(new { success = false, message = "You can only delete your own questions" });

            await _questionService.DeleteQuestionAsync(id);

            return Json(new { success = true, message = "Question deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {QuestionId}", id);
            return Json(new { success = false, message = "Failed to delete question" });
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

            return Json(new { success = true, message = "Vote recorded" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on question {QuestionId}", id);
            return Json(new { success = false, message = "Failed to vote" });
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

            return Json(new { success = true, message = "Vote removed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vote from question {QuestionId}", id);
            return Json(new { success = false, message = "Failed to remove vote" });
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

            return Json(new { success = true, message = "Answer accepted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting answer {AnswerId} for question {QuestionId}", answerId, questionId);
            return Json(new { success = false, message = "Failed to accept answer" });
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

            return Json(new { success = true, message = "Answer unaccepted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unaccepting answer for question {QuestionId}", questionId);
            return Json(new { success = false, message = "Failed to unaccept answer" });
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
            TempData["Error"] = "Failed to load your questions";
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
            TempData["Error"] = "Failed to search questions";
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userId;
    }
}
