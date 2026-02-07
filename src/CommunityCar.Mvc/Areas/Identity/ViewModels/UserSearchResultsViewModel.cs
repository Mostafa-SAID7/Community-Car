using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class UserSearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<ApplicationUser> Users { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
