using IceSync.Data.Repositories.Contracts;

namespace IceSync.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IceSyncDbContext context;

    public UnitOfWork(IceSyncDbContext context)
    {
        this.context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
        => await this.context.SaveChangesAsync(ct);
}
