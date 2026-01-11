using System.Text.Json.Serialization;

namespace IceSync.Web.ViewModels.Workflow;

public class WorkflowDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("multiExecBehavior")]
    public string? MultiExecBehavior { get; set; }
}
