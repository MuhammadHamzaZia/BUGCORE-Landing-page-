using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.Filters;
using BugCore.Web.ViewModels.Projects;

namespace BugCore.Web.Controllers;

[Authorize]
[BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
public class VersionController : Controller
{
    private readonly IVersionService _versionService;
    private readonly IProjectService _projectService;

    public VersionController(IVersionService versionService, IProjectService projectService)
    {
        _versionService = versionService;
        _projectService = projectService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int projectId, string version, string? description, DateTime? dateOrder, bool released = false, bool addAndEdit = false)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            TempData["Error"] = "Version label is required.";
            return Redirect($"/Project/Edit/{projectId}#versions");
        }

        var newVersion = new ProjectVersion
        {
            ProjectId = projectId,
            Version = version.Trim(),
            Description = description ?? string.Empty,
            DateOrder = dateOrder,
            Released = released,
            Obsolete = false
        };

        var created = await _versionService.CreateVersionAsync(newVersion);

        if (addAndEdit)
        {
            return RedirectToAction("Edit", new { id = created.Id });
        }

        TempData["Success"] = $"Version '{created.Version}' added.";
        return Redirect($"/Project/Edit/{projectId}#versions");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var version = await _versionService.GetVersionByIdAsync(id);
        if (version == null)
        {
            return NotFound();
        }

        var project = await _projectService.GetProjectByIdAsync(version.ProjectId);

        var viewModel = new VersionEditViewModel
        {
            Id = version.Id,
            ProjectId = version.ProjectId,
            Project = project,
            Version = version.Version,
            Description = version.Description,
            DateOrder = version.DateOrder,
            Released = version.Released,
            Obsolete = version.Obsolete
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VersionEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Version))
        {
            ModelState.AddModelError(nameof(model.Version), "Version name is required.");
            return View(model);
        }

        var version = await _versionService.GetVersionByIdAsync(model.Id);
        if (version == null)
        {
            return NotFound();
        }

        version.Version = model.Version.Trim();
        version.Description = model.Description ?? string.Empty;
        version.DateOrder = model.DateOrder;
        version.Released = model.Released;
        version.Obsolete = model.Obsolete;

        await _versionService.UpdateVersionAsync(version);
        TempData["Success"] = "Version updated successfully.";
        return Redirect($"/Project/Edit/{version.ProjectId}#versions");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        await _versionService.DeleteVersionAsync(id);
        TempData["Success"] = "Version deleted.";
        return Redirect($"/Project/Edit/{projectId}#versions");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(int projectId, int otherProjectId, bool copyFrom, bool copyTo)
    {
        int source = copyFrom ? otherProjectId : projectId;
        int target = copyFrom ? projectId : otherProjectId;

        if (source == 0 || target == 0)
        {
            TempData["Error"] = "Invalid project selection for version copy.";
            return Redirect($"/Project/Edit/{projectId}#versions");
        }

        await _versionService.CopyVersionsAsync(source, target);
        TempData["Success"] = "Versions copied successfully.";
        return Redirect($"/Project/Edit/{projectId}#versions");
    }
}
