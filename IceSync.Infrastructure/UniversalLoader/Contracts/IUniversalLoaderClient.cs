using IceSync.Web.ViewModels.Workflow;

namespace IceSync.Infrastructure.UniversalLoader.Contracts;

public interface IUniversalLoaderClient
{
    Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct);
    Task<bool> RunWorkflowAsync(string workflowId, CancellationToken ct);
}
