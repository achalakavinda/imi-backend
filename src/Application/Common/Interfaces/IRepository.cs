namespace MigratingAssistant.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for entity operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities as an IQueryable for further query composition
    /// </summary>
    IQueryable<T> GetAll();

    /// <summary>
    /// Gets an entity by its primary key
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by composite primary key
    /// </summary>
    /// <param name="keyValues">Primary key values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity into the database
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <returns>The inserted entity</returns>
    T Insert(T entity);

    /// <summary>
    /// Updates an existing entity in the database
    /// </summary>
    /// <param name="entity">Entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Deletes an entity from the database
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    void Delete(T entity);

    /// <summary>
    /// Deletes an entity by its primary key
    /// </summary>
    /// <param name="id">Primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default);
}
