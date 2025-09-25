using System.Linq.Expressions;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly FlowerShopDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(FlowerShopDbContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null
    ) => filter == null ? await _set.ToListAsync() : await _set.Where(filter).ToListAsync();

    public virtual async Task AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _db.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public IQueryable<T> Query() => _set.AsQueryable();
}
