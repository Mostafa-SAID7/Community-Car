
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Identity.Users;
using CommunityCar.Infrastructure.Services.Community;

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
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<AnswerComment> _answerCommentRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly INotificationService _notificationService;
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
        IRepository<Category> categoryRepository,
        IRepository<AnswerComment> answerCommentRepository,
        IRepository<ApplicationUser> userRepository,
        INotificationService notificationService,
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
        _categoryRepository = categoryRepository;
        _answerCommentRepository = answerCommentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Question> CreateQuestionAsync(string title, string content, Guid authorId, Guid? categoryId = null, string? tags = null)
    {
        var user = await _userRepository.GetByIdAsync(authorId);
        if (user == null) throw new Exception("User not found.");

        var today = DateTimeOffset.UtcNow.Date;
        var todayQuestionCount = await _questionRepository.CountAsync(q => q.AuthorId == authorId && q.CreatedAt >= today);
        
        int limit = user.Rank switch
        {
            UserRank.Master => 100,
            UserRank.Moderator => 50,
            UserRank.Author => 20,
            UserRank.Reviewer => 10,
            UserRank.Expert => 5,
            _ => 3
        };

        if (todayQuestionCount >= limit)
        {
            throw new Exception($"Daily question limit reached ({limit}) for your rank ({user.Rank.ToString()}). Upgrade your rank to increase your limit!");
        }

        var question = new Question(title, content, authorId, categoryId, tags);
        await _questionRepository.AddAsync(question);
        
        // Award points
        user.Points += 3;
        _userRepository.Update(user);
        
        await _uow.SaveChangesAsync();
        
        // Notify friends
        await _notificationService.NotifyFriendsOfNewQuestionAsync(authorId, question);

        return question;
    }

    public async Task<Question> UpdateQuestionAsync(Guid questionId, string title, string content, Guid? categoryId = null, string? tags = null)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        question.Update(title, content, categoryId, tags);
        await _uow.SaveChangesAsync();
        
        // Notify friends of update
        await _notificationService.NotifyFriendsOfQuestionUpdateAsync(question.AuthorId, question);
        
        return question;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.WhereAsync(c => c.IsActive);
        var sorted = categories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
        return _mapper.Map<IEnumerable<CategoryDto>>(sorted);
    }

    public async Task DeleteQuestionAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        _questionRepository.Delete(question);
        await _uow.SaveChangesAsync();
        
        // Notify friends of deletion
        await _notificationService.NotifyFriendsOfQuestionDeleteAsync(question.AuthorId, question.Title);
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId, Guid? currentUserId = null)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null) return null;
        
        var dto = _mapper.Map<QuestionDto>(question);
        if (currentUserId.HasValue)
        {
            await PopulateUserSpecificDataAsync(dto, currentUserId.Value);
        }
        return dto;
    }

    public async Task<QuestionDto?> GetQuestionBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var question = await _questionRepository.FirstOrDefaultAsync(q => q.Slug == slug);
        if (question == null) return null;

        var dto = _mapper.Map<QuestionDto>(question);
        if (currentUserId.HasValue)
        {
            await PopulateUserSpecificDataAsync(dto, currentUserId.Value);
        }
        return dto;
    }

    public async Task<PagedResult<QuestionDto>> GetQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, bool? isResolved = null, bool? hasAnswers = null, Guid? currentUserId = null)
    {
        var query = _questionRepository.GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .AsQueryable();

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

        if (hasAnswers.HasValue)
        {
            if (hasAnswers.Value)
                query = query.Where(q => q.Answers.Any());
            else
                query = query.Where(q => !q.Answers.Any());
        }

        var totalCount = await query.CountAsync();
        
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
                    ? query.OrderByDescending(q => q.Answers.Count) 
                    : query.OrderBy(q => q.Answers.Count),
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
        
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos)
            {
                await PopulateUserSpecificDataAsync(dto, currentUserId.Value);
            }
        }
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetUserQuestionsAsync(Guid userId, QueryParameters parameters)
    {
        var query = _questionRepository.GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .Where(q => q.AuthorId == userId);
            
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetTrendingQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, Guid? currentUserId = null)
    {
        var query = _questionRepository.GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .AsQueryable();

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

        var totalCount = await query.CountAsync();
        
        // Trending: Sort by combination of votes, views, and recent activity
        var items = await query
            .OrderByDescending(q => (q.VoteCount * 2) + (q.ViewCount / 10) + (q.Answers.Count * 3))
            .ThenByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos) await PopulateUserSpecificDataAsync(dto, currentUserId.Value);
        }
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<QuestionDto>> GetRecentQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, Guid? currentUserId = null)
    {
        var query = _questionRepository.GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .AsQueryable();

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

        // Recent: Just order by Creation Date (no strict time limit to ensure results are shown)
        // var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        // query = query.Where(q => q.CreatedAt >= sevenDaysAgo);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos) await PopulateUserSpecificDataAsync(dto, currentUserId.Value);
        }
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

        var query = _questionRepository.GetQueryable()
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.QuestionTags)
                .ThenInclude(qt => qt.Tag)
            .Where(q => q.AuthorId != userId);

        // If user has tags, prioritize questions with similar tags
        if (userTags.Any())
        {
            query = query.Where(q => 
                !string.IsNullOrEmpty(q.Tags) && 
                userTags.Any(tag => q.Tags.ToLower().Contains(tag)));
        }

        var totalCount = await query.CountAsync();
        
        // Sort by unanswered first, then by recent
        var items = await query
            .OrderBy(q => q.IsResolved)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<AnswerDto> AddAnswerAsync(Guid questionId, string content, Guid authorId)
    {
        var user = await _userRepository.GetByIdAsync(authorId);
        if (user == null) throw new Exception("User not found.");

        var today = DateTimeOffset.UtcNow.Date;
        var todayAnswerCount = await _answerRepository.CountAsync(a => a.AuthorId == authorId && a.CreatedAt >= today);
        
        int limit = user.Rank switch
        {
            UserRank.Master => 200,
            UserRank.Moderator => 100,
            UserRank.Author => 50,
            UserRank.Reviewer => 25,
            UserRank.Expert => 10,
            UserRank.Standard => 5,
            _ => 1
        };

        if (todayAnswerCount >= limit)
        {
            throw new Exception($"Daily answer limit reached ({limit}) for your rank ({user.Rank.ToString()}). Upgrade your rank to increase your limit!");
        }

        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
            throw new Exception($"Question with ID {questionId} not found.");

        var answer = new Answer(questionId, content, authorId);
        await _answerRepository.AddAsync(answer);
        
        // Award points
        user.Points += 5;
        _userRepository.Update(user);
        
        await _uow.SaveChangesAsync();
        
        // Notify question author
        await _notificationService.NotifyAuthorOfNewAnswerAsync(question.AuthorId, answer);
        
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

    public async Task<AnswerDto?> GetAnswerByIdAsync(Guid answerId, Guid? currentUserId = null)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null) return null;
        
        var dto = _mapper.Map<AnswerDto>(answer);
        if (currentUserId.HasValue)
        {
            await PopulateAnswerUserSpecificDataAsync(dto, currentUserId.Value);
        }
        return dto;
    }

    public async Task DeleteAnswerAsync(Guid answerId)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer == null)
            throw new Exception($"Answer with ID {answerId} not found.");

        _answerRepository.Delete(answer);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<AnswerDto>> GetAnswersAsync(Guid questionId, Guid? currentUserId = null)
    {
        var answers = await _answerRepository.GetQueryable()
            .Include(a => a.Author)
            .Include(a => a.Comments)
                .ThenInclude(c => c.Author)
            .Where(a => a.QuestionId == questionId)
            .OrderByDescending(a => a.IsAccepted)
            .ThenByDescending(a => a.VoteCount)
            .ToListAsync();
            
        var dtos = _mapper.Map<List<AnswerDto>>(answers);
        
        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos)
            {
                await PopulateAnswerUserSpecificDataAsync(dto, currentUserId.Value);
            }
        }
        
        return dtos;
    }

    private async Task PopulateAnswerUserSpecificDataAsync(AnswerDto dto, Guid userId)
    {
        var vote = await _answerVoteRepository.FirstOrDefaultAsync(v => v.AnswerId == dto.Id && v.UserId == userId);
        if (vote != null)
        {
            dto.CurrentUserVote = vote.IsUpvote ? 1 : -1;
        }
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

        try
        {
            // Use IgnoreQueryFilters to check for ANY vote including soft-deleted ones
            var existingVote = await _questionVoteRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsDeleted)
                {
                    // RESURRECT
                    existingVote.IsDeleted = false;
                    existingVote.DeletedAt = null;
                    existingVote.DeletedBy = null;
                    
                    if (existingVote.IsUpvote != isUpvote)
                    {
                        existingVote.Toggle();
                    }
                    
                    question.UpdateVoteCount(isUpvote ? 1 : -1);
                }
                else
                {
                    // ACTIVE
                    if (existingVote.IsUpvote == isUpvote)
                    {
                        // Clicked same button -> Remove vote
                        question.UpdateVoteCount(isUpvote ? -1 : 1);
                        _questionVoteRepository.Delete(existingVote);
                    }
                    else
                    {
                        // Clicked opposite button -> Toggle
                        var oldValue = existingVote.IsUpvote ? 1 : -1;
                        existingVote.Toggle();
                        var newValue = existingVote.IsUpvote ? 1 : -1;
                        question.UpdateVoteCount(newValue - oldValue);
                    }
                }
            }
            else
            {
                // NEW
                var vote = new QuestionVote(questionId, userId, isUpvote);
                await _questionVoteRepository.AddAsync(vote);
                question.UpdateVoteCount(isUpvote ? 1 : -1);
            }

            await _uow.SaveChangesAsync();

            // Notify author if a new vote was added or resurrected (and not removed)
            var freshVote = await _questionVoteRepository.GetQueryable()
                .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);
            
            if (freshVote != null && question.AuthorId != userId)
            {
                await _notificationService.NotifyAuthorOfQuestionVoteAsync(question.AuthorId, question, isUpvote);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            // Concurrency: Another request inserted the vote first.
            _uow.ClearTracker();
            
            // Re-fetch fresh state
            var freshQuestion = await _questionRepository.GetByIdAsync(questionId);
            var freshVote = await _questionVoteRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);
            
            if (freshVote != null && freshQuestion != null)
            {
                if (freshVote.IsDeleted)
                {
                    freshVote.IsDeleted = false;
                    freshVote.DeletedAt = null;
                    if (freshVote.IsUpvote != isUpvote) freshVote.Toggle();
                    freshQuestion.UpdateVoteCount(isUpvote ? 1 : -1);
                }
                else if (freshVote.IsUpvote == isUpvote)
                {
                   // Racing requests: If we find it already exists with the same value, remove it (standard toggle behavior)
                   freshQuestion.UpdateVoteCount(isUpvote ? -1 : 1);
                   _questionVoteRepository.Delete(freshVote);
                }
                else
                {
                    // Swapping vote
                    var oldValue = freshVote.IsUpvote ? 1 : -1;
                    freshVote.Toggle();
                    var newValue = freshVote.IsUpvote ? 1 : -1;
                    freshQuestion.UpdateVoteCount(newValue - oldValue);
                }
                await _uow.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveQuestionVoteAsync(Guid questionId, Guid userId)
    {
        // Use IgnoreQueryFilters just in case, though usually we only care about active votes here
        var vote = await _questionVoteRepository.GetQueryable()
            .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);

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

        try
        {
            // Use IgnoreQueryFilters to check for ANY vote including soft-deleted ones
            var existingVote = await _answerVoteRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsDeleted)
                {
                    // RESURRECT
                    existingVote.IsDeleted = false;
                    existingVote.DeletedAt = null;
                    existingVote.DeletedBy = null;
                    
                    if (existingVote.IsUpvote != isUpvote)
                    {
                        existingVote.Toggle();
                    }
                    
                    answer.UpdateVoteCount(isUpvote ? 1 : -1);
                }
                else
                {
                    // ACTIVE
                    if (existingVote.IsUpvote == isUpvote)
                    {
                        // Remove
                        answer.UpdateVoteCount(isUpvote ? -1 : 1);
                        _answerVoteRepository.Delete(existingVote);
                    }
                    else
                    {
                        // Toggle
                        var oldValue = existingVote.IsUpvote ? 1 : -1;
                        existingVote.Toggle();
                        var newValue = existingVote.IsUpvote ? 1 : -1;
                        answer.UpdateVoteCount(newValue - oldValue);
                    }
                }
            }
            else
            {
                // NEW
                var vote = new AnswerVote(answerId, userId, isUpvote);
                await _answerVoteRepository.AddAsync(vote);
                answer.UpdateVoteCount(isUpvote ? 1 : -1);
            }

            await _uow.SaveChangesAsync();
            
            // Notify author if a new vote was added
            var freshVote = await _answerVoteRepository.GetQueryable()
                .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);
            
            if (freshVote != null && answer.AuthorId != userId)
            {
                await _notificationService.NotifyAuthorOfAnswerVoteAsync(answer.AuthorId, answer, isUpvote);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _uow.ClearTracker();
            
            // Re-fetch fresh state
            var freshAnswer = await _answerRepository.GetByIdAsync(answerId);
            var freshVote = await _answerVoteRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId);
            
            if (freshVote != null && freshAnswer != null)
            {
                if (freshVote.IsDeleted)
                {
                    freshVote.IsDeleted = false;
                    freshVote.DeletedAt = null;
                    if (freshVote.IsUpvote != isUpvote) freshVote.Toggle();
                    freshAnswer.UpdateVoteCount(isUpvote ? 1 : -1);
                }
                else if (freshVote.IsUpvote == isUpvote)
                {
                    freshAnswer.UpdateVoteCount(isUpvote ? -1 : 1);
                    _answerVoteRepository.Delete(freshVote);
                }
                else
                {
                    var oldValue = freshVote.IsUpvote ? 1 : -1;
                    freshVote.Toggle();
                    var newValue = freshVote.IsUpvote ? 1 : -1;
                    freshAnswer.UpdateVoteCount(newValue - oldValue);
                }
                await _uow.SaveChangesAsync();
            }
        }
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

    public async Task<QuestionBookmark?> BookmarkQuestionAsync(Guid questionId, Guid userId, string? notes = null)
    {
        try
        {
            var existing = await _bookmarkRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.QuestionId == questionId && b.UserId == userId);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    // RESURRECT
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.DeletedBy = null;
                    if (notes != null) existing.UpdateNotes(notes);
                    await _uow.SaveChangesAsync();
                    return existing;
                }
                else
                {
                    // ACTIVE -> DEACTIVATE (Toggle Off)
                    _bookmarkRepository.Delete(existing);
                    await _uow.SaveChangesAsync();
                    return null;
                }
            }

            // NEW
            var bookmark = new QuestionBookmark(questionId, userId, notes);
            await _bookmarkRepository.AddAsync(bookmark);
            await _uow.SaveChangesAsync();
            
            // Notify author if bookmarked
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (bookmark != null && question != null && question.AuthorId != userId)
            {
                await _notificationService.NotifyAuthorOfQuestionBookmarkAsync(question.AuthorId, question);
            }
            
            return bookmark;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _uow.ClearTracker();
            var fresh = await _bookmarkRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.QuestionId == questionId && b.UserId == userId);

            if (fresh != null)
            {
                if (fresh.IsDeleted)
                {
                    fresh.IsDeleted = false;
                    fresh.DeletedAt = null;
                    if (notes != null) fresh.UpdateNotes(notes);
                }
                else
                {
                    // Raced and found it active -> Toggle off to respect standard behavior
                    _bookmarkRepository.Delete(fresh);
                    await _uow.SaveChangesAsync();
                    return null;
                }
                await _uow.SaveChangesAsync();
                return fresh;
            }
            throw;
        }
    }

    public async Task RemoveBookmarkAsync(Guid questionId, Guid userId)
    {
        var bookmark = await _bookmarkRepository.GetQueryable()
            .FirstOrDefaultAsync(b => b.QuestionId == questionId && b.UserId == userId);

        if (bookmark != null)
        {
            _bookmarkRepository.Delete(bookmark);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<QuestionDto>> GetBookmarkedQuestionsAsync(Guid userId, QueryParameters parameters)
    {
        var totalCount = await _bookmarkRepository.CountAsync(b => b.UserId == userId);
        
        var bookmarks = await _bookmarkRepository.GetQueryable()
            .Include(b => b.Question)
                .ThenInclude(q => q.Author)
            .Include(b => b.Question)
                .ThenInclude(q => q.Answers)
            .Include(b => b.Question)
                .ThenInclude(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var items = bookmarks.Select(b => b.Question).ToList();

        var dtos = _mapper.Map<List<QuestionDto>>(items);
        foreach (var dto in dtos) await PopulateUserSpecificDataAsync(dto, userId);
        
        return new PagedResult<QuestionDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
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
        try
        {
            var existing = await _questionReactionRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.QuestionId == questionId && r.UserId == userId);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    // RESURRECT
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.DeletedBy = null;
                    existing.ChangeReaction(reactionType);
                }
                else
                {
                    // UPDATE OR TOGGLE? (Current logic: Update)
                    existing.ChangeReaction(reactionType);
                }
            }
            else
            {
                // NEW
                existing = new QuestionReaction(questionId, userId, reactionType);
                await _questionReactionRepository.AddAsync(existing);
            }

            await _uow.SaveChangesAsync();
            
            // Notify author
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (existing != null && question != null && question.AuthorId != userId)
            {
                await _notificationService.NotifyAuthorOfQuestionReactionAsync(question.AuthorId, question, reactionType.ToString());
            }

            return existing;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _uow.ClearTracker();
            var fresh = await _questionReactionRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.QuestionId == questionId && r.UserId == userId);

            if (fresh != null)
            {
                fresh.IsDeleted = false;
                fresh.DeletedAt = null;
                fresh.ChangeReaction(reactionType);
                await _uow.SaveChangesAsync();
                return fresh;
            }
            throw;
        }
    }

    public async Task RemoveQuestionReactionAsync(Guid questionId, Guid userId)
    {
        var reaction = await _questionReactionRepository.GetQueryable()
            .FirstOrDefaultAsync(r => r.QuestionId == questionId && r.UserId == userId);

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
        try
        {
            var existing = await _answerReactionRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.AnswerId == answerId && r.UserId == userId);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    existing.DeletedBy = null;
                    existing.ChangeReaction(reactionType);
                }
                else
                {
                    existing.ChangeReaction(reactionType);
                }
            }
            else
            {
                existing = new AnswerReaction(answerId, userId, reactionType);
                await _answerReactionRepository.AddAsync(existing);
            }

            await _uow.SaveChangesAsync();
            
            // Notify author
            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (existing != null && answer != null && answer.AuthorId != userId)
            {
                await _notificationService.NotifyAuthorOfAnswerReactionAsync(answer.AuthorId, answer, reactionType.ToString());
            }

            return existing;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _uow.ClearTracker();
            var fresh = await _answerReactionRepository.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.AnswerId == answerId && r.UserId == userId);

            if (fresh != null)
            {
                fresh.IsDeleted = false;
                fresh.DeletedAt = null;
                fresh.ChangeReaction(reactionType);
                await _uow.SaveChangesAsync();
                return fresh;
            }
            throw;
        }
    }

    public async Task RemoveAnswerReactionAsync(Guid answerId, Guid userId)
    {
        var reaction = await _answerReactionRepository.GetQueryable()
            .FirstOrDefaultAsync(r => r.AnswerId == answerId && r.UserId == userId);

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
        
        // Notify author
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question != null && question.AuthorId != userId)
        {
            await _notificationService.NotifyAuthorOfQuestionShareAsync(question.AuthorId, question);
        }
        
        return share;
    }

    public async Task<int> GetQuestionShareCountAsync(Guid questionId)
    {
        return await _shareRepository.CountAsync(s => s.QuestionId == questionId);
    }

    public async Task<AnswerCommentDto> AddAnswerCommentAsync(Guid answerId, string content, Guid userId)
    {
        var comment = new AnswerComment(answerId, userId, content);
        await _answerCommentRepository.AddAsync(comment);
        await _uow.SaveChangesAsync();
        
        // Notify answer author
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer != null && answer.AuthorId != userId)
        {
            await _notificationService.NotifyAuthorOfNewCommentAsync(answer.AuthorId, comment);
        }
        
        // Fetch fresh to include author
        var freshComment = await _answerCommentRepository.GetQueryable()
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);
            
        return _mapper.Map<AnswerCommentDto>(freshComment);
    }

    public async Task<IEnumerable<AnswerCommentDto>> GetAnswerCommentsAsync(Guid answerId)
    {
        var comments = await _answerCommentRepository.GetQueryable()
            .Include(c => c.Author)
            .Where(c => c.AnswerId == answerId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<AnswerCommentDto>>(comments);
    }

    public async Task<AnswerCommentDto?> GetCommentByIdAsync(Guid commentId)
    {
        var comment = await _answerCommentRepository.GetQueryable()
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId);
            
        return _mapper.Map<AnswerCommentDto?>(comment);
    }

    private async Task PopulateUserSpecificDataAsync(QuestionDto dto, Guid userId)
    {
        var vote = await _questionVoteRepository.FirstOrDefaultAsync(v => v.QuestionId == dto.Id && v.UserId == userId);
        if (vote != null)
        {
            dto.CurrentUserVote = vote.IsUpvote ? 1 : -1;
        }

        var bookmark = await _bookmarkRepository.FirstOrDefaultAsync(b => b.QuestionId == dto.Id && b.UserId == userId);
        dto.IsBookmarkedByUser = bookmark != null;

        var reaction = await _questionReactionRepository.FirstOrDefaultAsync(r => r.QuestionId == dto.Id && r.UserId == userId);
        if (reaction != null)
        {
            dto.CurrentUserReactionType = (int)reaction.ReactionType;
        }
    }


}

