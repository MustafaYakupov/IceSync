using IceSync.Data.Models;
using IceSync.Data.Repositories.Contracts;
using IceSync.Infrastructure.UniversalLoader.Contracts;
using IceSync.Web.ViewModels.Workflow;
using IceSync.Services.Contracts;

namespace IceSync.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IUniversalLoaderClient api;
    private readonly IWorkflowRepository repo;
    private readonly IUnitOfWork uow;

    public WorkflowService(IUniversalLoaderClient api, IWorkflowRepository repo, IUnitOfWork uow)
    {
        this.api = api;
        this.repo = repo;
        this.uow = uow;
    }

    public async Task<IReadOnlyList<WorkflowDto>> GetWorkflowsFromApiAsync(CancellationToken ct)
        => await this.api.GetWorkflowsAsync(ct);
    public async Task<bool> RunWorkflowAsync(int workflowId, CancellationToken ct)
        => await this.api.RunWorkflowAsync(workflowId, ct); 

    public async Task<int> SyncWorkflowsToDatabaseAsync(CancellationToken ct)
    {
        var apiWorkflows = await this.api.GetWorkflowsAsync(ct);
        var apiIds = apiWorkflows.Select(x => x.Id.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var dbWorkflows = await this.repo.GetAllAsync(ct);

        // Soft-delete missing
        foreach (var db in dbWorkflows.Where(x => !apiIds.Contains(x.ApiWorkflowId)))
        {
            db.IsDeleted = true;
            this.repo.Update(db);
        }

        foreach (var dto in apiWorkflows)
        {
            var apiId = dto.Id.ToString();

            var existing = dbWorkflows.FirstOrDefault(x =>
                x.ApiWorkflowId == apiId);

            if (existing is null)
            {
                await this.repo.AddAsync(new Workflow
                {
                    ApiWorkflowId = apiId,
                    WorkflowName = dto.Name,
                    IsActive = dto.IsActive,
                    MultiExecBehavior = dto.MultiExecBehavior.ToString(),
                    IsDeleted = false
                }, ct);
            }
            else
            {
                existing.WorkflowName = dto.Name;
                existing.IsActive = dto.IsActive;
                existing.MultiExecBehavior = dto.MultiExecBehavior.ToString();
                existing.IsDeleted = false;
                this.repo.Update(existing);
            }
        }

        return await this.uow.SaveChangesAsync(ct);
    }
}
