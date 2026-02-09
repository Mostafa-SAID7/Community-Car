using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Entities.Community.guides;
using CommunityCar.Domain.Entities.Community.reviews;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Domain.Entities.Community.maps;
using CommunityCar.Domain.Entities.Community.news;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Entities.Communications.chats;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class PostSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Posts.CountAsync() > 5) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;

        var posts = new List<Post>();
        
        // Post 1: General Discussion
        var post1 = new Post(
            "Best tires for winter driving?",
            "I'm looking for recommendations for winter tires for my sedan. Does anyone have experience with Michelin X-Ice vs Bridgestone Blizzak?",
            PostType.Text,
            users[0].Id
        );
        post1.Publish();
        post1.IncrementViews(); post1.IncrementLikes();
        posts.Add(post1);

        // Post 2: Car Photo
        var post2 = new Post(
            "My new project car!",
            "Just picked up this beauty. It needs some work but I'm excited to get started.",
            PostType.Image,
            users[1].Id
        );
        post2.SetMedia("/images/posts/project-car.jpg", null);
        post2.Publish();
        posts.Add(post2);

        // Post 3: Link
        var post3 = new Post(
            "Check out this EV review",
            "Interesting perspective on the new charging infrastructure challenges.",
            PostType.Link,
            users[2].Id
        );
        post3.SetLink("https://example.com/ev-review", "EV Charging Infrastructure Analysis", "A deep dive into 2026 charging challenges.");
        post3.Publish();
        posts.Add(post3);

        await context.Posts.AddRangeAsync(posts);
        await context.SaveChangesAsync();
    }
}

public static class GuideSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Guides.CountAsync() > 2) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;

        var guides = new List<Guide>();

        // Guide 1: Oil Change
        var guide1 = new Guide(
            "How to Change Your Oil",
            "## Steps to Change Oil\n1. Lift the car\n2. Drain oil\n3. Replace filter\n4. Add new oil",
            "A comprehensive guide for beginners on changing engine oil.",
            "Maintenance",
            users[0].Id,
            GuideDifficulty.Beginner,
            45
        );
        guide1.Publish();
        guides.Add(guide1);

        // Guide 2: Brake Pad Replacement
        var guide2 = new Guide(
            "Replacing Brake Pads",
            "Detailed instructions on safety and procedure for replacing front brake pads...",
            "Save money by doing your own brake job safely.",
            "Maintenance",
            users[1].Id,
            GuideDifficulty.Intermediate,
            90
        );
        guide2.Publish();
        guides.Add(guide2);

        await context.Guides.AddRangeAsync(guides);
        await context.SaveChangesAsync();
    }
}

public static class ReviewSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Reviews.CountAsync() > 2) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;

        // Need IDs of things to review. For now, we'll review some random GUIDs or placeholders
        // In a real scenario, we'd fetch MapPoints or Events to review.
    }
}

public static class QuestionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Questions.CountAsync() > 2) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;

        var questions = new List<Question>();

        var q1 = new Question(
            "Engine making clicking noise?",
            "My engine started making a rhythmic clicking noise that speeds up with RPM. Any ideas?",
            users[0].Id
        );
        questions.Add(q1);

        var q2 = new Question(
            "Recommended dash cams 2026?",
            "Looking for a reliable dash cam with cloud backup features.",
            users[1].Id
        );
        questions.Add(q2);

        await context.Questions.AddRangeAsync(questions);
        await context.SaveChangesAsync();
    }
}

public static class GroupSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.CommunityGroups.CountAsync() > 2) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 1) return;

        var groups = new List<CommunityGroup>();

        var g1 = new CommunityGroup(
            "JDM Enthusiasts",
            "A group for fans of Japanese Domestic Market vehicles.",
            users[0].Id,
            false
        );
        g1.SetImage("/images/groups/jdm.jpg");
        groups.Add(g1);

        var g2 = new CommunityGroup(
            "Off-Road Adventures",
            "Sharing trails, tips, and build guides for 4x4 vehicles.",
            users[1].Id,
            false
        );
        g2.SetImage("/images/groups/offroad.jpg");
        groups.Add(g2);

        await context.CommunityGroups.AddRangeAsync(groups);
        await context.SaveChangesAsync();
    }
}

