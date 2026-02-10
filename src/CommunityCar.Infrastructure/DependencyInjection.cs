using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using CommunityCar.Infrastructure.Data;
using CommunityCar.Infrastructure.Repos.Common;
using CommunityCar.Infrastructure.Repos.Identity;
using CommunityCar.Infrastructure.Uow.Common;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Identity.Roles;
using CommunityCar.Domain.Interfaces.Identity;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Domain.Interfaces.Services;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Domain.Commands.Community;
using CommunityCar.Infrastructure.Services.Identity;
using CommunityCar.Infrastructure.Services.Community;
using CommunityCar.Infrastructure.Services.Communications;
using CommunityCar.Infrastructure.Services.Dashboard;
using CommunityCar.Infrastructure.Services.Common;
using CommunityCar.Infrastructure.Services;
using CommunityCar.Infrastructure.Services.ML;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Handlers.Community;

namespace CommunityCar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CommunityCar.Infrastructure.Data.Interceptors.AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
               .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            
            // 2FA - Two-Factor Authentication
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        // Configure cookie authentication paths
        services.PostConfigure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });

        // Configure external cookie to be permissive for localhost
        services.PostConfigure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("SuperAdmin", "Admin"));
            options.AddPolicy("Moderator", policy => policy.RequireRole("SuperAdmin", "Admin", "Moderator"));
        });

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["SocialAuth:Google:ClientId"] ?? "google-client-id";
                options.ClientSecret = configuration["SocialAuth:Google:ClientSecret"] ?? "google-client-secret";
                
                // Fix for "Correlation failed" on localhost
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

                options.Events.OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/Login?error=" + System.Net.WebUtility.UrlEncode(context.Failure?.Message ?? "Login failed"));
                    context.HandleResponse();
                    return System.Threading.Tasks.Task.CompletedTask;
                };
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["SocialAuth:Facebook:AppId"] ?? "facebook-app-id";
                options.AppSecret = configuration["SocialAuth:Facebook:AppSecret"] ?? "facebook-app-secret";
                
                // Fix for "Correlation failed" on localhost
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

                options.Events.OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/Login?error=" + System.Net.WebUtility.UrlEncode(context.Failure?.Message ?? "Login failed"));
                    context.HandleResponse();
                    return System.Threading.Tasks.Task.CompletedTask;
                };
            });


        // Generic Repository & UoW
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // SignalR Connection Manager
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        // SignalR Hub Services (Clean Architecture with IHubContext)
        services.AddScoped<INotificationHubService, NotificationHubService>();
        services.AddScoped<IQuestionHubService, QuestionHubService>();
        services.AddScoped<IPostHubService, PostHubService>();
        services.AddScoped<IFriendHubService, FriendHubService>();
        services.AddScoped<IChatHubService, ChatHubService>();
        
        // Command Handlers
        services.AddScoped<ICommandHandler<LikePostCommand, LikePostResult>, LikePostCommandHandler>();
        services.AddScoped<ICommandHandler<VoteQuestionCommand, VoteResult>, VoteQuestionCommandHandler>();
        services.AddScoped<ICommandHandler<VoteAnswerCommand, VoteResult>, VoteAnswerCommandHandler>();
        
        // Domain Event Handlers - Vote
        services.AddScoped<CommunityCar.Infrastructure.EventHandlers.VoteAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.VoteCreatedEvent>, CommunityCar.Infrastructure.EventHandlers.VoteAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.VoteChangedEvent>, CommunityCar.Infrastructure.EventHandlers.VoteAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.VoteRemovedEvent>, CommunityCar.Infrastructure.EventHandlers.VoteAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.VoteResurrectedEvent>, CommunityCar.Infrastructure.EventHandlers.VoteAuditEventHandler>();
        
        // Domain Event Handlers - Like
        services.AddScoped<CommunityCar.Infrastructure.EventHandlers.LikeAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.LikeCreatedEvent>, CommunityCar.Infrastructure.EventHandlers.LikeAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.LikeRemovedEvent>, CommunityCar.Infrastructure.EventHandlers.LikeAuditEventHandler>();
        
        // Domain Event Handlers - Comment
        services.AddScoped<CommunityCar.Infrastructure.EventHandlers.CommentAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.CommentCreatedEvent>, CommunityCar.Infrastructure.EventHandlers.CommentAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.CommentUpdatedEvent>, CommunityCar.Infrastructure.EventHandlers.CommentAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.CommentDeletedEvent>, CommunityCar.Infrastructure.EventHandlers.CommentAuditEventHandler>();
        
        // Domain Event Handlers - Bookmark
        services.AddScoped<CommunityCar.Infrastructure.EventHandlers.BookmarkAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.BookmarkCreatedEvent>, CommunityCar.Infrastructure.EventHandlers.BookmarkAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.BookmarkRemovedEvent>, CommunityCar.Infrastructure.EventHandlers.BookmarkAuditEventHandler>();
        
        // Domain Event Handlers - Rating
        services.AddScoped<CommunityCar.Infrastructure.EventHandlers.RatingAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.RatingCreatedEvent>, CommunityCar.Infrastructure.EventHandlers.RatingAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.RatingUpdatedEvent>, CommunityCar.Infrastructure.EventHandlers.RatingAuditEventHandler>();
        services.AddScoped<CommunityCar.Domain.Interfaces.IDomainEventHandler<CommunityCar.Domain.Events.Community.RatingRemovedEvent>, CommunityCar.Infrastructure.EventHandlers.RatingAuditEventHandler>();
        
        // Identity Services & Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Security & Auditing
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ISecurityService, SecurityService>();

        // Existing Services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IKPIService, KPIService>();
        services.AddScoped<ISecurityAlertService, SecurityAlertService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<ISystemService, SystemService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddScoped<IContentActivityService, ContentActivityService>();
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IHubNotificationService, HubNotificationService>();
        services.AddScoped<IWidgetService, WidgetService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IGuideService, GuideService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IAssistantService, AssistantService>();
        services.AddScoped<IMLPipelineService, MLPipelineService>();
        services.AddScoped<CommunityCar.Infrastructure.Interfaces.IFileStorageService, FileStorageService>();
        services.AddSingleton<IPredictionService, PredictionService>();
        services.AddSingleton<ISentimentAnalysisService, SentimentAnalysisService>();
        services.AddScoped<IGroupService, GroupService>();

        // MediatR for domain events
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
