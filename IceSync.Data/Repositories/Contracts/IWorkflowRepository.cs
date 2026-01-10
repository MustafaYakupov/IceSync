using IceSync.Data.Models;

namespace IceSync.Data.Repositories.Contracts;

public interface IWorkflowRepository : IRepository<Workflow>
{
    Task<List<Workflow>> GetAllAsync(CancellationToken ct);

    Task<Workflow?> GetByApiIdAsync(string apiWorkflowId, CancellationToken ct);

    void RemoveRange(IEnumerable<Workflow> workflows);
}
