using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityCar.Domain.Interfaces.Common;

/// <summary>
/// Unit of Work pattern for managing transactions and repository access
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Get repository for specific entity type
    /// </summary>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, Base.Interfaces.IEntity;
    
    /// <summary>
    /// Save all changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begin a database transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
    
    /// <summary>
    /// Clear the change tracker
    /// </summary>
    void ClearTracker();
}
