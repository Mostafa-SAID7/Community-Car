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
        string[] roleNames = { "SuperAdmin", "User" };
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
}
