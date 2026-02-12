using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Questions;
using AutoMapper;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture}/[controller]")]
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IQuestionService questionService,
        IMapper mapper,
        ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _mapper = mapper;
        _logger = logger;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1, string? search = null, string? tag = null, string? sortBy = null, bool sortDesc = true)
    {
        try
        {
            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20,
                SearchTerm = search,
                SortBy = sortBy ?? "created",
                SortDescending = sortDesc
            };

            var currentUserId = GetCurrentUserId();
            var result = await _questionService.GetQuestionsAsync(parameters, search, tag, null, null, null, currentUserId);
            
            ViewBag.SearchTerm = search;
            ViewBag.Tag = tag;
            ViewBag.SortBy = sortBy;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }

    [HttpGet("Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var questionDto = await _questionService.GetQuestionBySlugAsync(slug, currentUserId);

                if (questionDto == null)
                {
                    return NotFound();
                }

                await _questionService.IncrementViewCountAsync(questionDto.Id);

                var answersDto = await _questionService.GetAnswersAsync(questionDto.Id, currentUserId);
                var relatedDto = await _questionService.GetRelatedQuestionsAsync(questionDto.Id, 4);

                var viewModel = new CommunityCar.Mvc.ViewModels.Qa.QuestionDetailsViewModel
                {
                    Question = _mapper.Map<CommunityCar.Mvc.ViewModels.Qa.QuestionViewModel>(questionDto),
                    Answers = _mapper.Map<List<CommunityCar.Mvc.ViewModels.Qa.AnswerViewModel>>(answersDto),
                    RelatedQuestions = _mapper.Map<List<CommunityCar.Mvc.ViewModels.Qa.QuestionViewModel>>(relatedDto)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading question details for slug: {Slug}", slug);
                return NotFound();
            }
        }


    [HttpGet("Create")]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var categories = await _questionService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        return View(new CreateQuestionViewModel());
    }

    [HttpPost("Create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _questionService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var question = await _questionService.CreateQuestionAsync(
                model.Title,
                model.Content,
                currentUserId.Value,
                model.CategoryId,
                model.Tags
            );

            TempData["SuccessMessage"] = "Question created successfully!";
            return RedirectToAction(nameof(Details), new { slug = question.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            ModelState.AddModelError("", ex.Message);
            
            var categories = await _questionService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var questionDto = await _questionService.GetQuestionByIdAsync(id, currentUserId);
            
            if (questionDto == null)
            {
                return NotFound();
            }

            if (currentUserId.HasValue && questionDto.AuthorId != currentUserId.Value)
            {
                return Forbid();
            }

            var categories = await _questionService.GetCategoriesAsync();
            var viewModel = _mapper.Map<EditQuestionViewModel>(questionDto);
            ViewBag.Categories = categories;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading question for edit: {Id}", id);
            return NotFound();
        }
    }

    [HttpPost("Edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditQuestionViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var categories = await _questionService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var questionDto = await _questionService.GetQuestionByIdAsync(id, currentUserId);
            if (questionDto == null)
            {
                return NotFound();
            }

            if (questionDto.AuthorId != currentUserId.Value)
            {
                return Forbid();
            }

            var question = await _questionService.UpdateQuestionAsync(
                model.Id,
                model.Title,
                model.Content,
                model.CategoryId,
                model.Tags
            );

            TempData["SuccessMessage"] = "Question updated successfully!";
            return RedirectToAction(nameof(Details), new { slug = question.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question");
            ModelState.AddModelError("", ex.Message);
            
            var categories = await _questionService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var questionDto = await _questionService.GetQuestionByIdAsync(id, currentUserId);
            if (questionDto == null)
            {
                return NotFound();
            }

            if (questionDto.AuthorId != currentUserId.Value)
            {
                return Forbid();
            }

            await _questionService.DeleteQuestionAsync(id);
            TempData["SuccessMessage"] = "Question deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question: {Id}", id);
            TempData["ErrorMessage"] = "Failed to delete question.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet("MyQuestions")]
    [Authorize]
    public async Task<IActionResult> MyQuestions(int page = 1)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20
            };

            var result = await _questionService.GetUserQuestionsAsync(currentUserId.Value, parameters);
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }

    [HttpGet("Bookmarks")]
    [Authorize]
    public async Task<IActionResult> Bookmarks(int page = 1)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20
            };

            var result = await _questionService.GetBookmarkedQuestionsAsync(currentUserId.Value, parameters);
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookmarked questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }

    [HttpPost("AddAnswer")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswer(Guid questionId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Answer content is required.";
            return RedirectToAction(nameof(Details), new { id = questionId });
        }

        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            await _questionService.AddAnswerAsync(questionId, content, currentUserId.Value);
            TempData["SuccessMessage"] = "Answer added successfully!";
            
            var question = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            return RedirectToAction(nameof(Details), new { slug = question?.Slug ?? questionId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding answer");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }

    [HttpPost("EditAnswer")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAnswer(Guid answerId, Guid questionId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Answer content is required.";
            return RedirectToAction(nameof(Details), new { id = questionId });
        }

        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var answerDto = await _questionService.GetAnswerByIdAsync(answerId, currentUserId);
            if (answerDto == null)
            {
                return NotFound();
            }

            if (answerDto.AuthorId != currentUserId.Value)
            {
                return Forbid();
            }

            await _questionService.UpdateAnswerAsync(answerId, content);
            TempData["SuccessMessage"] = "Answer updated successfully!";
            
            var question = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            return RedirectToAction(nameof(Details), new { slug = question?.Slug ?? questionId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating answer");
            TempData["ErrorMessage"] = "Failed to update answer.";
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }

    [HttpPost("DeleteAnswer")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswer(Guid answerId, Guid questionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var answerDto = await _questionService.GetAnswerByIdAsync(answerId, currentUserId);
            if (answerDto == null)
            {
                return NotFound();
            }

            if (answerDto.AuthorId != currentUserId.Value)
            {
                return Forbid();
            }

            await _questionService.DeleteAnswerAsync(answerId);
            TempData["SuccessMessage"] = "Answer deleted successfully!";
            
            var question = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            return RedirectToAction(nameof(Details), new { slug = question?.Slug ?? questionId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting answer");
            TempData["ErrorMessage"] = "Failed to delete answer.";
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }

    [HttpPost("AcceptAnswer")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptAnswer(Guid questionId, Guid answerId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            await _questionService.AcceptAnswerAsync(questionId, answerId, currentUserId.Value);
            TempData["SuccessMessage"] = "Answer accepted!";
            
            var question = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            return RedirectToAction(nameof(Details), new { slug = question?.Slug ?? questionId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting answer");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }

    [HttpPost("UnacceptAnswer")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnacceptAnswer(Guid questionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            await _questionService.UnacceptAnswerAsync(questionId, currentUserId.Value);
            TempData["SuccessMessage"] = "Answer unaccepted!";
            
            var question = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            return RedirectToAction(nameof(Details), new { slug = question?.Slug ?? questionId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unaccepting answer");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = questionId });
        }
    }

    [HttpPost("VoteQuestion")]
    [Authorize]
    public async Task<IActionResult> VoteQuestion(Guid questionId, bool isUpvote)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _questionService.VoteQuestionAsync(questionId, currentUserId.Value, isUpvote);
            var questionDto = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            
            return Json(new { success = true, voteCount = questionDto?.VoteCount ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on question");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("RemoveQuestionVote")]
    [Authorize]
    public async Task<IActionResult> RemoveQuestionVote(Guid questionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _questionService.RemoveQuestionVoteAsync(questionId, currentUserId.Value);
            var questionDto = await _questionService.GetQuestionByIdAsync(questionId, currentUserId);
            
            return Json(new { success = true, voteCount = questionDto?.VoteCount ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing question vote");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("VoteAnswer")]
    [Authorize]
    public async Task<IActionResult> VoteAnswer(Guid answerId, bool isUpvote)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _questionService.VoteAnswerAsync(answerId, currentUserId.Value, isUpvote);
            var answerDto = await _questionService.GetAnswerByIdAsync(answerId, currentUserId);
            
            return Json(new { success = true, voteCount = answerDto?.VoteCount ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on answer");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("RemoveAnswerVote")]
    [Authorize]
    public async Task<IActionResult> RemoveAnswerVote(Guid answerId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _questionService.RemoveAnswerVoteAsync(answerId, currentUserId.Value);
            var answerDto = await _questionService.GetAnswerByIdAsync(answerId, currentUserId);
            
            return Json(new { success = true, voteCount = answerDto?.VoteCount ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing answer vote");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("BookmarkQuestion")]
    [Authorize]
    public async Task<IActionResult> BookmarkQuestion(Guid questionId, string? notes = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var bookmark = await _questionService.BookmarkQuestionAsync(questionId, currentUserId.Value, notes);
            return Json(new { success = true, isBookmarked = bookmark != null });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bookmarking question");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("RemoveBookmark")]
    [Authorize]
    public async Task<IActionResult> RemoveBookmark(Guid questionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _questionService.RemoveBookmarkAsync(questionId, currentUserId.Value);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing bookmark");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("AddComment")]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid answerId, string content)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var comment = await _questionService.AddAnswerCommentAsync(answerId, content, currentUserId.Value);
            return Json(new { success = true, comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("Trending")]
    public async Task<IActionResult> Trending(int page = 1, string? search = null, string? tag = null)
    {
        try
        {
            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20
            };

            var currentUserId = GetCurrentUserId();
            var result = await _questionService.GetTrendingQuestionsAsync(parameters, search, tag, currentUserId);
            
            ViewBag.SearchTerm = search;
            ViewBag.Tag = tag;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trending questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }

    [HttpGet("Recent")]
    public async Task<IActionResult> Recent(int page = 1, string? search = null, string? tag = null)
    {
        try
        {
            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20
            };

            var currentUserId = GetCurrentUserId();
            var result = await _questionService.GetRecentQuestionsAsync(parameters, search, tag, currentUserId);
            
            ViewBag.SearchTerm = search;
            ViewBag.Tag = tag;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }

    [HttpGet("Suggested")]
    [Authorize]
    public async Task<IActionResult> Suggested(int page = 1)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            var parameters = new QueryParameters
            {
                PageNumber = page,
                PageSize = 20
            };

            var result = await _questionService.GetSuggestedQuestionsAsync(currentUserId.Value, parameters);
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading suggested questions");
            return View(new PagedResult<Domain.DTOs.Community.QuestionDto>());
        }
    }
}
