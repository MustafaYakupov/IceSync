using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static IceSync.Common.Constants.DataConstants.Workflow;


namespace IceSync.Data.Models;

public class Workflow
{
    public Workflow()
    {
        this.Id = Guid.NewGuid();
    }

    [Key]
    [Comment("Unique Identifier")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ApiWorkflowIdMaxLength)]
    [Comment("Universal Loader Workflow unique identifier (from API)")]
    public string ApiWorkflowId { get; set; } = null!;

    [Required]
    [MaxLength(WorkflowNameMaxLength)]
    [Comment("Workflow name")]
    public string WorkflowName { get; set; } = null!;

    [Comment("Shows whether workflow is active")]
    public bool IsActive { get; set; }

    [MaxLength(MultiExecBehaviorMaxLength)]
    [Comment("Multi exec behavior")]
    public string? MultiExecBehavior { get; set; }

    [Comment("Shows whether Workflow is deleted or not")]
    public bool IsDeleted { get; set; }
}
