using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.Areas.Community.ViewModels.qa;
using CommunityCar.Domain.Enums.Community.qa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.qa;

[Area("Community")]
[Route("Community/Answers")]
[Authorize]
public class AnswersController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ICurrentUserService _currentUserService;

    public AnswersController(
        IQuestionService questionService,
        ICurrentUserService currentUserService)
    {
        _questionService = questionService;
        _currentUserService = currentUserService;
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnswerViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var answer = await _questionService.AddAnswerAsync(model.QuestionId, model.Content, userId.Value);

        return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();

        var answers = await _questionService.GetAnswersAsync(question.Id);
        var answer = answers.FirstOrDefault(a => a.Id == id);
        
        if (answer == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (answer.AuthorId != userId)
            return Forbid();

        var model = new EditAnswerViewModel
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            Content = answer.Content
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditAnswerViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var question = await _questionService.GetQuestionByIdAsync(model.QuestionId);
        if (question == null)
            return NotFound();

        var answers = await _questionService.GetAnswersAsync(question.Id);
        var answer = answers.FirstOrDefault(a => a.Id == id);
        
        if (answer == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (answer.AuthorId != userId)
            return Forbid();

        await _questionService.UpdateAnswerAsync(id, model.Content);

        return RedirectToAction("Details", "Questions", new { id = model.QuestionId });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid questionId)
    {
        var question = await _questionService.GetQuestionByIdAsync(questionId);
        if (question == null)
            return NotFound();

        var answers = await _questionService.GetAnswersAsync(question.Id);
        var answer = answers.FirstOrDefault(a => a.Id == id);
        
        if (answer == null)
            return NotFound();

        var userId = _currentUserService.UserId;
        if (answer.AuthorId != userId)
            return Forbid();

        await _questionService.DeleteAnswerAsync(id);

        return RedirectToAction("Details", "Questions", new { id = questionId });
    }

    [HttpPost("Vote/{id}")]
    public async Task<IActionResult> Vote(Guid id, bool isUpvote)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.VoteAnswerAsync(id, userId.Value, isUpvote);

        return Ok();
    }

    [HttpPost("RemoveVote/{id}")]
    public async Task<IActionResult> RemoveVote(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveAnswerVoteAsync(id, userId.Value);

        return Ok();
    }
}


    // Reaction actions
    [HttpPost("React/{id}")]
    public async Task<IActionResult> React(Guid id, int reactionType)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.ReactToAnswerAsync(id, userId.Value, (ReactionType)reactionType);

        var summary = await _questionService.GetAnswerReactionSummaryAsync(id, userId.Value);
        return Ok(summary);
    }

    [HttpPost("RemoveReaction/{id}")]
    public async Task<IActionResult> RemoveReaction(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        await _questionService.RemoveAnswerReactionAsync(id, userId.Value);

        var summary = await _questionService.GetAnswerReactionSummaryAsync(id, userId.Value);
        return Ok(summary);
    }

    [HttpGet("Reactions/{id}")]
    public async Task<IActionResult> GetReactions(Guid id)
    {
        var userId = _currentUserService.UserId;
        var summary = await _questionService.GetAnswerReactionSummaryAsync(id, userId);

        return Ok(summary);
    }
}
