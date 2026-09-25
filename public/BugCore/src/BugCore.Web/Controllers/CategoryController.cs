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
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IProjectService _projectService;

    public CategoryController(ICategoryService categoryService, IProjectService projectService)
    {
        _categoryService = categoryService;
        _projectService = projectService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int? projectId, string name, int? defaultAssigneeId, int status = 0, bool addAndEdit = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Category name cannot be empty.";
            if (projectId.HasValue && projectId.Value > 0)
            {
                return RedirectToAction("Edit", "Project", new { id = projectId.Value });
            }
            return RedirectToAction("Index", "Project");
        }

        var category = new Category
        {
            ProjectId = (projectId.HasValue && projectId.Value > 0) ? projectId.Value : null,
            Name = name.Trim(),
            DefaultAssigneeId = (defaultAssigneeId.HasValue && defaultAssigneeId.Value > 0) ? defaultAssigneeId.Value : null,
            Status = status
        };

        var created = await _categoryService.CreateCategoryAsync(category);

        if (addAndEdit)
        {
            return RedirectToAction("Edit", new { id = created.Id });
        }

        TempData["Success"] = $"Category '{created.Name}' added successfully.";
        if (projectId.HasValue && projectId.Value > 0)
        {
            return Redirect($"/Project/Edit/{projectId.Value}#categories");
        }
        return Redirect("/Project/Index#categories");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        Project? project = null;
        IEnumerable<ApplicationUser> users;

        if (category.ProjectId.HasValue && category.ProjectId.Value > 0)
        {
            project = await _projectService.GetProjectByIdAsync(category.ProjectId.Value);
            var projectUsers = await _projectService.GetProjectUsersAsync(category.ProjectId.Value);
            users = projectUsers.Select(pu => pu.User).ToList();
            if (!users.Any())
            {
                users = await _projectService.GetUnassignedUsersAsync(0);
            }
        }
        else
        {
            users = await _projectService.GetUnassignedUsersAsync(0);
        }

        var viewModel = new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Status = category.Status,
            DefaultAssigneeId = category.DefaultAssigneeId,
            ProjectId = category.ProjectId,
            Project = project,
            AvailableUsers = users
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Category name is required.");
            return View(model);
        }

        var category = await _categoryService.GetCategoryByIdAsync(model.Id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = model.Name.Trim();
        category.Status = model.Status;
        category.DefaultAssigneeId = (model.DefaultAssigneeId.HasValue && model.DefaultAssigneeId.Value > 0) 
            ? model.DefaultAssigneeId.Value 
            : null;

        await _categoryService.UpdateCategoryAsync(category);
        TempData["Success"] = "Category updated successfully.";

        if (category.ProjectId.HasValue && category.ProjectId.Value > 0)
        {
            return Redirect($"/Project/Edit/{category.ProjectId.Value}#categories");
        }
        return Redirect("/Project/Index#categories");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? projectId)
    {
        await _categoryService.DeleteCategoryAsync(id);
        TempData["Success"] = "Category deleted.";

        if (projectId.HasValue && projectId.Value > 0)
        {
            return Redirect($"/Project/Edit/{projectId.Value}#categories");
        }
        return Redirect("/Project/Index#categories");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(int projectId, int otherProjectId, bool copyFrom, bool copyTo, bool excludeInherited = false)
    {
        int source = copyFrom ? otherProjectId : projectId;
        int target = copyFrom ? projectId : otherProjectId;

        await _categoryService.CopyCategoriesAsync(source, target, excludeInherited);
        TempData["Success"] = "Categories copied successfully.";

        if (projectId > 0)
        {
            return Redirect($"/Project/Edit/{projectId}#categories");
        }
        return Redirect("/Project/Index#categories");
    }
}