public static class MapPointSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.MapPoints.CountAsync() > 3) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        
        var points = new List<MapPoint>();

        var p1 = new MapPoint(
            "Central Park Meetup Spot",
            new Location(40.785091, -73.968285, "Central Park, NY"),
            MapPointType.MeetingPoint,
            users.FirstOrDefault()?.Id,
            "Popular spot for weekend morning meets."
        );
        p1.Publish();
        p1.Verify();
        points.Add(p1);

        var p2 = new MapPoint(
            "Supercharger Station - Downtown",
            new Location(40.758896, -73.985130, "Times Square, NY"),
            MapPointType.ChargingStation,
            null,
            "12 V3 Superchargers available 24/7."
        );
        p2.Publish();
        p2.Verify();
        points.Add(p2);
        
        var p3 = new MapPoint(
            "AutoWorks Service Center",
            new Location(40.748817, -73.985428, "Empire State Building, NY"),
            MapPointType.ServiceCenter,
            users.LastOrDefault()?.Id,
            "Best mechanics in town for European cars."
        );
        p3.Publish();
        p3.Verify();
        points.Add(p3);

        await context.MapPoints.AddRangeAsync(points);
        await context.SaveChangesAsync();
    }
}

public static class ChatSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ChatRooms.CountAsync() > 0) return;
        
        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;
        
        // 1. Private Chat between User 1 and User 2
        var privateChat = new ChatRoom
        {
            Name = "Private Chat",
            IsGroup = false,
            CreatedBy = users[0].Id
        };
        
        privateChat.Members.Add(new ChatRoomMember { UserId = users[0].Id, JoinedAt = DateTimeOffset.UtcNow });
        privateChat.Members.Add(new ChatRoomMember { UserId = users[1].Id, JoinedAt = DateTimeOffset.UtcNow });
        
        privateChat.Messages.Add(new ChatMessage 
        { 
            SenderId = users[0].Id, 
            Content = "Hey, are you going to the car meet?", 
            Type = CommunityCar.Domain.Enums.Communications.chats.MessageType.Text
        });
        
        privateChat.Messages.Add(new ChatMessage 
        { 
            SenderId = users[1].Id, 
            Content = "Yeah, definitely! Bringing the new project car.", 
            Type = CommunityCar.Domain.Enums.Communications.chats.MessageType.Text
        });
        
        await context.ChatRooms.AddAsync(privateChat);
        
        // 2. Group Chat
        var groupChat = new ChatRoom
        {
            Name = "Weekend Racers",
            IsGroup = true,
            CreatedBy = users[0].Id
        };
        
        groupChat.Members.Add(new ChatRoomMember { UserId = users[0].Id, JoinedAt = DateTimeOffset.UtcNow });
        groupChat.Members.Add(new ChatRoomMember { UserId = users[1].Id, JoinedAt = DateTimeOffset.UtcNow });
        if (users.Count > 2)
            groupChat.Members.Add(new ChatRoomMember { UserId = users[2].Id, JoinedAt = DateTimeOffset.UtcNow });
            
        groupChat.Messages.Add(new ChatMessage 
        { 
            SenderId = users[0].Id, 
            Content = "Welcome to the group everyone!", 
            Type = CommunityCar.Domain.Enums.Communications.chats.MessageType.Text
        });
        
        await context.ChatRooms.AddAsync(groupChat);
        await context.SaveChangesAsync();
    }
}

public static class NewsSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.NewsArticles.CountAsync() > 2) return;

        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 1) return;

        var news = new List<NewsArticle>();

        var n1 = new NewsArticle(
            "New Model Released",
            "The manufacturer has just announced the release of their latest model...",
            "Breaking news about the latest car release.",
            NewsCategory.Industry,
            users[0].Id
        );
        n1.Publish();
        n1.SetImage("/images/news/new-model.jpg");
        news.Add(n1);

        var n2 = new NewsArticle(
            "Winter Driving Tips",
            "Stay safe on the roads this winter with our top 10 driving tips...",
            "Essential safety advice for winter conditions.",
            NewsCategory.Tips,
            users[0].Id
        );
        n2.Publish();
        n2.SetImage("/images/news/winter-tips.jpg");
        news.Add(n2);

        await context.NewsArticles.AddRangeAsync(news);
        await context.SaveChangesAsync();
    }
}
