using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Web.Areas.Community.ViewModels.qa;

public class QuestionListViewModel
{
    public List<QuestionDto> Questions { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<TagDto> PopularTags { get; set; } = new();
    public Guid? SelectedCategoryId { get; set; }
    public Guid? SelectedTagId { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "recent";
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalQuestions { get; set; }
}
