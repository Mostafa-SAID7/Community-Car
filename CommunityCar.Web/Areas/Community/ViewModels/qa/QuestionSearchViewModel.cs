namespace CommunityCar.Web.Areas.Community.ViewModels.qa;

public class QuestionSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Tag { get; set; }
    public bool? IsResolved { get; set; }
    public string SortBy { get; set; } = "created";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
