using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class TagService : ITagService
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly IRepository<QuestionTag> _questionTagRepository;
    private readonly IRepository<Question> _questionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TagService> _logger;

    public TagService(
        IRepository<Tag> tagRepository,
        IRepository<QuestionTag> questionTagRepository,
        IRepository<Question> questionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TagService> logger)
    {
        _tagRepository = tagRepository;
        _questionTagRepository = questionTagRepository;
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TagDto>>> GetAllTagsAsync(bool includeInactive = false)
    {
        try
        {
            var query = _tagRepository.GetQueryable();

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            var tags = await query
                .OrderBy(t => t.Name)
                .ToListAsync();

            var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            return Result.Success<IEnumerable<TagDto>>(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tags");
            return Result.Failure<IEnumerable<TagDto>>("Failed to retrieve tags");
        }
    }

    public async Task<Result<IEnumerable<TagDto>>> GetPopularTagsAsync(int count = 20)
    {
        try
        {
            var tags = await _tagRepository.GetQueryable()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.UsageCount)
                .Take(count)
                .ToListAsync();

            var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            return Result.Success<IEnumerable<TagDto>>(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular tags");
            return Result.Failure<IEnumerable<TagDto>>("Failed to retrieve popular tags");
        }
    }

    public async Task<Result<TagDto>> GetTagByIdAsync(Guid id)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return Result.Failure<TagDto>("Tag not found");
            }

            var tagDto = _mapper.Map<TagDto>(tag);
            return Result.Success<TagDto>(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tag by id {TagId}", id);
            return Result.Failure<TagDto>("Failed to retrieve tag");
        }
    }

    public async Task<Result<TagDto>> GetTagBySlugAsync(string slug)
    {
        try
        {
            var tag = await _tagRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tag == null)
            {
                return Result.Failure<TagDto>("Tag not found");
            }

            var tagDto = _mapper.Map<TagDto>(tag);
            return Result.Success<TagDto>(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tag by slug {Slug}", slug);
            return Result.Failure<TagDto>("Failed to retrieve tag");
        }
    }

    public async Task<Result<IEnumerable<TagDto>>> SearchTagsAsync(string searchTerm)
    {
        try
        {
            var tags = await _tagRepository.GetQueryable()
                .Where(t => t.IsActive && t.Name.Contains(searchTerm))
                .OrderBy(t => t.Name)
                .Take(10)
                .ToListAsync();

            var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            return Result.Success<IEnumerable<TagDto>>(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tags with term {SearchTerm}", searchTerm);
            return Result.Failure<IEnumerable<TagDto>>("Failed to search tags");
        }
    }

    public async Task<Result<TagDto>> CreateTagAsync(string name, string slug, string? description = null)
    {
        try
        {
            var existingTag = await _tagRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (existingTag != null)
            {
                return Result.Failure<TagDto>("A tag with this slug already exists");
            }

            var tag = new Tag(name, slug, description);
            await _tagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            var tagDto = _mapper.Map<TagDto>(tag);
            return Result.Success<TagDto>(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return Result.Failure<TagDto>("Failed to create tag");
        }
    }

    public async Task<Result<TagDto>> UpdateTagAsync(Guid id, string name, string slug, string? description = null)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return Result.Failure<TagDto>("Tag not found");
            }

            var existingTag = await _tagRepository.GetQueryable()
                .FirstOrDefaultAsync(t => t.Slug == slug && t.Id != id);

            if (existingTag != null)
            {
                return Result.Failure<TagDto>("A tag with this slug already exists");
            }

            tag.Update(name, slug, description);
            _tagRepository.Update(tag);
            await _unitOfWork.SaveChangesAsync();

            var tagDto = _mapper.Map<TagDto>(tag);
            return Result.Success<TagDto>(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", id);
            return Result.Failure<TagDto>("Failed to update tag");
        }
    }

    public async Task<Result> DeleteTagAsync(Guid id)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return Result.Failure("Tag not found");
            }

            _tagRepository.Delete(tag);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            return Result.Failure("Failed to delete tag");
        }
    }

    public async Task<Result> AddTagToQuestionAsync(Guid questionId, Guid tagId)
    {
        try
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return Result.Failure("Question not found");
            }

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                return Result.Failure("Tag not found");
            }

            var existingQuestionTag = await _questionTagRepository.GetQueryable()
                .FirstOrDefaultAsync(qt => qt.QuestionId == questionId && qt.TagId == tagId);

            if (existingQuestionTag != null)
            {
                return Result.Failure("Tag already added to question");
            }

            var questionTag = new QuestionTag(questionId, tagId);
            await _questionTagRepository.AddAsync(questionTag);
            
            tag.IncrementUsage();
            _tagRepository.Update(tag);
            
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag {TagId} to question {QuestionId}", tagId, questionId);
            return Result.Failure("Failed to add tag to question");
        }
    }

    public async Task<Result> RemoveTagFromQuestionAsync(Guid questionId, Guid tagId)
    {
        try
        {
            var questionTag = await _questionTagRepository.GetQueryable()
                .FirstOrDefaultAsync(qt => qt.QuestionId == questionId && qt.TagId == tagId);

            if (questionTag == null)
            {
                return Result.Failure("Tag not found on question");
            }

            _questionTagRepository.Delete(questionTag);
            
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag != null)
            {
                tag.DecrementUsage();
                _tagRepository.Update(tag);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag {TagId} from question {QuestionId}", tagId, questionId);
            return Result.Failure("Failed to remove tag from question");
        }
    }

    public async Task<Result> SyncQuestionTagsAsync(Guid questionId, List<Guid> tagIds)
    {
        try
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return Result.Failure("Question not found");
            }

            // Get existing tags
            var existingQuestionTags = await _questionTagRepository.GetQueryable()
                .Where(qt => qt.QuestionId == questionId)
                .ToListAsync();

            var existingTagIds = existingQuestionTags.Select(qt => qt.TagId).ToList();

            // Remove tags that are no longer needed
            var tagsToRemove = existingQuestionTags.Where(qt => !tagIds.Contains(qt.TagId)).ToList();
            foreach (var questionTag in tagsToRemove)
            {
                _questionTagRepository.Delete(questionTag);
                
                var tag = await _tagRepository.GetByIdAsync(questionTag.TagId);
                if (tag != null)
                {
                    tag.DecrementUsage();
                    _tagRepository.Update(tag);
                }
            }

            // Add new tags
            var tagsToAdd = tagIds.Where(tagId => !existingTagIds.Contains(tagId)).ToList();
            foreach (var tagId in tagsToAdd)
            {
                var tag = await _tagRepository.GetByIdAsync(tagId);
                if (tag == null) continue;

                var questionTag = new QuestionTag(questionId, tagId);
                await _questionTagRepository.AddAsync(questionTag);
                
                tag.IncrementUsage();
                _tagRepository.Update(tag);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing tags for question {QuestionId}", questionId);
            return Result.Failure("Failed to sync question tags");
        }
    }
}


