using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.events;
using CommunityCar.Domain.Entities.Community.news;
using CommunityCar.Domain.Entities.Community.guides;
using CommunityCar.Domain.Entities.Community.reviews;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Domain.Enums.Community.Feed;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class FeedService : IFeedService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<FeedService> _logger;

    public FeedService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<FeedService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FeedResultDto> GetFeedAsync(
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
        Guid? currentUserId = null)
    {
        var items = new List<FeedItemDto>();
        var totalCount = 0;

        if (contentType.HasValue)
        {
            var (typeItems, typeCount) = await GetItemsByTypeAsync(
                contentType.Value, page, pageSize, searchTerm, category, 
                dateFilter, sortBy, authorId, tag, isFeatured, currentUserId);
            items.AddRange(typeItems);
            totalCount = typeCount;
        }
        else
        {
            items = await GetAllItemsAsync(page, pageSize, searchTerm, category, 
                dateFilter, sortBy, authorId, tag, isFeatured, currentUserId);
            totalCount = items.Count;
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new FeedResultDto
        {
            Items = items,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<FeedResultDto> GetPopularFeedAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null)
    {
        return await GetFeedAsync(
            page: page,
            pageSize: pageSize,
            sortBy: FeedSortType.Popular,
            currentUserId: currentUserId);
    }

    public async Task<FeedResultDto> GetTrendingFeedAsync(int page = 1, int pageSize = 20, Guid? currentUserId = null)
    {
        return await GetFeedAsync(
            page: page,
            pageSize: pageSize,
            sortBy: FeedSortType.Trending,
            currentUserId: currentUserId);
    }

    public async Task<FeedItemDto?> GetFeedItemByIdAsync(Guid id, FeedItemType type)
    {
        return type switch
        {
            FeedItemType.Post => await GetPostItemAsync(id),
            FeedItemType.Question => await GetQuestionItemAsync(id),
            FeedItemType.Event => await GetEventItemAsync(id),
            FeedItemType.News => await GetNewsItemAsync(id),
            FeedItemType.Guide => await GetGuideItemAsync(id),
            FeedItemType.Review => await GetReviewItemAsync(id),
            FeedItemType.Group => await GetGroupItemAsync(id),
            _ => null
        };
    }

    private async Task<(List<FeedItemDto>, int)> GetItemsByTypeAsync(
        FeedItemType type,
        int page,
        int pageSize,
        string? searchTerm,
        string? category,
        DateFilterType dateFilter,
        FeedSortType sortBy,
        Guid? authorId,
        string? tag,
        bool? isFeatured,
        Guid? currentUserId)
    {
        var items = new List<FeedItemDto>();
        var totalCount = 0;

        switch (type)
        {
            case FeedItemType.Post:
                var posts = await GetPostsAsync(page, pageSize, searchTerm, dateFilter, sortBy);
                items.AddRange(posts);
                totalCount = await CountPostsAsync(searchTerm, dateFilter);
                break;
            case FeedItemType.Question:
                var questions = await GetQuestionsAsync(page, pageSize, searchTerm, dateFilter, sortBy);
                items.AddRange(questions);
                totalCount = await CountQuestionsAsync(searchTerm, dateFilter);
                break;
        }

        return (items, totalCount);
    }

    private async Task<List<FeedItemDto>> GetAllItemsAsync(
        int page,
        int pageSize,
        string? searchTerm,
        string? category,
        DateFilterType dateFilter,
        FeedSortType sortBy,
        Guid? authorId,
        string? tag,
        bool? isFeatured,
        Guid? currentUserId)
    {
        var items = new List<FeedItemDto>();
        
        var posts = await GetPostsAsync(1, pageSize, searchTerm, dateFilter, sortBy);
        items.AddRange(posts);

        return items.OrderByDescending(x => x.CreatedAt).Take(pageSize).ToList();
    }

    private async Task<List<FeedItemDto>> GetPostsAsync(
        int page, int pageSize, string? searchTerm, DateFilterType dateFilter, FeedSortType sortBy)
    {
        IQueryable<Post> query = _uow.Repository<Post>().GetQueryable()
            .Include(p => p.Author);

        query = ApplyDateFilter(query, dateFilter, p => p.CreatedAt);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm));
        }

        query = ApplySorting(query, sortBy, 
            p => p.CreatedAt, 
            p => p.ViewCount, 
            p => p.LikeCount);

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return posts.Select(p => new FeedItemDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
            Slug = p.Slug,
            Type = FeedItemType.Post,
            TypeName = "Post",
            TypeIcon = "fa-file-alt",
            TypeColor = "primary",
            AuthorId = p.AuthorId,
            AuthorName = p.Author?.UserName ?? "Unknown",
            AuthorAvatar = p.Author?.ProfilePictureUrl,
            ImageUrl = p.ImageUrl,
            Category = p.Category.ToString(),
            Tags = p.Tags,
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            InteractionCount = p.ViewCount + p.LikeCount + p.CommentCount,
            CreatedAt = p.CreatedAt,
            TimeAgo = GetTimeAgo(p.CreatedAt),
            ActionUrl = $"/Posts/Details/{p.Slug}",
            ActionText = "Read More"
        }).ToList();
    }

    private async Task<List<FeedItemDto>> GetQuestionsAsync(
        int page, int pageSize, string? searchTerm, DateFilterType dateFilter, FeedSortType sortBy)
    {
        IQueryable<Question> query = _uow.Repository<Question>().GetQueryable()
            .Include(q => q.Author);

        query = ApplyDateFilter(query, dateFilter, q => q.CreatedAt);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(q => q.Title.Contains(searchTerm) || q.Content.Contains(searchTerm));
        }

        var questions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return questions.Select(q => new FeedItemDto
        {
            Id = q.Id,
            Title = q.Title,
            Content = q.Content.Length > 200 ? q.Content.Substring(0, 200) + "..." : q.Content,
            Slug = q.Slug,
            Type = FeedItemType.Question,
            TypeName = "Question",
            TypeIcon = "fa-question-circle",
            TypeColor = "info",
            AuthorId = q.AuthorId,
            AuthorName = q.Author?.UserName ?? "Unknown",
            AuthorAvatar = q.Author?.ProfilePictureUrl,
            Category = q.Category?.Name ?? "",
            Tags = q.Tags,
            ViewCount = q.ViewCount,
            LikeCount = q.VoteCount,
            CommentCount = q.AnswerCount,
            InteractionCount = q.ViewCount + q.VoteCount + q.AnswerCount,
            CreatedAt = q.CreatedAt,
            TimeAgo = GetTimeAgo(q.CreatedAt),
            ActionUrl = $"/Questions/Details/{q.Slug}",
            ActionText = "View Question",
            IsResolved = q.IsResolved,
            AnswerCount = q.AnswerCount
        }).ToList();
    }

    private async Task<int> CountPostsAsync(string? searchTerm, DateFilterType dateFilter)
    {
        var query = _uow.Repository<Post>().GetQueryable();
        query = ApplyDateFilter(query, dateFilter, p => p.CreatedAt);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    private async Task<int> CountQuestionsAsync(string? searchTerm, DateFilterType dateFilter)
    {
        var query = _uow.Repository<Question>().GetQueryable();
        query = ApplyDateFilter(query, dateFilter, q => q.CreatedAt);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(q => q.Title.Contains(searchTerm) || q.Content.Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    private async Task<FeedItemDto?> GetPostItemAsync(Guid id)
    {
        var post = await _uow.Repository<Post>().GetQueryable()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return null;

        return new FeedItemDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Slug = post.Slug,
            Type = FeedItemType.Post,
            TypeName = "Post",
            TypeIcon = "fa-file-alt",
            TypeColor = "primary",
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.UserName ?? "Unknown",
            AuthorAvatar = post.Author?.ProfilePictureUrl,
            ImageUrl = post.ImageUrl,
            Category = post.Category.ToString(),
            Tags = post.Tags,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            CreatedAt = post.CreatedAt,
            TimeAgo = GetTimeAgo(post.CreatedAt),
            ActionUrl = $"/Posts/Details/{post.Slug}"
        };
    }

    private async Task<FeedItemDto?> GetQuestionItemAsync(Guid id)
    {
        var question = await _uow.Repository<Question>().GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null) return null;

        return new FeedItemDto
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            Slug = question.Slug,
            Type = FeedItemType.Question,
            TypeName = "Question",
            TypeIcon = "fa-question-circle",
            TypeColor = "info",
            AuthorId = question.AuthorId,
            AuthorName = question.Author?.UserName ?? "Unknown",
            AuthorAvatar = question.Author?.ProfilePictureUrl,
            Category = question.Category?.Name ?? "",
            Tags = question.Tags,
            ViewCount = question.ViewCount,
            LikeCount = question.VoteCount,
            CommentCount = question.AnswerCount,
            CreatedAt = question.CreatedAt,
            TimeAgo = GetTimeAgo(question.CreatedAt),
            ActionUrl = $"/Questions/Details/{question.Slug}",
            IsResolved = question.IsResolved,
            AnswerCount = question.AnswerCount
        };
    }

    private Task<FeedItemDto?> GetEventItemAsync(Guid id) => Task.FromResult<FeedItemDto?>(null);
    private Task<FeedItemDto?> GetNewsItemAsync(Guid id) => Task.FromResult<FeedItemDto?>(null);
    private Task<FeedItemDto?> GetGuideItemAsync(Guid id) => Task.FromResult<FeedItemDto?>(null);
    private Task<FeedItemDto?> GetReviewItemAsync(Guid id) => Task.FromResult<FeedItemDto?>(null);
    private Task<FeedItemDto?> GetGroupItemAsync(Guid id) => Task.FromResult<FeedItemDto?>(null);

    private IQueryable<T> ApplyDateFilter<T>(
        IQueryable<T> query, 
        DateFilterType dateFilter, 
        System.Linq.Expressions.Expression<Func<T, DateTimeOffset>> dateSelector)
    {
        var now = DateTimeOffset.UtcNow;
        
        return dateFilter switch
        {
            DateFilterType.Today => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name).Date == now.Date),
            DateFilterType.ThisWeek => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name) >= now.AddDays(-7)),
            DateFilterType.ThisMonth => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name) >= now.AddDays(-30)),
            DateFilterType.Last24Hours => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name) >= now.AddHours(-24)),
            DateFilterType.Last7Days => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name) >= now.AddDays(-7)),
            DateFilterType.Last30Days => query.Where(x => EF.Property<DateTimeOffset>(x, ((System.Linq.Expressions.MemberExpression)dateSelector.Body).Member.Name) >= now.AddDays(-30)),
            _ => query
        };
    }

    private IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        FeedSortType sortBy,
        System.Linq.Expressions.Expression<Func<T, DateTimeOffset>> dateSelector,
        System.Linq.Expressions.Expression<Func<T, int>> viewSelector,
        System.Linq.Expressions.Expression<Func<T, int>> likeSelector)
    {
        return sortBy switch
        {
            FeedSortType.Recent => query.OrderByDescending(dateSelector),
            FeedSortType.Popular => query.OrderByDescending(likeSelector),
            FeedSortType.MostViewed => query.OrderByDescending(viewSelector),
            FeedSortType.MostLiked => query.OrderByDescending(likeSelector),
            FeedSortType.Trending => query.OrderByDescending(viewSelector).ThenByDescending(likeSelector),
            _ => query.OrderByDescending(dateSelector)
        };
    }

    private string GetTimeAgo(DateTimeOffset date)
    {
        var timeSpan = DateTimeOffset.UtcNow - date;

        if (timeSpan.TotalMinutes < 1) return "just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
        if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
        return $"{(int)(timeSpan.TotalDays / 365)}y ago";
    }
}
