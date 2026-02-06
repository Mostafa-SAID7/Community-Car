
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Services.Community;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepository;
    private readonly IRepository<Answer> _answerRepository;
    private readonly IRepository<QuestionVote> _questionVoteRepository;
    private readonly IRepository<AnswerVote> _answerVoteRepository;
    private readonly IRepository<QuestionBookmark> _bookmarkRepository;
    private readonly IRepository<QuestionReaction> _questionReactionRepository;
    private readonly IRepository<AnswerReaction> _answerReactionRepository;
    private readonly IRepository<QuestionShare> _shareRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public QuestionService(
        IRepository<Question> questionRepository,
        IRepository<Answer> answerRepository,
        IRepository<QuestionVote> questionVoteRepository,
        IRepository<AnswerVote> answerVoteRepository,
        IRepository<QuestionBookmark> bookmarkRepository,
        IRepository<QuestionReaction> questionReactionRepository,
        IRepository<AnswerReaction> answerReactionRepository,
        IRepository<QuestionShare> shareRepository,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _questionVoteRepository = questionVoteRepository;
        _answerVoteRepository = answerVoteRepository;
        _bookmarkRepository = bookmarkRepository;
        _questionReactionRepository = questionReactionRepository;
        _answerReactionRepository = answerReactionRepository;
        _shareRepository = shareRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Question> CreateQuestionAsync(string title, string content, Guid authorId, string? tags = null)
    {
        var question = new Question(title, content, authorId, null, tags);
        await _questionRepository.AddAsync(question);
        await _uow.SaveChangesAsync();
        return question;
    }

    public async Task<Question> UpdateQuestionAsync(Guid questionId, string title, string content, string? tags = null)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        question.Update(title, content, null, tags);
        await _uow.SaveChangesAsync();
        return question;
    }

    public async Task DeleteQuestionAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        _questionRepository.Delete(question);
        await _uow.SaveChangesAsync();
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        return question == null ? null : _mapper.Map<QuestionDto>(question);
    }

    public async Task<PagedResult<QuestionDto>> GetQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, bool? isResolved = null)
    {
        var allQuestions = await _questionRepository.GetAllAsync();
        var query = allQuestions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(q => 
                q.Title.ToLower().Contains(lowerSearch) || 
                q.Content.ToLower().Contains(lowerSearch) ||
                (q.Tags != null && q.Tags.ToLower().Contains(lowerSearch)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(q => q.Tags != null && q.Tags.Contains(tag));

        if (isResolved.HasValue)
            query = query.Where(q => q.IsResolved == isResolved.Value);

        var totalCount = query.Count();
        
        // Sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = parameters.SortBy.ToLower() switch
            {
                "votes" => parameters.SortDescending 
                    ? query.OrderByDescending(q => q.VoteCount) 
                    : query.OrderBy(q => q.VoteCount),
                "views" => parameters.SortDescending 
                    ? query.OrderByDescending(q => q.ViewCount) 
                    : query.OrderBy(q => q.ViewCount),
                "answers" => parameters.SortDescending 
                    ? query.OrderByDescending(q => q.AnswerCount) 
                    : query.OrderBy(q => q.AnswerCount),
                "created" => parameters.SortDescending 
                    ? query.OrderByDescending(q => q.CreatedAt) 
                    : query.OrderBy(q => q.CreatedAt),
                _ => query.OrderByDescending(q => q.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(q => q.CreatedAt);
        }
        
        var items = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetUserQuestionsAsync(Guid userId, QueryParameters parameters)
    {
        var allQuestions = await _questionRepository.WhereAsync(q => q.AuthorId == userId);
        var query = allQuestions.AsQueryable();
        var totalCount = query.Count();
        
        var items = query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetTrendingQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null)
    {
        var allQuestions = await _questionRepository.GetAllAsync();
        var query = allQuestions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(q => 
                q.Title.ToLower().Contains(lowerSearch) || 
                q.Content.ToLower().Contains(lowerSearch) ||
                (q.Tags != null && q.Tags.ToLower().Contains(lowerSearch)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(q => q.Tags != null && q.Tags.Contains(tag));

        var totalCount = query.Count();
        
        // Trending: Sort by combination of votes, views, and recent activity
        var items = query
            .OrderByDescending(q => (q.VoteCount * 2) + (q.ViewCount / 10) + (q.AnswerCount * 3))
            .ThenByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetRecentQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null)
    {
        var allQuestions = await _questionRepository.GetAllAsync();
        var query = allQuestions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(q => 
                q.Title.ToLower().Contains(lowerSearch) || 
                q.Content.ToLower().Contains(lowerSearch) ||
                (q.Tags != null && q.Tags.ToLower().Contains(lowerSearch)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(q => q.Tags != null && q.Tags.Contains(tag));

        var totalCount = query.Count();
        
        // Recent: Questions from last 7 days, sorted by newest
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var items = query
            .Where(q => q.CreatedAt >= sevenDaysAgo)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetSuggestedQuestionsAsync(Guid userId, QueryParameters parameters)
    {
        // Get user's questions to find their interests (tags)
        var userQuestions = await _questionRepository.WhereAsync(q => q.AuthorId == userId);
        var userTags = userQuestions
            .Where(q => !string.IsNullOrEmpty(q.Tags))
            .SelectMany(q => q.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim().ToLower())
            .Distinct()
            .ToList();

        var allQuestions = await _questionRepository.GetAllAsync();
        var query = allQuestions
            .Where(q => q.AuthorId != userId) // Exclude user's own questions
            .AsQueryable();

        // If user has tags, prioritize questions with similar tags
        if (userTags.Any())
        {
            query = query.Where(q => 
                !string.IsNullOrEmpty(q.Tags) && 
                userTags.Any(tag => q.Tags.ToLower().Contains(tag)));
        }

        var totalCount = query.Count();
        
        // Sort by unanswered first, then by recent
        var items = query
            .OrderBy(q => q.IsResolved)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<AnswerDto> AddAnswerAsync(Guid questionId, string content, Guid authorId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        var answer = new Answer(questionId, content, authorId);
        await _answerRepository.AddAsync(answer);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AnswerDto>(answer);
    }

    public async Task<AnswerDto> UpdateAnswerAsync(Guid answerId, string content)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null)
            throw new Exception($"Answer with ID {answerId} not found.");

        answer.Update(content);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AnswerDto>(answer);
    }

    public async Task DeleteAnswerAsync(Guid answerId)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null)
            throw new Exception($"Answer with ID {answerId} not found.");

        _answerRepository.Delete(answer);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<AnswerDto>> GetAnswersAsync(Guid questionId)
    {
        var answers = await _answerRepository.WhereAsync(a => a.QuestionId == questionId);
        var sortedAnswers = answers.OrderByDescending(a => a.IsAccepted).ThenByDescending(a => a.VoteCount);
        return _mapper.Map<IEnumerable<AnswerDto>>(sortedAnswers);
    }

    public async Task AcceptAnswerAsync(Guid questionId, Guid answerId, Guid userId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        if (question.AuthorId != userId)
            throw new Exception("Only the question author can accept an answer.");

        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null || answer.QuestionId != questionId)
            throw new Exception($"Answer with ID {answerId} not found for this question.");

        if (question.AcceptedAnswerId.HasValue)
        {
            var previousAnswer = await _answerRepository.GetByIdAsync(question.AcceptedAnswerId.Value);
            previousAnswer?.UnmarkAsAccepted();
        }

        answer.MarkAsAccepted();
        question.MarkAsResolved(answerId);
        await _uow.SaveChangesAsync();
    }

    public async Task UnacceptAnswerAsync(Guid questionId, Guid userId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        if (question.AuthorId != userId)
            throw new Exception("Only the question author can unaccept an answer.");

        if (question.AcceptedAnswerId.HasValue)
        {
            var answer = await _answerRepository.GetByIdAsync(question.AcceptedAnswerId.Value);
            answer?.UnmarkAsAccepted();
        }

        question.MarkAsUnresolved();
        await _uow.SaveChangesAsync();
    }

    public async Task VoteQuestionAsync(Guid questionId, Guid userId, bool isUpvote)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        var existingVote = await _questionVoteRepository.FirstOrDefaultAsync(v => 
            v.QuestionId == questionId && v.UserId == userId);

        if (existingVote != null)
        {
            var oldValue = existingVote.IsUpvote ? 1 : -1;
            existingVote.Toggle();
            var newValue = existingVote.IsUpvote ? 1 : -1;
            question.UpdateVoteCount(newValue - oldValue);
        }
        else
        {
            var vote = new QuestionVote(questionId, userId, isUpvote);
            await _questionVoteRepository.AddAsync(vote);
            question.UpdateVoteCount(isUpvote ? 1 : -1);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task RemoveQuestionVoteAsync(Guid questionId, Guid userId)
    {
        var vote = await _questionVoteRepository.FirstOrDefaultAsync(v => 
            v.QuestionId == questionId && v.UserId == userId);

        if (vote != null)
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question != null)
            {
                question.UpdateVoteCount(vote.IsUpvote ? -1 : 1);
            }

            _questionVoteRepository.Delete(vote);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task VoteAnswerAsync(Guid answerId, Guid userId, bool isUpvote)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null)
            throw new Exception($"Answer with ID {answerId} not found.");

        var existingVote = await _answerVoteRepository.FirstOrDefaultAsync(v => 
            v.AnswerId == answerId && v.UserId == userId);

        if (existingVote != null)
        {
            var oldValue = existingVote.IsUpvote ? 1 : -1;
            existingVote.Toggle();
            var newValue = existingVote.IsUpvote ? 1 : -1;
            answer.UpdateVoteCount(newValue - oldValue);
        }
        else
        {
            var vote = new AnswerVote(answerId, userId, isUpvote);
            await _answerVoteRepository.AddAsync(vote);
            answer.UpdateVoteCount(isUpvote ? 1 : -1);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task RemoveAnswerVoteAsync(Guid answerId, Guid userId)
    {
        var vote = await _answerVoteRepository.FirstOrDefaultAsync(v => 
            v.AnswerId == answerId && v.UserId == userId);

        if (vote != null)
        {
            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (answer != null)
            {
                answer.UpdateVoteCount(vote.IsUpvote ? -1 : 1);
            }

            _answerVoteRepository.Delete(vote);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task IncrementViewCountAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question != null)
        {
            question.IncrementViewCount();
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<QuestionBookmark> BookmarkQuestionAsync(Guid questionId, Guid userId, string? notes = null)
    {
        var bookmark = new QuestionBookmark(questionId, userId, notes);
        await _bookmarkRepository.AddAsync(bookmark);
        await _uow.SaveChangesAsync();
        return bookmark;
    }

    public async Task RemoveBookmarkAsync(Guid questionId, Guid userId)
    {
        var bookmark = await _bookmarkRepository.FirstOrDefaultAsync(b => b.QuestionId == questionId && b.UserId == userId);
        if (bookmark != null)
        {
            _bookmarkRepository.Delete(bookmark);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<QuestionBookmark?> GetBookmarkAsync(Guid questionId, Guid userId)
    {
        return await _bookmarkRepository.FirstOrDefaultAsync(b => b.QuestionId == questionId && b.UserId == userId);
    }

    public async Task<PagedResult<QuestionBookmark>> GetUserBookmarksAsync(Guid userId, QueryParameters parameters)
    {
        var allBookmarks = await _bookmarkRepository.WhereAsync(b => b.UserId == userId);
        var query = allBookmarks.AsQueryable();
        var totalCount = query.Count();
        var items = query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();
        return new PagedResult<QuestionBookmark>(items, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task UpdateBookmarkNotesAsync(Guid bookmarkId, Guid userId, string? notes)
    {
        var bookmark = await _bookmarkRepository.GetByIdAsync(bookmarkId);
        if (bookmark != null && bookmark.UserId == userId)
        {
            bookmark.UpdateNotes(notes);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<QuestionReaction> ReactToQuestionAsync(Guid questionId, Guid userId, ReactionType reactionType)
    {
        var existing = await _questionReactionRepository.FirstOrDefaultAsync(r => r.QuestionId == questionId && r.UserId == userId);
        if (existing != null)
        {
            existing.ChangeReaction(reactionType);
        }
        else
        {
            existing = new QuestionReaction(questionId, userId, reactionType);
            await _questionReactionRepository.AddAsync(existing);
        }
        await _uow.SaveChangesAsync();
        return existing;
    }

    public async Task RemoveQuestionReactionAsync(Guid questionId, Guid userId)
    {
        var reaction = await _questionReactionRepository.FirstOrDefaultAsync(r => r.QuestionId == questionId && r.UserId == userId);
        if (reaction != null)
        {
            _questionReactionRepository.Delete(reaction);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ReactionSummary>> GetQuestionReactionSummaryAsync(Guid questionId, Guid? currentUserId = null)
    {
        var reactions = await _questionReactionRepository.WhereAsync(r => r.QuestionId == questionId);
        return reactions.GroupBy(r => r.ReactionType)
            .Select(g => new ReactionSummary(
                g.Key,
                g.Count(),
                currentUserId.HasValue && g.Any(r => r.UserId == currentUserId.Value)
            ));
    }

    public async Task<AnswerReaction> ReactToAnswerAsync(Guid answerId, Guid userId, ReactionType reactionType)
    {
        var existing = await _answerReactionRepository.FirstOrDefaultAsync(r => r.AnswerId == answerId && r.UserId == userId);
        if (existing != null)
        {
            existing.ChangeReaction(reactionType);
        }
        else
        {
            existing = new AnswerReaction(answerId, userId, reactionType);
            await _answerReactionRepository.AddAsync(existing);
        }
        await _uow.SaveChangesAsync();
        return existing;
    }

    public async Task RemoveAnswerReactionAsync(Guid answerId, Guid userId)
    {
        var reaction = await _answerReactionRepository.FirstOrDefaultAsync(r => r.AnswerId == answerId && r.UserId == userId);
        if (reaction != null)
        {
            _answerReactionRepository.Delete(reaction);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ReactionSummary>> GetAnswerReactionSummaryAsync(Guid answerId, Guid? currentUserId = null)
    {
        var reactions = await _answerReactionRepository.WhereAsync(r => r.AnswerId == answerId);
        return reactions.GroupBy(r => r.ReactionType)
            .Select(g => new ReactionSummary(
                g.Key,
                g.Count(),
                currentUserId.HasValue && g.Any(r => r.UserId == currentUserId.Value)
            ));
    }

    public async Task<QuestionShare> ShareQuestionAsync(Guid questionId, Guid userId, string? platform = null, string? sharedUrl = null)
    {
        var share = new QuestionShare(questionId, userId, platform, sharedUrl);
        await _shareRepository.AddAsync(share);
        await _uow.SaveChangesAsync();
        return share;
    }

    public async Task<int> GetQuestionShareCountAsync(Guid questionId)
    {
        return await _shareRepository.CountAsync(s => s.QuestionId == questionId);
    }
}

