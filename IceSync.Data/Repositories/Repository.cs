using IceSync.Data.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IceSync.Data.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly IceSyncDbContext context;
    private readonly DbSet<T> dbSet;

    public Repository(IceSyncDbContext context)
    {
        this.context = context;
        this.dbSet = context.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken ct)
        => await this.dbSet.AddAsync(entity, ct).AsTask();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct) 
        => await this.dbSet.FirstOrDefaultAsync(predicate, ct);

    public IQueryable<T> Query()
        => this.dbSet.AsQueryable();

    public void Remove(T entity)
        => this.dbSet.Remove(entity);

    public void Update(T entity)
        => this.dbSet.Update(entity);
}
