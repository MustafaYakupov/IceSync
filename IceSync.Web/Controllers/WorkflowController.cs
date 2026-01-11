using IceSync.Services.Contracts;
using IceSync.Web.ViewModels.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace IceSync.Web.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IWorkflowService service;

        public WorkflowController(IWorkflowService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var api = await this.service.GetWorkflowsFromApiAsync(ct);

            var model = api.Select(x => new WorkflowRowViewModel
            {
                WorkflowId = x.Id,
                WorkflowName = x.Name,
                IsActive = x.IsActive,
                MultiExecBehavior = x.MultiExecBehavior
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(string workflowId, CancellationToken ct)
        {
            var ok = await this.service.RunWorkflowAsync(workflowId, ct);

            TempData["StatusOk"] = ok;
            TempData["StatusMessage"] = ok
                ? $"Workflow '{workflowId}' started successfully."
                : $"Failed to start workflow '{workflowId}'.";

            return RedirectToAction(nameof(Index));
        }
    }
}
