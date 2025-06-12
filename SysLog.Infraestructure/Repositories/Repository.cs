using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;

namespace SysLog.Repository.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _dbContext;
    private DbSet<T> _dbSet;

    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Queryable.OrderByDescending<T, DateTime>(_dbSet, e => EF.Property<DateTime>(e, "DateTime")).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public Task Update(T obj)
    {
        _dbSet.Update(obj);
        return Task.CompletedTask;
    }
    public Task Remove(T obj)
    {
        _dbContext.Remove(obj);
        return Task.CompletedTask;
    }
    
    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}