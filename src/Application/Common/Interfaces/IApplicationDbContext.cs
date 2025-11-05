using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<ServiceProvider> ServiceProviders { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Document> Documents { get; }
    DbSet<SupportTicket> SupportTickets { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Payment> Payments { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<ServiceType> ServiceTypes { get; }
    DbSet<Listing> Listings { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Job> Jobs { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
