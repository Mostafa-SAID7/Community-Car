using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class FriendshipSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Don't re-seed if we already have a good amount of data
        if (await context.Friendships.CountAsync() > 5) return;

        // Get all users
        var users = await context.Users.Where(u => !u.IsDeleted).ToListAsync();
        if (users.Count < 5) return;

        // Clear existing to avoid duplicates if re-running
        if (await context.Friendships.AnyAsync())
        {
            context.Friendships.RemoveRange(context.Friendships);
            await context.SaveChangesAsync();
        }

        var friendships = new List<Friendship>();
        var rand = new Random();

        // Create a mix of friendships for the first few users
        for (int i = 0; i < Math.Min(users.Count, 10); i++)
        {
            var currentUser = users[i];
            
            // Connect to 3-5 other users
            var targets = users.Where(u => u.Id != currentUser.Id)
                             .OrderBy(x => rand.Next())
                             .Take(rand.Next(3, 6))
                             .ToList();

            foreach (var target in targets)
            {
                // Check if relationship already exists
                if (friendships.Any(f => (f.UserId == currentUser.Id && f.FriendId == target.Id) || 
                                       (f.UserId == target.Id && f.FriendId == currentUser.Id)))
                    continue;

                var f = new Friendship(currentUser.Id, target.Id);
                
                // Randomize status
                int statusRoll = rand.Next(100);
                if (statusRoll < 60) // 60% Accepted
                {
                    f.Accept();
                }
                else if (statusRoll < 85) // 25% Pending
                {
                    // Leave as pending
                }
                else // 15% Blocked
                {
                    f.Block();
                }

                friendships.Add(f);
            }
        }

        await context.Friendships.AddRangeAsync(friendships);
        await context.SaveChangesAsync();
    }
}
