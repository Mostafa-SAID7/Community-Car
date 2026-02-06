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
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Services.Identity;
using CommunityCar.Infrastructure.Services.Community;
using CommunityCar.Infrastructure.Services.Communications;
using CommunityCar.Infrastructure.Services.Dashboard;
using CommunityCar.Infrastructure.Services.Common;

namespace CommunityCar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CommunityCar.Infrastructure.Data.Interceptors.AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<CommunityCar.Infrastructure.Data.Interceptors.AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
               .AddInterceptors(auditableInterceptor);
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
        // Configure cookie authentication paths
        services.PostConfigure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Moderator", policy => policy.RequireRole("Admin", "Moderator"));
        });

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["SocialAuth:Google:ClientId"] ?? "google-client-id";
                options.ClientSecret = configuration["SocialAuth:Google:ClientSecret"] ?? "google-client-secret";
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["SocialAuth:Facebook:AppId"] ?? "facebook-app-id";
                options.AppSecret = configuration["SocialAuth:Facebook:AppSecret"] ?? "facebook-app-secret";
            });


        // Generic Repository & UoW
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
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
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
