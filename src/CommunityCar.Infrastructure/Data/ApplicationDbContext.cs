using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Identity.Roles;
using System.Reflection;
using System;
using System.Threading;
using System.Threading.Tasks;

using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Entities.Dashboard.KPIs;
using CommunityCar.Domain.Entities.Dashboard.health;
using CommunityCar.Domain.Entities.Dashboard.settings;
using CommunityCar.Domain.Entities.Dashboard.Localization;
using CommunityCar.Domain.Entities.Dashboard.widgets;
using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Entities.Communications.notifications;
using CommunityCar.Domain.Entities.Communications.chats;
using CommunityCar.Domain.Entities.Community.maps;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Infrastructure.Data.Extensions;

namespace CommunityCar.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Features
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<KPI> KPIs => Set<KPI>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();
    public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();
    public DbSet<SystemMetric> SystemMetrics => Set<SystemMetric>();
    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<LocalizationResource> LocalizationResources => Set<LocalizationResource>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<CommunityCar.Domain.Entities.Community.Common.Category> Categories => Set<CommunityCar.Domain.Entities.Community.Common.Category>();
    
    // Groups
    public DbSet<CommunityGroup> CommunityGroups => Set<CommunityGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    
    // Chat
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
    
    // Maps
    public DbSet<MapPoint> MapPoints => Set<MapPoint>();
    public DbSet<MapPointRating> MapPointRatings => Set<MapPointRating>();
    public DbSet<MapPointComment> MapPointComments => Set<MapPointComment>();
    public DbSet<MapPointFavorite> MapPointFavorites => Set<MapPointFavorite>();
    public DbSet<MapPointCheckIn> MapPointCheckIns => Set<MapPointCheckIn>();
    public DbSet<MapPointReview> MapPointReviews => Set<MapPointReview>();
    
    // Q&A
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<QuestionVote> QuestionVotes => Set<QuestionVote>();
    public DbSet<AnswerVote> AnswerVotes => Set<AnswerVote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplySoftDeleteQueryFilter();
    }
}
