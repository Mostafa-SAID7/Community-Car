using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Qa;

public class SidebarQAViewModel
{
    public IEnumerable<TagDto> PopularTags { get; set; } = new List<TagDto>();
    public IEnumerable<QuestionDto> HelpNeededQuestions { get; set; } = new List<QuestionDto>();
    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}
