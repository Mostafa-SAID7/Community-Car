using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.DTOs.Identity;

namespace CommunityCar.Domain.Queries.Identity;

/// <summary>
/// Query to search users with pagination
/// </summary>
public class SearchUsersQuery : IQuery<List<UserSearchDto>>
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public SearchUsersQuery()
    {
    }

    public SearchUsersQuery(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        SearchTerm = searchTerm;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
