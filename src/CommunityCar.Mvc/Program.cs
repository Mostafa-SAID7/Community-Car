using CommunityCar.Infrastructure.Data.Seed;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Kestrel to accept larger requests (for file uploads)
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
    });

    // Configure form options for larger file uploads
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 52428800; // 50 MB
        options.ValueLengthLimit = 52428800;
        options.MultipartHeadersLengthLimit = 52428800;
    });

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

    // Run migrations and seed database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<CommunityCar.Infrastructure.Data.ApplicationDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully.");
            
            // Check if seeding is enabled
            var configuration = services.GetRequiredService<IConfiguration>();
            var enableSeeding = configuration.GetValue<bool>("Database:EnableSeeding", false);
            
            if (enableSeeding)
            {
                logger.LogInformation("Starting database seeding...");
                
                // Seed base data (users, roles, categories, groups)
                await services.SeedDatabase();
                
                // Seed community content (posts, guides, news, etc.)
                await PostSeeder.SeedAsync(context);
                await GuideSeeder.SeedAsync(context);
                await QuestionSeeder.SeedAsync(context);
                await MapPointSeeder.SeedAsync(context);
                await ChatSeeder.SeedAsync(context);
                await NewsSeeder.SeedAsync(context);
                
                logger.LogInformation("Database seeding completed successfully.");
            }
            else
            {
                logger.LogInformation("Database seeding is disabled in configuration.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
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
    
    // Serve static files from wwwroot directory (includes /uploads, /css, /js, etc.)
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

    // SignalR Hub Endpoints - Each hub has its own endpoint for specific functionality
    // All hubs inherit from BaseHub<THub> for consistent connection management
    // NOTE: SignalR hubs do NOT use culture-prefixed URLs
    
    // Generic Hub - Backward compatibility and general notifications
    app.MapHub<CommunityCar.Infrastructure.Hubs.GenericHub>("/hub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.GenericHub>("/hubs/generic");
    
    // Specific Hubs - Dedicated endpoints for each feature area
    app.MapHub<CommunityCar.Infrastructure.Hubs.QuestionHub>("/hubs/question");
    app.MapHub<CommunityCar.Infrastructure.Hubs.NotificationHub>("/hubs/notification");
    app.MapHub<CommunityCar.Infrastructure.Hubs.ChatHub>("/hubs/chat");
    app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/hubs/friend");
    app.MapHub<CommunityCar.Infrastructure.Hubs.PostHub>("/hubs/post");
    app.MapHub<CommunityCar.Infrastructure.Hubs.CommunityHub>("/hubs/community");
    
    // Legacy endpoints for backward compatibility
    app.MapHub<CommunityCar.Infrastructure.Hubs.QuestionHub>("/questionHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.NotificationHub>("/notificationHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.ChatHub>("/chatHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/friendHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.PostHub>("/postHub");
    app.MapHub<CommunityCar.Infrastructure.Hubs.CommunityHub>("/communityHub");

    app.Run();
}
catch (Exception ex)
{
    // Fatal startup error logging
    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "startup_fatal_error.txt");
    File.WriteAllText(logPath, DateTime.UtcNow + ": " + ex.ToString());
    throw; // Re-throw to let the process exit
}
