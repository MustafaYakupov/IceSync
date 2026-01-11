using IceSync.Web.ViewModels.Models;

namespace IceSync.Infrastructure.UniversalLoader.Contracts;

public interface IUniversalLoaderClient
{
    Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct);
    Task<bool> RunWorkflowAsync(string workflowId, CancellationToken ct);
}
