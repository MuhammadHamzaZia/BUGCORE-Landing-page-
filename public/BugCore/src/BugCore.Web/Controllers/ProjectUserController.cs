using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.Filters;

namespace BugCore.Web.Controllers;

[Authorize]
[BugCoreAuthorize(AccessLevel.Manager)]
public class ProjectUserController : Controller
{
    private readonly IProjectService _projectService;

    public ProjectUserController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int projectId, List<int> userIds, AccessLevel accessLevel)
    {
        if (userIds == null || !userIds.Any())
        {
            TempData["Error"] = "No users selected.";
            return Redirect($"/Project/Edit/{projectId}#project-users");
        }

        await _projectService.AssignUsersBatchAsync(projectId, userIds, accessLevel);
        TempData["Success"] = $"Assigned {userIds.Count} user(s) to project.";
        return Redirect($"/Project/Edit/{projectId}#project-users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUsers(int projectId, IFormCollection form)
    {
        var deletes = new List<int>();
        var updates = new Dictionary<int, AccessLevel>();

        // Find all delete checkboxes: name="user_access_delete"
        if (form.TryGetValue("user_access_delete", out var deleteValues))
        {
            foreach (var val in deleteValues)
            {
                if (int.TryParse(val, out var deleteId))
                {
                    deletes.Add(deleteId);
                }
            }
        }

        // Find all access level inputs: name="user_access_level[{userId}]"
        foreach (var key in form.Keys)
        {
            if (key.StartsWith("user_access_level[") && key.EndsWith("]"))
            {
                var idStr = key.Substring("user_access_level[".Length, key.Length - "user_access_level[".Length - 1);
                if (int.TryParse(idStr, out var userId) && int.TryParse(form[key], out var accessVal))
                {
                    updates[userId] = (AccessLevel)accessVal;
                }
            }
        }

        await _projectService.UpdateProjectUsersBatchAsync(projectId, updates, deletes);
        TempData["Success"] = "Project user permissions updated successfully.";
        return Redirect($"/Project/Edit/{projectId}#project-users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int projectId, int userId)
    {
        await _projectService.RemoveUserAsync(projectId, userId);
        TempData["Success"] = "User assignment removed.";
        return Redirect($"/Project/Edit/{projectId}#project-users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAll(int projectId)
    {
        await _projectService.RemoveAllUsersAsync(projectId);
        TempData["Success"] = "All user assignments removed from project.";
        return Redirect($"/Project/Edit/{projectId}#project-users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(int projectId, int otherProjectId, bool copyFrom, bool copyTo)
    {
        int source = copyFrom ? otherProjectId : projectId;
        int target = copyFrom ? projectId : otherProjectId;

        if (source == 0 || target == 0)
        {
            TempData["Error"] = "Invalid project selection for copying users.";
            return Redirect($"/Project/Edit/{projectId}#project-users");
        }

        await _projectService.CopyUsersFromProjectAsync(source, target);
        TempData["Success"] = "User permissions copied successfully.";
        return Redirect($"/Project/Edit/{projectId}#project-users");
    }
}
