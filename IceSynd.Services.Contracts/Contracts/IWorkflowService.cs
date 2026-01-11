using IceSync.Web.ViewModels.Workflow;

namespace IceSync.Services.Contracts;

public interface IWorkflowService
{
    Task<IReadOnlyList<WorkflowDto>> GetWorkflowsFromApiAsync(CancellationToken ct);
    Task<bool> RunWorkflowAsync(string workflowId, CancellationToken ct);
    Task<int> SyncWorkflowsToDatabaseAsync(CancellationToken ct);
}
