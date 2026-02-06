using Microsoft.Extensions.DependencyInjection;
using CommunityCar.Infrastructure.Data;

namespace CommunityCar.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created and migrations are applied
        // await context.Database.MigrateAsync();

        // Add seeding logic here
        await Task.CompletedTask;
    }
}
