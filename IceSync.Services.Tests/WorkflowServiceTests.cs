using IceSync.Data.Repositories.Contracts;
using IceSync.Infrastructure.UniversalLoader.Contracts;
using IceSync.Data.Models;
using IceSync.Web.ViewModels.Workflow;
using Moq;

namespace IceSync.Services.Tests;

public class WorkflowServiceTests
{
    private Mock<IUniversalLoaderClient> api = null!;
    private Mock<IWorkflowRepository> repo = null!;
    private Mock<IUnitOfWork> uow = null!;
    private WorkflowService sut = null!;

    [SetUp]
    public void SetUp()
    {
        api = new Mock<IUniversalLoaderClient>(MockBehavior.Strict);
        repo = new Mock<IWorkflowRepository>(MockBehavior.Strict);
        uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

        sut = new WorkflowService(api.Object, repo.Object, uow.Object);
    }

    [Test]
    public async Task RunWorkflowAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        int workflowId = 123;
        this.api.Setup(x => x.RunWorkflowAsync(workflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this.sut.RunWorkflowAsync(workflowId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        this.api.VerifyAll();
    }

    [Test]
    public async Task SyncWorkflowsToDatabaseAsync_InsertsNewWorkflow_WhenNotInDb()
    {
        // Arrange: API returns 1 workflow, DB is empty
        var apiWorkflows = new List<WorkflowDto>
        {
            new() { Id = 1, Name = "WF1", IsActive = true, MultiExecBehavior = "2" }
        };

        this.api.Setup(x => x.GetWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiWorkflows);

        this.repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new List<Workflow>()));

        this.repo.Setup(x => x.AddAsync(It.Is<Workflow>(w =>
                w.ApiWorkflowId == "1" &&
                w.WorkflowName == "WF1" &&
                w.IsActive == true &&
                w.MultiExecBehavior == "2" &&
                w.IsDeleted == false
            ),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var changes = await this.sut.SyncWorkflowsToDatabaseAsync(CancellationToken.None);

        // Assert
        Assert.That(changes, Is.EqualTo(1));

        this.api.VerifyAll();
        this.repo.VerifyAll();
        this.uow.VerifyAll();
    }

    [Test]
    public async Task SyncWorkflowsToDatabaseAsync_UpdatesExistingWorkflow_WhenFoundInDb()
    {
        // Arrange: API returns workflow 1 with new values, DB contains workflow 1 old values
        var db = new List<Workflow>
        {
            new()
            {
                ApiWorkflowId = "1",
                WorkflowName = "OLD",
                IsActive = false,
                MultiExecBehavior = "0",
                IsDeleted = false
            }
        };

        var api = new List<WorkflowDto>
        {
            new() { Id = 1, Name = "NEW", IsActive = true, MultiExecBehavior = "3" }
        };

        this.api.Setup(x => x.GetWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(api);

        this.repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(db);

        // We expect Update to be called with the SAME entity instance, mutated
        this.repo.Setup(x => x.Update(It.Is<Workflow>(w =>
            w.ApiWorkflowId == "1" &&
            w.WorkflowName == "NEW" &&
            w.IsActive == true &&
            w.MultiExecBehavior == "3" &&
            w.IsDeleted == false
        )));

        this.uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var changes = await this.sut.SyncWorkflowsToDatabaseAsync(CancellationToken.None);

        // Assert
        Assert.That(changes, Is.EqualTo(1));

        this.api.VerifyAll();
        this.repo.VerifyAll();
        this.uow.VerifyAll();
    }

    [Test]
    public async Task SyncWorkflowsToDatabaseAsync_SoftDeletesWorkflow_WhenMissingFromApi()
    {
        // Arrange: DB has workflow 99, API returns none
        var db = new List<Workflow>
        {
            new()
            {
                ApiWorkflowId = "99",
                WorkflowName = "TO DELETE",
                IsActive = true,
                MultiExecBehavior = "1",
                IsDeleted = false
            }
        };

        this.api.Setup(x => x.GetWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkflowDto>());

        this.repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(db);

        // When your service soft deletes, it typically sets IsDeleted=true and calls Update
        this.repo.Setup(x => x.Update(It.Is<Workflow>(w =>
            w.ApiWorkflowId == "99" &&
            w.IsDeleted == true
        )));

        this.uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var changes = await this.sut.SyncWorkflowsToDatabaseAsync(CancellationToken.None);

        // Assert
        Assert.That(changes, Is.EqualTo(1));

        this.api.VerifyAll();
        this.repo.VerifyAll();
        this.uow.VerifyAll();
    }
}
