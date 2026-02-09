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
    try
    {
        var context = services.GetRequiredService<CommunityCar.Infrastructure.Data.ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await services.SeedDatabase(); // Call the extension method on IServiceProvider
        await FriendshipSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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

app.MapStaticAssets();

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
    pattern: "{controller=Feed}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<CommunityCar.Mvc.Hubs.QuestionHub>("/questionHub");
app.MapHub<CommunityCar.Infrastructure.Hubs.NotificationHub>("/notificationHub");
app.MapHub<CommunityCar.Infrastructure.Hubs.ChatHub>("/chatHub");
app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/friendHub");

app.Run();
