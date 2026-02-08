using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Mvc.Hubs;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Qa;

[Route("{culture:alpha}/Comments")]
[Authorize]
public class CommentsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<QuestionHub> _hubContext;
    private readonly IStringLocalizer<CommentsController> _localizer;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService,
        IHubContext<QuestionHub> hubContext,
        IStringLocalizer<CommentsController> localizer,
        ILogger<CommentsController> logger)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
        _hubContext = hubContext;
        _localizer = localizer;
        _logger = logger;
    }

    // POST: Comments/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { success = false, message = _localizer["InvalidCommentData"].Value });
                }
                TempData["Error"] = _localizer["ProvideValidContent"].Value;
                return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
            }

            var userId = GetCurrentUserId();
            var comment = await _questionService.AddAnswerCommentAsync(model.AnswerId, model.Content, userId);

            // Real-time broadcast with full comment data
            await _hubContext.Clients.All.SendAsync("ReceiveComment", new { 
                answerId = model.AnswerId,
                comment = new
                {
                    comment.Id,
                    comment.Content,
                    comment.AuthorId,
                    comment.AuthorName,
                    comment.CreatedAt
                }
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(new { 
                    success = true, 
                    message = _localizer["CommentPostedSuccessfully"].Value,
                    comment = comment
                });
            }

            TempData["Success"] = _localizer["CommentPostedSuccessfully"].Value;
            return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for answer {AnswerId}", model.AnswerId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return StatusCode(500, new { success = false, message = _localizer["FailedToPostComment"].Value });
            }

            TempData["Error"] = _localizer["FailedToPostComment"].Value;
            return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
        }
    }

    // GET: Comments/GetCommentCard/{id}
    [HttpGet("GetCommentCard/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommentCard(Guid id)
    {
        try
        {
            var comment = await _questionService.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound();

            return PartialView("_CommentItem", comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comment card {CommentId}", id);
            return StatusCode(500);
        }
    }

    // GET: Comments/GetComments/{answerId}
    [HttpGet("GetComments/{answerId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(Guid answerId)
    {
        try
        {
            var comments = await _questionService.GetAnswerCommentsAsync(answerId);
            return PartialView("_CommentList", comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comments for answer {AnswerId}", answerId);
            return StatusCode(500);
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(_localizer["UserNotAuthenticated"].Value);
        }

        return userId;
    }

    #endregion
}
