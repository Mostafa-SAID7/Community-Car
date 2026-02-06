using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Domain.Interfaces.Community;

public interface ITagService
{
    Task<Result<IEnumerable<TagDto>>> GetAllTagsAsync(bool includeInactive = false);
    Task<Result<IEnumerable<TagDto>>> GetPopularTagsAsync(int count = 20);
    Task<Result<TagDto>> GetTagByIdAsync(Guid id);
    Task<Result<TagDto>> GetTagBySlugAsync(string slug);
    Task<Result<IEnumerable<TagDto>>> SearchTagsAsync(string searchTerm);
    Task<Result<TagDto>> CreateTagAsync(string name, string slug, string? description = null);
    Task<Result<TagDto>> UpdateTagAsync(Guid id, string name, string slug, string? description = null);
    Task<Result> DeleteTagAsync(Guid id);
    Task<Result> AddTagToQuestionAsync(Guid questionId, Guid tagId);
    Task<Result> RemoveTagFromQuestionAsync(Guid questionId, Guid tagId);
    Task<Result> SyncQuestionTagsAsync(Guid questionId, List<Guid> tagIds);
}
