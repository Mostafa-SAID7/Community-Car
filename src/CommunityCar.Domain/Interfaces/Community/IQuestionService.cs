using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IQuestionService
{
    Task<Question> CreateQuestionAsync(string title, string content, Guid authorId, Guid? categoryId = null, string? tags = null);
    Task<Question> UpdateQuestionAsync(Guid questionId, string title, string content, Guid? categoryId = null, string? tags = null);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task DeleteQuestionAsync(Guid questionId);
    Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId, Guid? currentUserId = null);
    Task<QuestionDto?> GetQuestionBySlugAsync(string slug, Guid? currentUserId = null);
    Task<PagedResult<QuestionDto>> GetQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, bool? isResolved = null, bool? hasAnswers = null, Guid? currentUserId = null);
    Task<PagedResult<QuestionDto>> GetUserQuestionsAsync(Guid userId, QueryParameters parameters);
    Task<PagedResult<QuestionDto>> GetBookmarkedQuestionsAsync(Guid userId, QueryParameters parameters);
    Task<PagedResult<QuestionDto>> GetTrendingQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, Guid? currentUserId = null);
    Task<PagedResult<QuestionDto>> GetRecentQuestionsAsync(QueryParameters parameters, string? searchTerm = null, string? tag = null, Guid? currentUserId = null);
    Task<PagedResult<QuestionDto>> GetSuggestedQuestionsAsync(Guid userId, QueryParameters parameters);
    Task<List<QuestionDto>> GetRelatedQuestionsAsync(Guid questionId, int count = 4);
    
    Task<AnswerDto> AddAnswerAsync(Guid questionId, string content, Guid authorId);
    Task<AnswerDto> UpdateAnswerAsync(Guid answerId, string content);
    Task DeleteAnswerAsync(Guid answerId);
    Task<AnswerDto?> GetAnswerByIdAsync(Guid answerId, Guid? currentUserId = null);
    Task<IEnumerable<AnswerDto>> GetAnswersAsync(Guid questionId, Guid? currentUserId = null);
    
    Task AcceptAnswerAsync(Guid questionId, Guid answerId, Guid userId);
    Task UnacceptAnswerAsync(Guid questionId, Guid userId);
    
    Task VoteQuestionAsync(Guid questionId, Guid userId, bool isUpvote);
    Task RemoveQuestionVoteAsync(Guid questionId, Guid userId);
    Task VoteAnswerAsync(Guid answerId, Guid userId, bool isUpvote);
    Task RemoveAnswerVoteAsync(Guid answerId, Guid userId);
    
    Task IncrementViewCountAsync(Guid questionId);
    
    // Bookmark methods
    Task<QuestionBookmark?> BookmarkQuestionAsync(Guid questionId, Guid userId, string? notes = null);
    Task RemoveBookmarkAsync(Guid questionId, Guid userId);
    Task<QuestionBookmark?> GetBookmarkAsync(Guid questionId, Guid userId);
    Task<PagedResult<QuestionBookmark>> GetUserBookmarksAsync(Guid userId, QueryParameters parameters);
    Task UpdateBookmarkNotesAsync(Guid bookmarkId, Guid userId, string? notes);
    
    // Reaction methods
    Task<QuestionReaction> ReactToQuestionAsync(Guid questionId, Guid userId, ReactionType reactionType);
    Task RemoveQuestionReactionAsync(Guid questionId, Guid userId);
    Task<IEnumerable<ReactionSummary>> GetQuestionReactionSummaryAsync(Guid questionId, Guid? currentUserId = null);
    
    Task<AnswerReaction> ReactToAnswerAsync(Guid answerId, Guid userId, ReactionType reactionType);
    Task RemoveAnswerReactionAsync(Guid answerId, Guid userId);
    Task<IEnumerable<ReactionSummary>> GetAnswerReactionSummaryAsync(Guid answerId, Guid? currentUserId = null);
    
    // Share methods
    Task<QuestionShare> ShareQuestionAsync(Guid questionId, Guid userId, string? platform = null, string? sharedUrl = null);
    Task<int> GetQuestionShareCountAsync(Guid questionId);

    // Comment methods
    Task<AnswerCommentDto> AddAnswerCommentAsync(Guid answerId, string content, Guid userId);
    Task<IEnumerable<AnswerCommentDto>> GetAnswerCommentsAsync(Guid answerId);
    Task<AnswerCommentDto?> GetCommentByIdAsync(Guid commentId);
}

