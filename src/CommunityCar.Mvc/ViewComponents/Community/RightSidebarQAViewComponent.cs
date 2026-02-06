using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Qa;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.ViewComponents.Community;

public class RightSidebarQAViewComponent : ViewComponent
{
    private readonly IQuestionService _questionService;
    private readonly ITagService _tagService;

    public RightSidebarQAViewComponent(IQuestionService questionService, ITagService tagService)
    {
        _questionService = questionService;
        _tagService = tagService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var popularTagsResult = await _tagService.GetPopularTagsAsync(10);
        
        // Help Needed = Recent Questions without any answers
        var helpNeededPageResult = await _questionService.GetQuestionsAsync(new QueryParameters { PageNumber = 1, PageSize = 5 }, hasAnswers: false);

        var viewModel = new SidebarQAViewModel
        {
            PopularTags = popularTagsResult.IsSuccess ? popularTagsResult.Value : new List<CommunityCar.Domain.DTOs.Community.TagDto>(),
            HelpNeededQuestions = helpNeededPageResult.Items ?? new List<CommunityCar.Domain.DTOs.Community.QuestionDto>()
        };

        return View(viewModel);
    }
}
