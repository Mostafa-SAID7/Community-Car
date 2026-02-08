using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Qa;

[Route("{culture:alpha}/Answers")]
[Authorize]
public class AnswersController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IStringLocalizer<AnswersController> _localizer;
    private readonly ILogger<AnswersController> _logger;

    public AnswersController(
        IQuestionService questionService,
        IStringLocalizer<AnswersController> localizer,
        ILogger<AnswersController> logger)
    {
        _questionService = questionService;
        _localizer = localizer;
        _logger = logger;
    }

    // POST: Answers/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnswerViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = _localizer["ProvideValidContent"].Value;
                return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
            }

            var userId = GetCurrentUserId();
            await _questionService.AddAnswerAsync(model.QuestionId, model.Content, userId);

            TempData["Success"] = _localizer["AnswerPostedSuccessfully"].Value;
            return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating answer for question {QuestionId}", model.QuestionId);
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
        }
    }

    // GET: Answers/Edit/{id}
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var answer = await _questionService.GetAnswerByIdAsync(id, userId);
            
            if (answer == null)
            {
                TempData["Error"] = _localizer["AnswerNotFound"].Value;
                return RedirectToAction("Index", "Questions");
            }

            if (answer.AuthorId != userId)
            {
                TempData["Error"] = _localizer["CannotEditOthersAnswer"].Value;
                return RedirectToAction("Details", "Questions", new { id = answer.QuestionId });
            }

            var model = new EditAnswerViewModel
            {
                Id = answer.Id,
                QuestionId = answer.QuestionId,
                Content = answer.Content
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading answer for edit {AnswerId}", id);
            TempData["Error"] = _localizer["FailedToLoadAnswer"].Value;
            return RedirectToAction("Index", "Questions");
        }
    }

    // POST: Answers/Edit/{id}
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditAnswerViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();
            var answer = await _questionService.GetAnswerByIdAsync(id, userId);
            
            if (answer == null)
            {
                TempData["Error"] = _localizer["AnswerNotFound"].Value;
                return RedirectToAction("Index", "Questions");
            }

            if (answer.AuthorId != userId)
            {
                TempData["Error"] = _localizer["CannotEditOthersAnswer"].Value;
                return RedirectToAction("Details", "Questions", new { id = answer.QuestionId });
            }

            await _questionService.UpdateAnswerAsync(id, model.Content);

            TempData["Success"] = _localizer["AnswerUpdatedSuccessfully"].Value;
            return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating answer {AnswerId}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: Answers/Delete/{id}
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid questionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var answer = await _questionService.GetAnswerByIdAsync(id, userId);
            
            if (answer == null)
                return Json(new { success = false, message = _localizer["AnswerNotFound"].Value });

            if (answer.AuthorId != userId)
                return Json(new { success = false, message = _localizer["CannotDeleteOthersAnswer"].Value });

            await _questionService.DeleteAnswerAsync(id);

            return Json(new { success = true, message = _localizer["AnswerDeletedSuccessfully"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting answer {AnswerId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Answers/Vote/{id}
    [HttpPost("Vote/{id:guid}")]
    public async Task<IActionResult> Vote(Guid id, bool isUpvote)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.VoteAnswerAsync(id, userId, isUpvote);

            return Json(new { success = true, message = _localizer["VoteRecorded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on answer {AnswerId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Answers/RemoveVote/{id}
    [HttpPost("RemoveVote/{id:guid}")]
    public async Task<IActionResult> RemoveVote(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _questionService.RemoveAnswerVoteAsync(id, userId);

            return Json(new { success = true, message = _localizer["VoteRemoved"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vote from answer {AnswerId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Answers/AddComment
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid answerId, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, message = _localizer["CommentRequired"].Value });

            var userId = GetCurrentUserId();
            var comment = await _questionService.AddAnswerCommentAsync(answerId, content, userId);

            return Json(new { success = true, comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to answer {AnswerId}", answerId);
            return Json(new { success = false, message = ex.Message });
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
