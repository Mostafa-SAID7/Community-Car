using CommunityCar.Domain.Entities.Community.events;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.events;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class EventSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Don't re-seed if we already have events
        if (await context.Events.CountAsync() > 3) return;

        // Get users to be event organizers
        var users = await context.Users.Where(u => !u.IsDeleted).Take(5).ToListAsync();
        if (users.Count < 2) return;

        // Clear existing events to avoid duplicates
        if (await context.Events.AnyAsync())
        {
            context.Events.RemoveRange(context.Events);
            await context.SaveChangesAsync();
        }

        var events = new List<CommunityEvent>();
        var now = DateTimeOffset.UtcNow;

        // Event 1: Car Show - Upcoming
        var carShow = new CommunityEvent(
            "Classic Car Show & Meetup",
            "Join us for the annual classic car show! Display your vintage vehicles, meet fellow enthusiasts, and enjoy a day of automotive history. All classic car owners welcome!",
            now.AddDays(15),
            now.AddDays(15).AddHours(6),
            "Central City Park",
            users[0].Id,
            EventCategory.Meetup,
            maxAttendees: 100,
            isOnline: false
        );
        carShow.SetAddress("123 Park Avenue, Central City", 40.7128m, -74.0060m);
        carShow.SetImageUrl("/images/events/classic-car-show.jpg");
        carShow.SetFeatured(true);
        carShow.Publish();
        events.Add(carShow);

        // Event 2: EV Workshop - Upcoming
        var evWorkshop = new CommunityEvent(
            "Electric Vehicle Maintenance Workshop",
            "Learn the basics of EV maintenance from certified technicians. Topics include battery care, charging best practices, and troubleshooting common issues.",
            now.AddDays(7),
            now.AddDays(7).AddHours(3),
            "Tech Hub Community Center",
            users[1].Id,
            EventCategory.Workshop,
            maxAttendees: 30,
            isOnline: true
        );
        evWorkshop.SetOnlineUrl("https://meet.communitycar.com/ev-workshop");
        evWorkshop.SetImageUrl("/images/events/ev-workshop.jpg");
        evWorkshop.Publish();
        events.Add(evWorkshop);

        // Event 3: Road Trip - Upcoming
        var roadTrip = new CommunityEvent(
            "Weekend Mountain Road Trip",
            "Hit the scenic mountain roads for a weekend adventure! We'll convoy through beautiful landscapes, stop at interesting spots, and enjoy great company. All car types welcome!",
            now.AddDays(30),
            now.AddDays(32),
            "Mountain View Trail",
            users[2].Id,
            EventCategory.RoadTrip,
            maxAttendees: 20,
            isOnline: false
        );
        roadTrip.SetAddress("Mountain View Trail Head, North Region", 39.7392m, -104.9903m);
        roadTrip.SetImageUrl("/images/events/mountain-road-trip.jpg");
        roadTrip.SetFeatured(true);
        roadTrip.Publish();
        events.Add(roadTrip);

        // Event 4: Racing Event - Upcoming
        var racing = new CommunityEvent(
            "Community Track Day",
            "Experience the thrill of the track! Join us for a safe, supervised track day where you can push your car to its limits. All skill levels welcome, safety briefing included.",
            now.AddDays(45),
            now.AddDays(45).AddHours(8),
            "Speedway Racing Circuit",
            users[3].Id,
            EventCategory.Racing,
            maxAttendees: 50,
            isOnline: false
        );
        racing.SetAddress("Speedway Circuit, Track Road 100", 34.0522m, -118.2437m);
        racing.SetImageUrl("/images/events/track-day.jpg");
        racing.Publish();
        events.Add(racing);

        // Event 5: Charity Drive - Past Event
        var charity = new CommunityEvent(
            "Cars for Charity Parade",
            "Thank you to everyone who participated in our charity parade! Together we raised over $5,000 for local children's hospitals.",
            now.AddDays(-30),
            now.AddDays(-30).AddHours(4),
            "Downtown Main Street",
            users[4].Id,
            EventCategory.Charity,
            maxAttendees: 80,
            isOnline: false
        );
        charity.SetAddress("Main Street, Downtown District", 37.7749m, -122.4194m);
        charity.SetImageUrl("/images/events/charity-parade.jpg");
        charity.Publish();
        charity.Complete();
        events.Add(charity);

        // Event 6: Modification Workshop - Upcoming
        var modWorkshop = new CommunityEvent(
            "DIY Car Modification Basics",
            "Learn how to safely modify your car with guidance from experienced builders. We'll cover intake systems, exhaust upgrades, and suspension basics.",
            now.AddDays(20),
            now.AddDays(20).AddHours(4),
            "Garage 54 Workshop",
            users[0].Id,
            EventCategory.Workshop,
            maxAttendees: 25,
            isOnline: false
        );
        modWorkshop.SetAddress("54 Workshop Street, Industrial Zone", 41.8781m, -87.6298m);
        modWorkshop.Publish();
        events.Add(modWorkshop);

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
    }
}
