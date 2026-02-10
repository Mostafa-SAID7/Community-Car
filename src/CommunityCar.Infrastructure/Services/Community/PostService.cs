using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Commands.Community;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class PostService : IPostService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PostService> _logger;
    private readonly ICommandHandler<LikePostCommand, LikePostResult> _likePostHandler;

    public PostService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<PostService> logger,
        ICommandHandler<LikePostCommand, LikePostResult> likePostHandler)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _likePostHandler = likePostHandler;
    }

    public async Task<Post> CreatePostAsync(
        string title,
        string content,
        PostType type,
        Guid authorId,
        PostCategory category = PostCategory.General,
        Guid? groupId = null,
        PostStatus status = PostStatus.Draft)
    {
        var post = new Post(title, content, type, authorId, category, groupId, status);
        
        await _uow.Repository<Post>().AddAsync(post);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post created: {PostId} by user {UserId} with status {Status} and category {Category}", post.Id, authorId, status, category);
        return post;
    }

    public async Task<Post> UpdatePostAsync(
        Guid postId,
        string title,
        string content,
        PostType type,
        PostCategory? category = null,
        PostStatus? status = null)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Update(title, content, type, category, status);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post updated: {PostId} with status {Status} and category {Category}", postId, status, category);
        return post;
    }

    public async Task DeletePostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        _uow.Repository<Post>().Delete(post);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post deleted: {PostId}", postId);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid postId, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Post>().GetQueryable();
        var post = await query
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            return null;

        return MapToDto(post, currentUserId);
    }

    public async Task<PostDto?> GetPostBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Post>().GetQueryable();
        var post = await query
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post == null)
            return null;

        return MapToDto(post, currentUserId);
    }

    public async Task<PagedResult<PostDto>> GetPostsAsync(
        QueryParameters parameters,
        PostStatus? status = null,
        PostType? type = null,
        Guid? groupId = null,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Post>().GetQueryable()
            .Include(p => p.Author)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        else
            query = query.Where(p => p.Status == PostStatus.Published);

        if (type.HasValue)
            query = query.Where(p => p.Type == type.Value);

        if (groupId.HasValue)
            query = query.Where(p => p.GroupId == groupId.Value);

        // Pinned posts first, then by published date
        query = query.OrderByDescending(p => p.IsPinned)
                     .ThenByDescending(p => p.PublishedAt ?? p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(p => MapToDto(p, currentUserId)).ToList();

        return new PagedResult<PostDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<PostDto>> GetUserPostsAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Post>().GetQueryable()
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(p => MapToDto(p, currentUserId)).ToList();

        return new PagedResult<PostDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<List<PostDto>> GetFeaturedPostsAsync(int count = 5)
    {
        var query = _uow.Repository<Post>().GetQueryable();
        var posts = await query
            .Include(p => p.Author)
            .Where(p => p.Status == PostStatus.Published && p.IsPinned)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return posts.Select(p => MapToDto(p, null)).ToList();
    }

    public async Task<List<PostDto>> GetLatestPostsAsync(int count = 10)
    {
        var query = _uow.Repository<Post>().GetQueryable();
        var posts = await query
            .Include(p => p.Author)
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return posts.Select(p => MapToDto(p, null)).ToList();
    }

    public async Task PublishPostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Publish();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post published: {PostId}", postId);
    }

    public async Task ArchivePostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Archive();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post archived: {PostId}", postId);
    }

    public async Task PinPostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Pin();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post pinned: {PostId}", postId);
    }

    public async Task UnpinPostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Unpin();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post unpinned: {PostId}", postId);
    }

    public async Task LockPostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Lock();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post locked: {PostId}", postId);
    }

    public async Task UnlockPostAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.Unlock();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Post unlocked: {PostId}", postId);
    }

    public async Task IncrementViewsAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.IncrementViews();
        await _uow.SaveChangesAsync();
    }

    public async Task<LikePostResult> ToggleLikeAsync(Guid postId, Guid userId)
    {
        var command = new LikePostCommand(postId, userId);
        var result = await _likePostHandler.HandleAsync(command);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(result.Error);
        }

        _logger.LogInformation(
            "Post {PostId} like toggled by user {UserId}. IsLiked: {IsLiked}, TotalLikes: {TotalLikes}", 
            postId, 
            userId, 
            result.Value.IsLiked, 
            result.Value.TotalLikes);

        return result.Value;
    }

    public async Task IncrementSharesAsync(Guid postId)
    {
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new NotFoundException("Post not found");

        post.IncrementShares();
        await _uow.SaveChangesAsync();
    }

    public async Task<List<PostLikerDto>> GetPostLikersAsync(Guid postId, QueryParameters parameters)
    {
        var query = _uow.Repository<PostReaction>().GetQueryable();
        var likes = await query
            .Where(l => l.PostId == postId && l.ReactionType == Domain.Enums.Community.qa.ReactionType.Like)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(l => new PostLikerDto
            {
                UserId = l.UserId,
                UserName = l.User.UserName ?? "Unknown",
                UserAvatar = l.User.ProfilePictureUrl,
                LikedAt = l.CreatedAt
            })
            .ToListAsync();

        return likes;
    }


    public async Task<PostComment> AddCommentAsync(
        Guid postId,
        Guid userId,
        string content,
        Guid? parentCommentId = null)
    {
        var comment = new PostComment(postId, userId, content, parentCommentId);
        
        await _uow.Repository<PostComment>().AddAsync(comment);
        
        // Increment comment count on post
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post != null)
        {
            post.IncrementComments();
        }
        
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment added to post {PostId} by user {UserId}", postId, userId);
        return comment;
    }

    public async Task<PostComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _uow.Repository<PostComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Update(content);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated", commentId);
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _uow.Repository<PostComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        // Decrement comment count on post
        var post = await _uow.Repository<Post>()
            .FirstOrDefaultAsync(p => p.Id == comment.PostId);
        if (post != null)
        {
            post.DecrementComments();
        }

        _uow.Repository<PostComment>().Delete(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted", commentId);
    }

    public async Task<PagedResult<PostCommentDto>> GetPostCommentsAsync(
        Guid postId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<PostComment>().GetQueryable()
            .Include(c => c.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new PostCommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            UserId = c.UserId,
            UserName = c.User.UserName ?? "Unknown",
            UserAvatar = c.User.ProfilePictureUrl,
            Content = c.Content,
            LikeCount = c.LikeCount,
            ParentCommentId = c.ParentCommentId,
            ReplyCount = c.Replies.Count,
            IsAuthor = currentUserId.HasValue && c.UserId == currentUserId.Value,
            HasLiked = false, // TODO: Implement proper like tracking
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.ModifiedAt
        }).ToList();

        return new PagedResult<PostCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private PostDto MapToDto(Post post, Guid? currentUserId)
    {
        // In a real implementation, check if user has liked
        var isLiked = false;

        return new PostDto
        {
            Id = post.Id,
            Slug = post.Slug,
            Title = post.Title,
            Content = post.Content,
            Type = post.Type,
            TypeName = post.Type.ToString(),
            Status = post.Status,
            StatusName = post.Status.ToString(),
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.UserName ?? "Unknown",
            AuthorAvatar = post.Author?.ProfilePictureUrl,
            GroupId = post.GroupId,
            ImageUrl = post.ImageUrl,
            VideoUrl = post.VideoUrl,
            LinkUrl = post.LinkUrl,
            LinkTitle = post.LinkTitle,
            LinkDescription = post.LinkDescription,
            Tags = post.Tags,
            IsPinned = post.IsPinned,
            IsFeatured = post.IsPinned, // Using IsPinned as featured indicator
            IsLocked = post.IsLocked,
            AllowComments = !post.IsLocked,
            PublishedAt = post.PublishedAt,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            ShareCount = post.ShareCount,
            CommentCount = post.CommentCount,
            IsAuthor = currentUserId.HasValue && post.AuthorId == currentUserId.Value,
            IsLiked = isLiked,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.ModifiedAt
        };
    }
    
    public async Task SaveChangesAsync()
    {
        await _uow.SaveChangesAsync();
    }
}
