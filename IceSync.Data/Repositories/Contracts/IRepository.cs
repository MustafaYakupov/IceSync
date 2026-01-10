using System.Linq.Expressions;

namespace IceSync.Data.Repositories.Contracts;

public interface  IRepository<T> 
    where T : class
{
    IQueryable<T> Query();

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);

    Task AddAsync(T entity,  CancellationToken ct);

    void Update(T entity);

    void Remove(T entity);
}
