using System.Collections.Generic;

namespace CommunityCar.Mvc.ViewModels.Search;

public class GlobalSearchViewModel
{
    public string? Query { get; set; }
    public List<SearchResultItemViewModel> Results { get; set; } = new();
    public int TotalResults => Results.Count;
}

public class SearchResultItemViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string Type { get; set; } // Post, Question, Group, Event, User
    public string Icon { get; set; }
    public string ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
