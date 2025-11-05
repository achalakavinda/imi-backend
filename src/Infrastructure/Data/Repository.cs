using MigratingAssistant.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Infrastructure.Data;

/// <summary>
/// Generic repository implementation for entity operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbSet<T> _dbSet;
    protected readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context, DbSet<T> dbSet)
    {
        _dbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all entities as an IQueryable for further query composition
    /// </summary>
    public IQueryable<T> GetAll() => _dbSet;

    /// <summary>
    /// Gets an entity by its primary key
    /// </summary>
    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by composite primary key
    /// </summary>
    public async Task<T?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(keyValues, cancellationToken);
    }

    /// <summary>
    /// Inserts a new entity into the database
    /// Note: Audit fields (Created/Modified) are automatically set by AuditableEntityInterceptor
    /// </summary>
    public T Insert(T entity)
    {
        return _dbSet.Add(entity).Entity;
    }

    /// <summary>
    /// Updates an existing entity in the database
    /// Note: Modified audit fields are automatically updated by AuditableEntityInterceptor
    /// </summary>
    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// Deletes an entity from the database
    /// </summary>
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes an entity by its primary key
    /// </summary>
    public async Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }
}
