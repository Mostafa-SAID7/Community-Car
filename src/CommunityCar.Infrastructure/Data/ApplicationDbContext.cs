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
using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Entities.Communications.notifications;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplySoftDeleteQueryFilter();
    }
}
