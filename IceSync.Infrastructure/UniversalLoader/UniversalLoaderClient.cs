using IceSync.Infrastructure.UniversalLoader.Contracts;
using IceSync.Infrastructure.UniversalLoader.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace IceSync.Infrastructure.UniversalLoader;

public class UniversalLoaderClient : IUniversalLoaderClient
{
    private readonly HttpClient http;
    private readonly IUniversalLoaderTokenProvider tokens;

    public UniversalLoaderClient(HttpClient http, IUniversalLoaderTokenProvider tokens)
    {
        this.http = http;
        this.tokens = tokens;
    }

    public async Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct)
    {
        await Authorize(ct);

        using var response = await this.http.GetAsync("/workflows", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<WorkflowDto>>(cancellationToken: ct)
               ?? [];
    }

    public async Task<bool> RunWorkflowAsync(string workflowId, CancellationToken ct)
    {
        await Authorize(ct);

        using var resp = await this.http.PostAsync($"/workflows/{Uri.EscapeDataString(workflowId)}/run", null, ct);
        return resp.IsSuccessStatusCode;
    }

    private async Task Authorize(CancellationToken ct)
    {
        var token = await this.tokens.GetTokenAsync(ct);
        this.http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
