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
}
