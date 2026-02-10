using CommunityCar.Infrastructure.Data;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Repos.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityCar.Infrastructure.Uow.Common;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    /// <summary>
    /// Get or create repository for specific entity type
    /// </summary>
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class, Domain.Base.Interfaces.IEntity
    {
        var type = typeof(TEntity);
        
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = new Repository<TEntity>(_context);
            _repositories.Add(type, repositoryInstance);
        }
        
        return (IRepository<TEntity>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void ClearTracker()
    {
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
