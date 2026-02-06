using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CommunityCar.Infrastructure.Repos.Common;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => 
        await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate) => 
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) => 
        predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();
}
