using Microsoft.Extensions.DependencyInjection;
using CommunityCar.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Identity.Roles;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class DbSeeder
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

            // 1. Seed Roles (using context directly to be safe)
            await SeedRolesAsync(context, roleManager, logger);

            // 2. Seed Users
            await SeedUsersAsync(userManager, logger);

            // 3. Seed Mock Users
            await SeedMockUsersAsync(userManager, logger);

            // 4. Seed Categories
            await SeedCategoriesAsync(context, logger);

            // 5. Seed Groups
            await SeedGroupsAsync(context, userManager, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : "";
            logger.LogError(ex, "An error occurred during seeding: {Message}{Inner}", ex.Message, inner);
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
                logger.LogInformation("Role {RoleName} created successfully via Context.", roleName);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // SuperAdmin
        if (await userManager.FindByEmailAsync("admin@communitycar.com") == null)
        {
            logger.LogInformation("Creating superadmin user...");
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin@communitycar.com",
                Email = "admin@communitycar.com",
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                logger.LogInformation("SuperAdmin user created.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    logger.LogError("Error creating superadmin: {ErrorMessage}", error.Description);
                }
            }
        }

        // Normal User
        if (await userManager.FindByEmailAsync("user@communitycar.com") == null)
        {
            logger.LogInformation("Creating normal user...");
            var normalUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user@communitycar.com",
                Email = "user@communitycar.com",
                FirstName = "Normal",
                LastName = "User",
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await userManager.CreateAsync(normalUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(normalUser, "User");
                logger.LogInformation("Normal user created.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    logger.LogError("Error creating normal user: {ErrorMessage}", error.Description);
                }
            }
        }
    }

    private static async Task SeedMockUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var mockUsers = new List<(string Email, string FirstName, string LastName, string Password)>
        {
            ("ahmed.ali@example.com", "Ahmed", "Ali", "User123!"),
            ("sarah.smith@example.com", "Sarah", "Smith", "User123!"),
            ("mohamed.hassan@example.com", "Mohamed", "Hassan", "User123!"),
            ("fatima.zara@example.com", "Fatima", "Zara", "User123!"),
            ("john.doe@example.com", "John", "Doe", "User123!"),
            ("jane.doe@example.com", "Jane", "Doe", "User123!"),
            ("omar.fayed@example.com", "Omar", "Fayed", "User123!"),
            ("layla.mahmoud@example.com", "Layla", "Mahmoud", "User123!"),
            ("youssef.mansour@example.com", "Youssef", "Mansour", "User123!"),
            ("nour.elsayed@example.com", "Nour", "Elsayed", "User123!"),
            ("khaled.ibrahim@example.com", "Khaled", "Ibrahim", "User123!"),
            ("mona.zakaria@example.com", "Mona", "Zakaria", "User123!"),
            ("tarek.hamad@example.com", "Tarek", "Hamad", "User123!"),
            ("reem.saeed@example.com", "Reem", "Saeed", "User123!"),
            ("zane.miller@example.com", "Zane", "Miller", "User123!")
        };

        foreach (var mock in mockUsers)
        {
            if (await userManager.FindByEmailAsync(mock.Email) == null)
            {
                logger.LogInformation("Creating mock user: {Email}", mock.Email);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = mock.Email,
                    Email = mock.Email,
                    FirstName = mock.FirstName,
                    LastName = mock.LastName,
                    EmailConfirmed = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, mock.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    logger.LogInformation("Mock user {Email} created.", mock.Email);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("Error creating mock user {Email}: {ErrorMessage}", mock.Email, error.Description);
                    }
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
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "Engine & Transmission", 
                    "engine-transmission", 
                    "Discussions related to engine repair, transmission issues, and performance.", 
                    "bi bi-gear-fill", 
                    "#dc3545", 
                    1),
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "Electrical & Lights", 
                    "electrical-lights", 
                    "Battery, alternator, wiring, and lighting systems.", 
                    "bi bi-lightning-fill", 
                    "#ffc107", 
                    2),
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "Brakes & Suspension", 
                    "brakes-suspension", 
                    "Braking systems, shocks, struts, and wheel alignment.", 
                    "bi bi-slash-circle-fill", 
                    "#fd7e14", 
                    3),
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "Interior & Accessories", 
                    "interior-accessories", 
                    "Dashboard, seats, audio systems, and modifications.", 
                    "bi bi-ui-checks-grid", 
                    "#198754", 
                    4),
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "Maintenance & Service", 
                    "maintenance-service", 
                    "Oil changes, scheduled service, and general upkeep.", 
                    "bi bi-wrench-adjustable", 
                    "#0d6efd", 
                    5),
                new CommunityCar.Domain.Entities.Community.Common.Category(
                    "General Discussion", 
                    "general-discussion", 
                    "General car talk, news, and off-topic discussions.", 
                    "bi bi-chat-dots-fill", 
                    "#6c757d", 
                    6)
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

            // Get users for group creation
            var normalUser = await userManager.FindByEmailAsync("user@communitycar.com");
            var ahmed = await userManager.FindByEmailAsync("ahmed.ali@example.com");
            var sarah = await userManager.FindByEmailAsync("sarah.smith@example.com");
            var mohamed = await userManager.FindByEmailAsync("mohamed.hassan@example.com");
            var fatima = await userManager.FindByEmailAsync("fatima.zara@example.com");
            var john = await userManager.FindByEmailAsync("john.doe@example.com");
            var jane = await userManager.FindByEmailAsync("jane.doe@example.com");
            var omar = await userManager.FindByEmailAsync("omar.fayed@example.com");
            var layla = await userManager.FindByEmailAsync("layla.mahmoud@example.com");
            var youssef = await userManager.FindByEmailAsync("youssef.mansour@example.com");

            if (normalUser == null || ahmed == null || sarah == null || mohamed == null)
            {
                logger.LogWarning("Required users not found for group seeding. Skipping groups.");
                return;
            }

            // Create groups
            var groups = new List<CommunityCar.Domain.Entities.Community.groups.CommunityGroup>
            {
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Classic Car Enthusiasts",
                    "A community for lovers of vintage and classic automobiles. Share restoration projects, maintenance tips, and celebrate automotive history.",
                    normalUser.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Electric Vehicle Owners",
                    "Connect with fellow EV owners. Discuss charging infrastructure, range optimization, and the future of electric mobility.",
                    ahmed.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "DIY Car Repair",
                    "Learn and share DIY car repair techniques. From oil changes to brake replacements, we help each other save money and learn new skills.",
                    sarah.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Performance Tuning",
                    "For those who want more power! Discuss engine modifications, ECU tuning, turbocharging, and track day experiences.",
                    mohamed.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Off-Road Adventures",
                    "4x4 enthusiasts unite! Share trail recommendations, vehicle modifications, and off-road adventure stories.",
                    fatima!.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Luxury Car Club",
                    "An exclusive community for luxury and exotic car owners. Share experiences, maintenance tips, and lifestyle discussions.",
                    john!.Id,
                    true), // Private group
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "First Time Car Buyers",
                    "New to car ownership? Get advice on buying your first car, understanding maintenance, and navigating insurance.",
                    jane!.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Car Photography",
                    "Showcase your automotive photography skills. Share tips on capturing the perfect car shot and discuss camera gear.",
                    omar!.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Eco-Friendly Driving",
                    "Discuss fuel efficiency, hybrid vehicles, eco-driving techniques, and reducing your carbon footprint.",
                    layla!.Id,
                    false),
                
                new CommunityCar.Domain.Entities.Community.groups.CommunityGroup(
                    "Track Day Warriors",
                    "For those who take their cars to the track. Share lap times, driving techniques, and track day preparation tips.",
                    youssef!.Id,
                    true) // Private group
            };

            await context.CommunityGroups.AddRangeAsync(groups);
            await context.SaveChangesAsync();
            logger.LogInformation("Groups created successfully.");

            // Add members to groups
            var groupMembers = new List<CommunityCar.Domain.Entities.Community.groups.GroupMember>();

            // Classic Car Enthusiasts - Public group with many members
            var classicCarGroup = groups[0];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(classicCarGroup.Id, normalUser.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(classicCarGroup.Id, ahmed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Moderator),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(classicCarGroup.Id, sarah.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(classicCarGroup.Id, mohamed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(classicCarGroup.Id, john!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            classicCarGroup.IncrementMemberCount(); // +4 more members (creator already counted)
            classicCarGroup.IncrementMemberCount();
            classicCarGroup.IncrementMemberCount();
            classicCarGroup.IncrementMemberCount();

            // Electric Vehicle Owners
            var evGroup = groups[1];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(evGroup.Id, ahmed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(evGroup.Id, fatima!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(evGroup.Id, layla!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            evGroup.IncrementMemberCount();
            evGroup.IncrementMemberCount();

            // DIY Car Repair - Popular group
            var diyGroup = groups[2];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, sarah.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, normalUser.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Moderator),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, jane!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, omar!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, youssef!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(diyGroup.Id, ahmed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            diyGroup.IncrementMemberCount();
            diyGroup.IncrementMemberCount();
            diyGroup.IncrementMemberCount();
            diyGroup.IncrementMemberCount();
            diyGroup.IncrementMemberCount();

            // Performance Tuning
            var perfGroup = groups[3];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(perfGroup.Id, mohamed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(perfGroup.Id, youssef!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Moderator),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(perfGroup.Id, john!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            perfGroup.IncrementMemberCount();
            perfGroup.IncrementMemberCount();

            // Off-Road Adventures
            var offRoadGroup = groups[4];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(offRoadGroup.Id, fatima!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(offRoadGroup.Id, omar!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(offRoadGroup.Id, mohamed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            offRoadGroup.IncrementMemberCount();
            offRoadGroup.IncrementMemberCount();

            // Luxury Car Club - Private
            var luxuryGroup = groups[5];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(luxuryGroup.Id, john!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(luxuryGroup.Id, sarah.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            luxuryGroup.IncrementMemberCount();

            // First Time Car Buyers
            var firstTimeGroup = groups[6];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(firstTimeGroup.Id, jane!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(firstTimeGroup.Id, normalUser.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Moderator),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(firstTimeGroup.Id, layla!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            firstTimeGroup.IncrementMemberCount();
            firstTimeGroup.IncrementMemberCount();

            // Car Photography
            var photoGroup = groups[7];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(photoGroup.Id, omar!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(photoGroup.Id, ahmed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(photoGroup.Id, fatima!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(photoGroup.Id, jane!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            photoGroup.IncrementMemberCount();
            photoGroup.IncrementMemberCount();
            photoGroup.IncrementMemberCount();

            // Eco-Friendly Driving
            var ecoGroup = groups[8];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(ecoGroup.Id, layla!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(ecoGroup.Id, ahmed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(ecoGroup.Id, sarah.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            ecoGroup.IncrementMemberCount();
            ecoGroup.IncrementMemberCount();

            // Track Day Warriors - Private
            var trackGroup = groups[9];
            groupMembers.AddRange(new[]
            {
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(trackGroup.Id, youssef!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Admin),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(trackGroup.Id, mohamed.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Moderator),
                new CommunityCar.Domain.Entities.Community.groups.GroupMember(trackGroup.Id, john!.Id, CommunityCar.Domain.Enums.Community.groups.GroupMemberRole.Member)
            });
            trackGroup.IncrementMemberCount();
            trackGroup.IncrementMemberCount();

            await context.GroupMembers.AddRangeAsync(groupMembers);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Group members added successfully. Total groups: {GroupCount}, Total memberships: {MemberCount}", 
                groups.Count, groupMembers.Count);
        }
    }
}
