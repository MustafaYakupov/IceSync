using IceSync.Data.Models;
using IceSync.Data.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Data.Repositories;

public class WorkflowRepository : Repository<Workflow>, IWorkflowRepository
{
    private readonly IceSyncDbContext context;

    public WorkflowRepository(IceSyncDbContext context) 
        : base(context)
    {
        this.context = context;
    }

    public async Task<List<Workflow>> GetAllAsync(CancellationToken ct)
        => await this.context.Workflows.ToListAsync();

    public async Task<Workflow?> GetByApiIdAsync(string apiWorkflowId, CancellationToken ct)
        => await this.context.Workflows
            .FirstOrDefaultAsync(w => w.ApiWorkflowId == apiWorkflowId, ct);

    public void RemoveRange(IEnumerable<Workflow> workflows)
        => this.context.Workflows.RemoveRange(workflows);
}
