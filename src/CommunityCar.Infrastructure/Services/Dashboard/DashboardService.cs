using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Repos.Common;
using CommunityCar.Infrastructure.Uow.Common;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<CommunityCar.Domain.Entities.Community.friends.Friendship> _friendshipRepository;
    private readonly IUnitOfWork _uow;

    public DashboardService(
        IRepository<ApplicationUser> userRepository,
        IRepository<CommunityCar.Domain.Entities.Community.friends.Friendship> friendshipRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _uow = uow;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        return new DashboardSummary(
            TotalUsers: await _userRepository.CountAsync(),
            Slug: "main-dashboard",
            TotalFriendships: await _friendshipRepository.CountAsync(f => f.Status == CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.Accepted),
            ActiveEvents: 12, // Placeholder
            SystemHealth: 98.5
        );
    }

    public async Task<IEnumerable<KPIValue>> GetWeeklyActivityAsync()
    {
        return await Task.FromResult(new List<KPIValue>
        {
            new("Mon", 45),
            new("Tue", 52),
            new("Wed", 48),
            new("Thu", 61),
            new("Fri", 55),
            new("Sat", 72),
            new("Sun", 68)
        });
    }
}
