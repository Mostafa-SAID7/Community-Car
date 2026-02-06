using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Web.Hubs;
using CommunityCar.Web.ViewModels.Community.Qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Web.Controllers;

[Route("Comments")]
[Authorize]
public class CommentsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<QuestionHub> _hubContext;

    public CommentsController(
        IQuestionService questionService,
        ICurrentUserService currentUserService,
        IHubContext<QuestionHub> hubContext)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
        _hubContext = hubContext;
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var comment = await _questionService.AddAnswerCommentAsync(model.AnswerId, model.Content, userId.Value);

        // Real-time broadcast
        await _hubContext.Clients.All.SendAsync("ReceiveComment", new { 
            answerId = model.AnswerId,
            commentId = comment.Id
        });

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Ok(new { success = true, message = "Comment posted successfully." });
        }

        return RedirectToAction("Details", "Questions", new { idOrSlug = model.QuestionId }); // Need QuestionId for redirect. Model binding.
    }

    [HttpGet("GetCommentCard/{id}")]
    public async Task<IActionResult> GetCommentCard(Guid id)
    {
        var comment = await _questionService.GetCommentByIdAsync(id);
        if (comment == null) return NotFound();

        return PartialView("_CommentItem", comment);
    }
}
