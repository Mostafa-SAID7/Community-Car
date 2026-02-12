namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

/// <summary>
/// ViewModel for user search results
/// </summary>
public class UserSearchViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Slug { get; set; }
}
