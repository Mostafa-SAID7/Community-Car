using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Qa;

[Route("Answers")]
[Authorize]
public class AnswersController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<AnswersController> _logger;

    public AnswersController(
        IQuestionService questionService,
        ILogger<AnswersController> logger)
    {
        _questionService = questionService;
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
                TempData["Error"] = "Please provide valid answer content";
                return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
            }

            var userId = GetCurrentUserId();
            await _questionService.AddAnswerAsync(model.QuestionId, model.Content, userId);

            TempData["Success"] = "Answer posted successfully";
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
                TempData["Error"] = "Answer not found";
                return RedirectToAction("Index", "Questions");
            }

            if (answer.AuthorId != userId)
            {
                TempData["Error"] = "You can only edit your own answers";
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
            TempData["Error"] = "Failed to load answer";
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
                TempData["Error"] = "Answer not found";
                return RedirectToAction("Index", "Questions");
            }

            if (answer.AuthorId != userId)
            {
                TempData["Error"] = "You can only edit your own answers";
                return RedirectToAction("Details", "Questions", new { id = answer.QuestionId });
            }

            await _questionService.UpdateAnswerAsync(id, model.Content);

            TempData["Success"] = "Answer updated successfully";
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
                return Json(new { success = false, message = "Answer not found" });

            if (answer.AuthorId != userId)
                return Json(new { success = false, message = "You can only delete your own answers" });

            await _questionService.DeleteAnswerAsync(id);

            return Json(new { success = true, message = "Answer deleted successfully" });
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

            return Json(new { success = true, message = "Vote recorded" });
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

            return Json(new { success = true, message = "Vote removed" });
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
                return Json(new { success = false, message = "Comment content is required" });

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
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userId;
    }
}
