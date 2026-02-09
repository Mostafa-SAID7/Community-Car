using CommunityCar.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using CommunityCar.Web.Infrastructure.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using CommunityCar.Infrastructure.Mappings;
using CommunityCar.Web.Filters;
using CommunityCar.Web.Middleware;
using CommunityCar.Infrastructure.Data.Seed;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using CommunityCar.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using CommunityCar.Web.Infrastructure.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using CommunityCar.Infrastructure.Mappings;
using CommunityCar.Web.Filters;
using CommunityCar.Web.Middleware;
using CommunityCar.Infrastructure.Data.Seed;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

    builder.Services.AddSignalR(options =>
    {
        // Configure keep-alive to prevent 1006 errors
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<CommunityCar.Domain.Interfaces.Community.IQuestionHubService, CommunityCar.Mvc.Services.QuestionHubService>();
    builder.Services.AddAutoMapper(typeof(FriendshipProfile).Assembly);

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CommunityCar.Web.Areas.Identity.Validators.RegisterValidator>();

    // Localization Configuration
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
        options.DefaultRequestCulture = new RequestCulture("en");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;

        // Add RouteDataRequestCultureProvider to prioritize culture from URL
        options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
    });

    var app = builder.Build();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var env = services.GetRequiredService<IWebHostEnvironment>();
        
        try
        {
            var context = services.GetRequiredService<CommunityCar.Infrastructure.Data.ApplicationDbContext>();
            
            // Always run migrations
            await context.Database.MigrateAsync();
            
            // Check if seeding is enabled via configuration (defaults to IsDevelopment)
            var shouldSeed = app.Configuration.GetValue<bool>("Database:EnableSeeding", env.IsDevelopment());
            
            if (shouldSeed)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Seeding is enabled. Starting database seed...");
                
                try
                {
                    await services.SeedDatabase();
                    logger.LogInformation("✓ DbSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ DbSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await FriendshipSeeder.SeedAsync(context);
                    logger.LogInformation("✓ FriendshipSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ FriendshipSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await EventSeeder.SeedAsync(context);
                    logger.LogInformation("✓ EventSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ EventSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await PostSeeder.SeedAsync(context);
                    logger.LogInformation("✓ PostSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ PostSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await GuideSeeder.SeedAsync(context);
                    logger.LogInformation("✓ GuideSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ GuideSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await ReviewSeeder.SeedAsync(context);
                    logger.LogInformation("✓ ReviewSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ ReviewSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await QuestionSeeder.SeedAsync(context);
                    logger.LogInformation("✓ QuestionSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ QuestionSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await GroupSeeder.SeedAsync(context);
                    logger.LogInformation("✓ GroupSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ GroupSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await MapPointSeeder.SeedAsync(context);
                    logger.LogInformation("✓ MapPointSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ MapPointSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await ChatSeeder.SeedAsync(context);
                    logger.LogInformation("✓ ChatSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ ChatSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    await NewsSeeder.SeedAsync(context);
                    logger.LogInformation("✓ NewsSeeder completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ NewsSeeder failed: {Message}", ex.Message);
                }

                try
                {
                    // Train ML Models
                    var mlPipelineService = services.GetRequiredService<CommunityCar.Infrastructure.Interfaces.ML.IMLPipelineService>();
                    await mlPipelineService.TrainModelsAsync();
                    logger.LogInformation("✓ ML Model training completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "✗ ML Model training failed: {Message}", ex.Message);
                }
                
                logger.LogInformation("Database seeding completed.");
            }
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Seeding is disabled via configuration.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            
            // Log prominently in production but allow app to start
            if (!env.IsDevelopment())
            {
                logger.LogCritical("PRODUCTION STARTUP WARNING: Database migration or seeding failed. Application will continue but may have connectivity issues.");
            }
        }
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseMiddleware<CultureRedirectMiddleware>();

    app.UseRouting();

    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture("en")
        .AddSupportedCultures("en", "ar")
        .AddSupportedUICultures("en", "ar");

    // Add RouteDataRequestCultureProvider to prioritize culture from URL
    localizationOptions.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());

    app.UseRequestLocalization(localizationOptions);

    // Handle status code errors (404, 401, etc.)
    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    app.UseAuthentication();
    app.UseAuthorization();

    // Culture/Language switching route (must be before other routes)
    app.MapControllerRoute(
        name: "culture_switch",
        pattern: "Culture/SetLanguage",
        defaults: new { controller = "Culture", action = "SetLanguage" });

    app.MapControllerRoute(
        name: "culture_switch_localized",
        pattern: "{culture}/Culture/SetLanguage",
        defaults: new { controller = "Culture", action = "SetLanguage" });

    app.MapControllerRoute(
        name: "areas_localized",
        pattern: "{culture:alpha}/{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default_localized",
        pattern: "{culture:alpha}/{controller=Feed}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Feed}/{action=Index}/{id?}");

    app.MapHub<CommunityCar.Mvc.Hubs.QuestionHub>("/questionHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.NotificationHub>("/notificationHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.ChatHub>("/chatHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/friendHub");

    app.Run();
}
catch (Exception ex)
{
    // Fatal startup error logging
    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "startup_fatal_error.txt");
    File.WriteAllText(logPath, DateTime.UtcNow + ": " + ex.ToString());
    throw; // Re-throw to let the process exit
}
