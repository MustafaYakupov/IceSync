using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IceSync.Services.Contracts;

namespace IceSync.Infrastructure.Background;

public class WorkflowSyncHostedService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<WorkflowSyncHostedService> logger;

    public WorkflowSyncHostedService(IServiceProvider serviceProvider, ILogger<WorkflowSyncHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Sync(stoppingToken);

        var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await Sync(stoppingToken);
        }
    }

    private async Task Sync(CancellationToken ct)
    {
        try
        {
            using var scope = this.serviceProvider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
            var changes = await svc.SyncWorkflowsToDatabaseAsync(ct);

            this.logger.LogInformation("Sync completed. Changes: {Changes}", changes);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Sync failed.");
        }
    }
}
