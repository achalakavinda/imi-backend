using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Common.Interfaces;

/// <summary>
/// Represents the unit of work pattern for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the application database context
    /// </summary>
    IApplicationDbContext Context { get; }

    // Repository properties for all entities
    IRepository<User> Users { get; }
    IRepository<UserProfile> UserProfiles { get; }
    IRepository<ServiceProvider> ServiceProviders { get; }
    IRepository<Document> Documents { get; }
    IRepository<SupportTicket> SupportTickets { get; }
    IRepository<Booking> Bookings { get; }
    IRepository<Payment> Payments { get; }
    IRepository<JobApplication> JobApplications { get; }
    IRepository<ServiceType> ServiceTypes { get; }
    IRepository<Listing> Listings { get; }
    IRepository<InventoryItem> InventoryItems { get; }
    IRepository<Job> Jobs { get; }
    IRepository<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
