namespace IceSync.Data.Repositories.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
