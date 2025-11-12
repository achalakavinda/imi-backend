using System.Reflection;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Infrastructure.Data.Migrations.MySQL;
using MigratingAssistant.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public new DbSet<User> Users => Set<User>();
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply MySQL-specific configurations
        if (Database.ProviderName == "Pomelo.EntityFrameworkCore.MySql")
        {
            builder.ConfigureForMySql();
        }

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
