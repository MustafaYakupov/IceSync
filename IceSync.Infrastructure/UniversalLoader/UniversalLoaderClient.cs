using IceSync.Infrastructure.UniversalLoader.Contracts;
using IceSync.Web.ViewModels.Workflow;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

        this.http.DefaultRequestHeaders.Accept.Clear();
        this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
    }

    public async Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct)
    {
        await Authorize(ct);

        using var response = await this.http.GetAsync("/workflows", ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<List<WorkflowDto>>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
    }

    public async Task<bool> RunWorkflowAsync(int workflowId, CancellationToken ct)
    {
        await Authorize(ct);

        using var response = await this.http.PostAsync($"/workflows/{workflowId}/run", null, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task Authorize(CancellationToken ct)
    {
        var token = await this.tokens.GetTokenAsync(ct);
        this.http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
