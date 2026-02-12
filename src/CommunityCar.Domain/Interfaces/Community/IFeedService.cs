using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Enums.Community.Feed;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IFeedService
{
    Task<FeedResultDto> GetFeedAsync(
        int page = 1,
        int pageSize = 20,
        FeedItemType? contentType = null,
        string? searchTerm = null,
        string? category = null,
        DateFilterType dateFilter = DateFilterType.All,
        FeedSortType sortBy = FeedSortType.Recent,
        Guid? authorId = null,
        string? tag = null,
        bool? isFeatured = null,
        bool followingOnly = false,
        Guid? currentUserId = null);

    Task<FeedResultDto> GetPopularFeedAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null);

    Task<FeedResultDto> GetTrendingFeedAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null);

    Task<FeedItemDto?> GetFeedItemByIdAsync(Guid id, FeedItemType type);
}
