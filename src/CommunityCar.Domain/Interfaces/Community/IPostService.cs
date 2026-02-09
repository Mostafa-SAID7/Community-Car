using CommunityCar.Domain.Base;
using CommunityCar.Domain.Commands.Community;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Enums.Community.post;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IPostService
{
    // CRUD Operations
    Task<Post> CreatePostAsync(string title, string content, PostType type, Guid authorId, Guid? groupId = null, PostStatus status = PostStatus.Draft);
    Task<Post> UpdatePostAsync(Guid postId, string title, string content, PostType type, PostStatus? status = null);
    Task DeletePostAsync(Guid postId);
    
    // Query Operations
    Task<PostDto?> GetPostByIdAsync(Guid postId, Guid? currentUserId = null);
    Task<PostDto?> GetPostBySlugAsync(string slug, Guid? currentUserId = null);
    Task<PagedResult<PostDto>> GetPostsAsync(QueryParameters parameters, PostStatus? status = null, PostType? type = null, Guid? groupId = null, Guid? currentUserId = null);
    Task<PagedResult<PostDto>> GetUserPostsAsync(Guid userId, QueryParameters parameters, Guid? currentUserId = null);
    Task<List<PostDto>> GetFeaturedPostsAsync(int count = 5);
    Task<List<PostDto>> GetLatestPostsAsync(int count = 10);
    
    // Status Management
    Task PublishPostAsync(Guid postId);
    Task ArchivePostAsync(Guid postId);
    Task PinPostAsync(Guid postId);
    Task UnpinPostAsync(Guid postId);
    Task LockPostAsync(Guid postId);
    Task UnlockPostAsync(Guid postId);
    
    // Engagement
    Task IncrementViewsAsync(Guid postId);
    Task<LikePostResult> ToggleLikeAsync(Guid postId, Guid userId);
    Task IncrementSharesAsync(Guid postId);
    
    // Comments
    Task<PostComment> AddCommentAsync(Guid postId, Guid userId, string content, Guid? parentCommentId = null);
    Task<PostComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
    Task<PagedResult<PostCommentDto>> GetPostCommentsAsync(Guid postId, QueryParameters parameters, Guid? currentUserId = null);
    
    // Persistence
    Task SaveChangesAsync();
}
