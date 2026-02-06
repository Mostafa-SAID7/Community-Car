using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class FriendshipSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Friendships.AnyAsync()) return;

        var users = await context.Users.Take(3).ToListAsync();
        if (users.Count < 2) return;

        var user1 = users[0];
        var user2 = users[1];
        var user3 = users.Count > 2 ? users[2] : null;

        var friendships = new List<Friendship>
        {
            new Friendship(user1.Id, user2.Id) // Pending
        };

        if (user3 != null)
        {
            var f2 = new Friendship(user2.Id, user3.Id);
            f2.Accept();
            friendships.Add(f2);
        }

        await context.Friendships.AddRangeAsync(friendships);
        await context.SaveChangesAsync();
    }
}
