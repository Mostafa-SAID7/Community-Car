using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Infrastructure.Data;
using CommunityCar.Infrastructure.Services.Community;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Moq;
using Xunit;
using CommunityCar.Infrastructure.Uow.Common;
using CommunityCar.Infrastructure.Repos.Common;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Tests.Concurrency;

public class VotingConcurrencyTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public VotingConcurrencyTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ConcurrentVotes_ShouldNotThrowDuplicateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        using (var context = new ApplicationDbContext(_options))
        {
            var author = new ApplicationUser { Id = userId, UserName = "testuser", Email = "testuser@example.com" };
            context.Set<ApplicationUser>().Add(author);
            var question = new Question("Test Title", "Test Content", userId);
            // Use reflection or set ID if possible, but for InMemory Guid.NewGuid() works fine for ref
            context.Set<Question>().Add(question);
            await context.SaveChangesAsync();
            
            // Re-fetch to get tracked entities
            var qId = question.Id;

            var uow = new UnitOfWork(context);
            var qRepo = new Repository<Question>(context);
            var aRepo = new Repository<Answer>(context);
            var qvRepo = new Repository<QuestionVote>(context);
            var avRepo = new Repository<AnswerVote>(context);
            var arRepo = new Repository<AnswerReaction>(context);
            var qrRepo = new Repository<QuestionReaction>(context);
            var qsRepo = new Repository<QuestionShare>(context);
            var cRepo = new Repository<Category>(context);
            var qbRepo = new Repository<QuestionBookmark>(context);
            var acRepo = new Repository<AnswerComment>(context);
            var userRepo = new Repository<ApplicationUser>(context);
            var notificationService = Mock.Of<INotificationService>();

            var service = new QuestionService(
                qRepo, aRepo, qvRepo, avRepo, qbRepo, qrRepo, arRepo, qsRepo, cRepo, acRepo, userRepo, notificationService, uow, Mock.Of<IMapper>());

            // Act
            // Simulate 5 concurrent upvotes from the same user
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(service.VoteQuestionAsync(qId, userId, true));
            }

            // This hits the memory DB. In memory might not be perfectly parallel in a way that triggers the race 
            // the same way SQL does, but our try-catch logic should be triggered if the DB provider throws.
            // Note: EF InMemory DOES NOT enforce unique constraints by default! 
            // So this test is more about verifying the service logic runs without crashing.
            
            await Task.WhenAll(tasks);

            // Assert
            var finalQuestion = await context.Set<Question>().AsNoTracking().FirstOrDefaultAsync(q => q.Id == qId);
            Assert.NotNull(finalQuestion);
            // Since it's a toggle: 1st click = 1, 2nd = 0, 3rd = 1, 4th = 0, 5th = 1
            Assert.Equal(1, finalQuestion.VoteCount);
            
            var finalVotes = await context.Set<QuestionVote>().CountAsync(v => v.QuestionId == qId && v.UserId == userId);
            Assert.Equal(1, finalVotes);
        }
    }
}
