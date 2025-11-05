using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace MigratingAssistant.Infrastructure.Data;

/// <summary>
/// Implementation of the Unit of Work pattern with repository pattern support
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    // Lazy-loaded repositories
    private IRepository<User>? _users;
    private IRepository<UserProfile>? _userProfiles;
    private IRepository<ServiceProvider>? _serviceProviders;
    private IRepository<Document>? _documents;
    private IRepository<SupportTicket>? _supportTickets;
    private IRepository<Booking>? _bookings;
    private IRepository<Payment>? _payments;
    private IRepository<JobApplication>? _jobApplications;
    private IRepository<ServiceType>? _serviceTypes;
    private IRepository<Listing>? _listings;
    private IRepository<InventoryItem>? _inventoryItems;
    private IRepository<Job>? _jobs;
    private IRepository<AuditLog>? _auditLogs;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the application database context
    /// </summary>
    public IApplicationDbContext Context => _context;

    // Repository properties - lazy loaded on first access
    public IRepository<User> Users => _users ??= new Repository<User>(_context, _context.Users);
    public IRepository<UserProfile> UserProfiles => _userProfiles ??= new Repository<UserProfile>(_context, _context.UserProfiles);
    public IRepository<ServiceProvider> ServiceProviders => _serviceProviders ??= new Repository<ServiceProvider>(_context, _context.ServiceProviders);
    public IRepository<Document> Documents => _documents ??= new Repository<Document>(_context, _context.Documents);
    public IRepository<SupportTicket> SupportTickets => _supportTickets ??= new Repository<SupportTicket>(_context, _context.SupportTickets);
    public IRepository<Booking> Bookings => _bookings ??= new Repository<Booking>(_context, _context.Bookings);
    public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context, _context.Payments);
    public IRepository<JobApplication> JobApplications => _jobApplications ??= new Repository<JobApplication>(_context, _context.JobApplications);
    public IRepository<ServiceType> ServiceTypes => _serviceTypes ??= new Repository<ServiceType>(_context, _context.ServiceTypes);
    public IRepository<Listing> Listings => _listings ??= new Repository<Listing>(_context, _context.Listings);
    public IRepository<InventoryItem> InventoryItems => _inventoryItems ??= new Repository<InventoryItem>(_context, _context.InventoryItems);
    public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(_context, _context.Jobs);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context, _context.AuditLogs);

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Disposes the unit of work and releases resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }
    }
}
