using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.Filters;
using BugCore.Web.ViewModels.Projects;

namespace BugCore.Web.Controllers;

[Authorize]
public class ProjectController : Controller
{
    private readonly IProjectService _projectService;
    private readonly ICategoryService _categoryService;
    private readonly IVersionService _versionService;
    private readonly ICustomFieldService _customFieldService;
    private readonly IUserContextService _userContext;
    private readonly IAccessControlService _accessControl;

    public ProjectController(
        IProjectService projectService,
        ICategoryService categoryService,
        IVersionService versionService,
        ICustomFieldService customFieldService,
        IUserContextService userContext,
        IAccessControlService accessControl)
    {
        _projectService = projectService;
        _categoryService = categoryService;
        _versionService = versionService;
        _customFieldService = customFieldService;
        _userContext = userContext;
        _accessControl = accessControl;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userContext.GetCurrentUserId() ?? 0;
        var hierarchicalProjects = await _projectService.GetHierarchicalProjectsAsync(includeDisabled: true);
        var globalCategories = await _categoryService.GetGlobalCategoriesAsync();
        var unassignedUsers = await _projectService.GetUnassignedUsersAsync(0);

        var isManagerOrAdmin = await _accessControl.HasAccessLevelAsync(userId, AccessLevel.Manager);
        var isAdmin = await _accessControl.HasAccessLevelAsync(userId, AccessLevel.Administrator);

        var viewModel = new ProjectListViewModel
        {
            HierarchicalProjects = hierarchicalProjects,
            GlobalCategories = globalCategories,
            AllUsers = unassignedUsers,
            CanCreateProject = isManagerOrAdmin,
            CanManageSite = isAdmin
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelectProject(int? projectId, string? returnUrl = null)
    {
        _userContext.SetActiveProjectId(projectId);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> Create(int? parentProjectId = null)
    {
        var model = new ProjectCreateViewModel
        {
            Enabled = true,
            InheritCategories = true,
            ParentProjectId = parentProjectId
        };

        if (parentProjectId.HasValue && parentProjectId.Value > 0)
        {
            model.ParentProject = await _projectService.GetProjectByIdAsync(parentProjectId.Value);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Project name is required.");
        }
        else
        {
            var existingProjects = await _projectService.GetAllProjectsAsync(includeDisabled: true);
            if (existingProjects.Any(p => p.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(model.Name), $"A project named '{model.Name.Trim()}' already exists.");
            }
        }

        if (!ModelState.IsValid)
        {
            if (model.ParentProjectId.HasValue && model.ParentProjectId.Value > 0)
            {
                model.ParentProject = await _projectService.GetProjectByIdAsync(model.ParentProjectId.Value);
            }
            return View(model);
        }

        try
        {
            var project = new Project
            {
                Name = model.Name.Trim(),
                Status = model.Status,
                ViewState = model.ViewState,
                Enabled = model.Enabled,
                InheritCategories = model.InheritCategories,
                Description = model.Description ?? string.Empty
            };

            var created = await _projectService.CreateProjectAsync(project, model.ParentProjectId, model.InheritCategories);
            TempData["Success"] = $"Project '{created.Name}' created successfully.";
            return RedirectToAction("Edit", new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Failed to create project: {ex.Message}");
            if (model.ParentProjectId.HasValue && model.ParentProjectId.Value > 0)
            {
                model.ParentProject = await _projectService.GetProjectByIdAsync(model.ParentProjectId.Value);
            }
            return View(model);
        }
    }

    [HttpGet]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> Edit(int id, bool showGlobalUsers = false)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var subprojects = await _projectService.GetSubprojectHierarchiesAsync(id);
        var eligibleSubprojects = await _projectService.GetEligibleSubprojectsAsync(id);
        var categories = await _categoryService.GetCategoriesForProjectAsync(id);
        var versions = await _versionService.GetVersionsForProjectAsync(id, includeObsolete: true);
        var linkedCustomFields = await _customFieldService.GetLinkedFieldsForProjectAsync(id);
        var availableCustomFields = await _customFieldService.GetUnlinkedFieldsForProjectAsync(id);
        var assignedUsers = await _projectService.GetProjectUsersAsync(id);
        var unassignedUsers = await _projectService.GetUnassignedUsersAsync(id);
        var allOtherProjects = (await _projectService.GetAllProjectsAsync(includeDisabled: true))
            .Where(p => p.Id != id)
            .OrderBy(p => p.Name)
            .ToList();

        var globalUsers = showGlobalUsers ? (await _projectService.GetUnassignedUsersAsync(0)).ToList() : new List<ApplicationUser>();

        var viewModel = new ProjectDetailsViewModel
        {
            Project = project,
            Subprojects = subprojects,
            EligibleSubprojects = eligibleSubprojects,
            Categories = categories,
            Versions = versions,
            LinkedCustomFields = linkedCustomFields,
            AvailableCustomFields = availableCustomFields,
            AssignedUsers = assignedUsers,
            UnassignedUsers = unassignedUsers,
            GlobalUsers = globalUsers,
            ShowGlobalUsers = showGlobalUsers,
            OtherProjects = allOtherProjects,
            CanManage = true
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> Edit(Project project)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            TempData["Error"] = "Project name cannot be empty.";
            return RedirectToAction("Edit", new { id = project.Id });
        }

        var existing = await _projectService.GetProjectByIdAsync(project.Id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = project.Name.Trim();
        existing.Status = project.Status;
        existing.ViewState = project.ViewState;
        existing.Enabled = project.Enabled;
        existing.InheritCategories = project.InheritCategories;
        existing.Description = project.Description ?? string.Empty;

        // Save Slack ChatOps Routing Properties
        existing.SlackChannelName = project.SlackChannelName?.Trim();
        existing.SlackChannelId = project.SlackChannelId?.Trim();
        existing.SlackNotificationsEnabled = project.SlackNotificationsEnabled;
        existing.SlackWebhookUrl = project.SlackWebhookUrl?.Trim();

        await _projectService.UpdateProjectAsync(existing);
        TempData["Success"] = "Project updated successfully.";
        return RedirectToAction("Edit", new { id = project.Id });
    }

    [HttpGet]
    [BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return View("DeleteConfirm", project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project != null)
        {
            await _projectService.DeleteProjectAsync(id);
            TempData["Success"] = $"Project '{project.Name}' was deleted.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> AddSubproject(int parentId, int childId, bool inherit = false)
    {
        var success = await _projectService.AddSubprojectAsync(parentId, childId, inherit);
        if (!success)
        {
            TempData["Error"] = "Could not attach subproject (check for circular hierarchy or existing link).";
        }
        else
        {
            TempData["Success"] = "Subproject attached successfully.";
        }

        return RedirectToAction("Edit", new { id = parentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> RemoveSubproject(int parentId, int childId)
    {
        await _projectService.RemoveSubprojectAsync(parentId, childId);
        TempData["Success"] = "Subproject unlinked.";
        return RedirectToAction("Edit", new { id = parentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> UpdateSubprojectChildren(int projectId, IFormCollection form)
    {
        var subprojects = await _projectService.GetSubprojectsAsync(projectId);
        var map = new Dictionary<int, bool>();

        foreach (var sp in subprojects)
        {
            var key = $"inherit_child_{sp.Id}";
            var isChecked = form.ContainsKey(key) && (form[key] == "true" || form[key] == "1" || form[key] == "on");
            map[sp.Id] = isChecked;
        }

        await _projectService.UpdateSubprojectsInheritanceAsync(projectId, map);
        TempData["Success"] = "Subproject inheritance settings updated.";
        return RedirectToAction("Edit", new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> AddCustomField(int projectId, int fieldId, int sequence = 0)
    {
        await _customFieldService.LinkFieldToProjectAsync(fieldId, projectId, sequence);
        TempData["Success"] = "Custom field linked to project.";
        return RedirectToAction("Edit", new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> UpdateCustomFieldSequence(int projectId, int fieldId, int sequence)
    {
        await _customFieldService.UpdateFieldSequenceAsync(fieldId, projectId, sequence);
        TempData["Success"] = "Custom field sequence updated.";
        return RedirectToAction("Edit", new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> RemoveCustomField(int projectId, int fieldId)
    {
        await _customFieldService.UnlinkFieldFromProjectAsync(fieldId, projectId);
        TempData["Success"] = "Custom field unlinked from project.";
        return RedirectToAction("Edit", new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [BugCoreAuthorize(AccessLevel.Manager, globalOnly: true)]
    public async Task<IActionResult> CopyCustomFields(int projectId, int otherProjectId, bool copyFrom, bool copyTo)
    {
        int source = copyFrom ? otherProjectId : projectId;
        int target = copyFrom ? projectId : otherProjectId;

        if (source == 0 || target == 0)
        {
            TempData["Error"] = "Invalid project selection for custom field copying.";
            return RedirectToAction("Edit", new { id = projectId });
        }

        await _customFieldService.CopyFieldsAsync(source, target);
        TempData["Success"] = "Custom fields copied successfully.";
        return RedirectToAction("Edit", new { id = projectId });
    }
}
