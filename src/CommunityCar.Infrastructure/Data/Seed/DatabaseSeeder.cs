using Microsoft.Extensions.DependencyInjection;
using CommunityCar.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Identity.Roles;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Entities.Community.guides;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Domain.Entities.Community.maps;
using CommunityCar.Domain.Entities.Community.news;
using CommunityCar.Domain.Entities.Community.events;
using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Entities.Communications.chats;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Domain.Enums.Community.events;

namespace CommunityCar.Infrastructure.Data.Seed;

/// <summary>
/// Consolidated database seeder - seeds all data in correct order
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try 
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            logger.LogInformation("Starting database seeding...");

            // 1. Seed Roles
            await SeedRolesAsync(context, roleManager, logger);

            // 2. Seed Users
            await SeedUsersAsync(userManager, logger);

            // 3. Seed Categories
            await SeedCategoriesAsync(context, logger);

            // 4. Seed Groups
            await SeedGroupsAsync(context, userManager, logger);

            // 5. Seed Community Content
            await SeedPostsAsync(context, logger);
            await SeedGuidesAsync(context, logger);
            await SeedQuestionsAsync(context, logger);
            await SeedMapPointsAsync(context, logger);
            await SeedNewsAsync(context, logger);
            await SeedEventsAsync(context, logger);

            // 6. Seed Friendships
            await SeedFriendshipsAsync(context, logger);

            // 7. Seed Chats
            await SeedChatsAsync(context, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during seeding: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        string[] roleNames = { "SuperAdmin", "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Creating role: {RoleName}", roleName);
                var role = new ApplicationRole 
                { 
                    Id = Guid.NewGuid(),
                    Name = roleName, 
                    NormalizedName = roleName.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                
                context.Roles.Add(role);
                await context.SaveChangesAsync();
                logger.LogInformation("Role {RoleName} created successfully.", roleName);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new List<(string Email, string FirstName, string LastName, string Password, string Role)>
        {
            ("admin@communitycar.com", "Super", "Admin", "Admin123!", "SuperAdmin"),
            ("user@communitycar.com", "Normal", "User", "User123!", "User"),
            ("ahmed.ali@example.com", "Ahmed", "Ali", "User123!", "User"),
            ("sarah.smith@example.com", "Sarah", "Smith", "User123!", "User"),
            ("mohamed.hassan@example.com", "Mohamed", "Hassan", "User123!", "User"),
            ("fatima.zara@example.com", "Fatima", "Zara", "User123!", "User"),
            ("john.doe@example.com", "John", "Doe", "User123!", "User"),
            ("jane.doe@example.com", "Jane", "Doe", "User123!", "User"),
            ("omar.fayed@example.com", "Omar", "Fayed", "User123!", "User"),
            ("layla.mahmoud@example.com", "Layla", "Mahmoud", "User123!", "User"),
            ("youssef.mansour@example.com", "Youssef", "Mansour", "User123!", "User"),
            ("nour.elsayed@example.com", "Nour", "Elsayed", "User123!", "User"),
            ("khaled.ibrahim@example.com", "Khaled", "Ibrahim", "User123!", "User"),
            ("mona.zakaria@example.com", "Mona", "Zakaria", "User123!", "User"),
            ("tarek.hamad@example.com", "Tarek", "Hamad", "User123!", "User"),
            ("reem.saeed@example.com", "Reem", "Saeed", "User123!", "User"),
            ("zane.miller@example.com", "Zane", "Miller", "User123!", "User")
        };

        foreach (var (email, firstName, lastName, password, role) in users)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                logger.LogInformation("Creating user: {Email}", email);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    logger.LogInformation("User {Email} created.", email);
                }
            }
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Categories.Any())
        {
            logger.LogInformation("Seeding categories...");
            
            var categories = new[]
            {
                new Category("Engine & Transmission", "engine-transmission", "Discussions related to engine repair, transmission issues, and performance.", "bi bi-gear-fill", "#dc3545", 1),
                new Category("Electrical & Lights", "electrical-lights", "Battery, alternator, wiring, and lighting systems.", "bi bi-lightning-fill", "#ffc107", 2),
                new Category("Brakes & Suspension", "brakes-suspension", "Braking systems, shocks, struts, and wheel alignment.", "bi bi-slash-circle-fill", "#fd7e14", 3),
                new Category("Interior & Accessories", "interior-accessories", "Dashboard, seats, audio systems, and modifications.", "bi bi-ui-checks-grid", "#198754", 4),
                new Category("Maintenance & Service", "maintenance-service", "Oil changes, scheduled service, and general upkeep.", "bi bi-wrench-adjustable", "#0d6efd", 5),
                new Category("General Discussion", "general-discussion", "General car talk, news, and off-topic discussions.", "bi bi-chat-dots-fill", "#6c757d", 6)
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("Categories seeded successfully.");
        }
    }

    private static async Task SeedGroupsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        if (!context.CommunityGroups.Any())
        {
            logger.LogInformation("Seeding groups...");

            var users = await context.Users.Where(u => !u.IsDeleted).Take(10).ToListAsync();
            if (users.Count < 5) return;

            var groups = new List<CommunityGroup>
            {
                new CommunityGroup("Classic Car Enthusiasts", "A community for lovers of vintage and classic automobiles.", users[0].Id, false),
                new CommunityGroup("Electric Vehicle Owners", "Connect with fellow EV owners.", users[1].Id, false),
                new CommunityGroup("DIY Car Repair", "Learn and share DIY car repair techniques.", users[2].Id, false),
                new CommunityGroup("Performance Tuning", "For those who want more power!", users[3].Id, false),
                new CommunityGroup("Off-Road Adventures", "4x4 enthusiasts unite!", users[4].Id, false)
            };

            await context.CommunityGroups.AddRangeAsync(groups);
            await context.SaveChangesAsync();
            logger.LogInformation("Groups seeded successfully.");
        }
    }

    private static async Task SeedPostsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Posts.Any())
        {
            logger.LogInformation("Seeding posts...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 2) return;

            var posts = new List<Post>
            {
                new Post("Best tires for winter driving?", "Looking for recommendations for winter tires.", PostType.Text, users[0].Id),
                new Post("My new project car!", "Just picked up this beauty.", PostType.Image, users[1].Id)
            };

            posts.ForEach(p => p.Publish());
            await context.Posts.AddRangeAsync(posts);
            await context.SaveChangesAsync();
            logger.LogInformation("Posts seeded successfully.");
        }
    }

    private static async Task SeedGuidesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Guides.Any())
        {
            logger.LogInformation("Seeding guides...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 2) return;

            var guides = new List<Guide>
            {
                new Guide("How to Change Your Oil", "## Steps\n1. Lift car\n2. Drain oil\n3. Replace filter", "Beginner guide", "Maintenance", users[0].Id, GuideDifficulty.Beginner, 45),
                new Guide("Replacing Brake Pads", "Detailed brake pad replacement instructions", "Save money", "Maintenance", users[1].Id, GuideDifficulty.Intermediate, 90)
            };

            guides.ForEach(g => g.Publish());
            await context.Guides.AddRangeAsync(guides);
            await context.SaveChangesAsync();
            logger.LogInformation("Guides seeded successfully.");
        }
    }

    private static async Task SeedQuestionsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Questions.Any())
        {
            logger.LogInformation("Seeding questions...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 2) return;

            var questions = new List<Question>
            {
                new Question("Engine making clicking noise?", "My engine started making a rhythmic clicking noise.", users[0].Id),
                new Question("Recommended dash cams 2026?", "Looking for a reliable dash cam.", users[1].Id)
            };

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();
            logger.LogInformation("Questions seeded successfully.");
        }
    }

    private static async Task SeedMapPointsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.MapPoints.Any())
        {
            logger.LogInformation("Seeding map points...");
            var users = await context.Users.Take(5).ToListAsync();

            var points = new List<MapPoint>
            {
                new MapPoint("Central Park Meetup", new Location(40.785091, -73.968285, "Central Park, NY"), MapPointType.MeetingPoint, users.FirstOrDefault()?.Id, "Weekend meets"),
                new MapPoint("Supercharger Station", new Location(40.758896, -73.985130, "Times Square, NY"), MapPointType.ChargingStation, null, "12 V3 Superchargers")
            };

            points.ForEach(p => { p.Publish(); p.Verify(); });
            await context.MapPoints.AddRangeAsync(points);
            await context.SaveChangesAsync();
            logger.LogInformation("Map points seeded successfully.");
        }
    }

    private static async Task SeedNewsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.NewsArticles.Any())
        {
            logger.LogInformation("Seeding news...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 1) return;

            var news = new List<NewsArticle>
            {
                new NewsArticle("New Model Released", "Latest model announcement...", "Breaking news", NewsCategory.Industry, users[0].Id),
                new NewsArticle("Winter Driving Tips", "Stay safe this winter...", "Safety advice", NewsCategory.Tips, users[0].Id)
            };

            news.ForEach(n => n.Publish());
            await context.NewsArticles.AddRangeAsync(news);
            await context.SaveChangesAsync();
            logger.LogInformation("News seeded successfully.");
        }
    }

    private static async Task SeedEventsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Events.Any())
        {
            logger.LogInformation("Seeding events...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 2) return;

            var now = DateTimeOffset.UtcNow;
            var events = new List<CommunityEvent>
            {
                new CommunityEvent("Classic Car Show", "Annual classic car show", now.AddDays(15), now.AddDays(15).AddHours(6), "Central Park", users[0].Id, EventCategory.Meetup, 100, false),
                new CommunityEvent("EV Workshop", "Learn EV maintenance", now.AddDays(7), now.AddDays(7).AddHours(3), "Tech Hub", users[1].Id, EventCategory.Workshop, 30, true)
            };

            events.ForEach(e => e.Publish());
            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();
            logger.LogInformation("Events seeded successfully.");
        }
    }

    private static async Task SeedFriendshipsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.Friendships.Any())
        {
            logger.LogInformation("Seeding friendships...");
            var users = await context.Users.Take(10).ToListAsync();
            if (users.Count < 5) return;

            var friendships = new List<Friendship>();
            var rand = new Random();

            for (int i = 0; i < Math.Min(users.Count, 5); i++)
            {
                var targets = users.Where(u => u.Id != users[i].Id).OrderBy(x => rand.Next()).Take(3).ToList();
                foreach (var target in targets)
                {
                    var f = new Friendship(users[i].Id, target.Id);
                    if (rand.Next(100) < 70) f.Accept();
                    friendships.Add(f);
                }
            }

            await context.Friendships.AddRangeAsync(friendships);
            await context.SaveChangesAsync();
            logger.LogInformation("Friendships seeded successfully.");
        }
    }

    private static async Task SeedChatsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!context.ChatRooms.Any())
        {
            logger.LogInformation("Seeding chats...");
            var users = await context.Users.Take(5).ToListAsync();
            if (users.Count < 2) return;

            var privateChat = new ChatRoom { Name = "Private Chat", IsGroup = false, CreatedBy = users[0].Id };
            privateChat.Members.Add(new ChatRoomMember { UserId = users[0].Id, JoinedAt = DateTimeOffset.UtcNow });
            privateChat.Members.Add(new ChatRoomMember { UserId = users[1].Id, JoinedAt = DateTimeOffset.UtcNow });
            privateChat.Messages.Add(new ChatMessage { SenderId = users[0].Id, Content = "Hey!", Type = CommunityCar.Domain.Enums.Communications.chats.MessageType.Text });

            await context.ChatRooms.AddAsync(privateChat);
            await context.SaveChangesAsync();
            logger.LogInformation("Chats seeded successfully.");
        }
    }
}
