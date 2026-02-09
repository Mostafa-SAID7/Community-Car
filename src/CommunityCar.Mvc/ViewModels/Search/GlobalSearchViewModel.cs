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
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Url { get; set; }
    public required string Type { get; set; } // Post, Question, Group, Event, User
    public required string Icon { get; set; }
    public required string ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
