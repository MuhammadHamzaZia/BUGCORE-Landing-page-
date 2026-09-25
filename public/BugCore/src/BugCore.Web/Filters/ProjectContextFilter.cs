using Microsoft.AspNetCore.Mvc.Filters;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Filters;

/// <summary>
/// Extracts active project context and injects it into ViewData for the master layout
/// and view dropdowns.
/// </summary>
public class ProjectContextFilter : IAsyncActionFilter
{
    private readonly IUserContextService _userContext;
    private readonly IProjectService _projectService;

    public ProjectContextFilter(IUserContextService userContext, IProjectService projectService)
    {
        _userContext = userContext;
        _projectService = projectService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as Microsoft.AspNetCore.Mvc.Controller;
        if (controller != null)
        {
            var activeProjectId = _userContext.GetActiveProjectId();
            controller.ViewData["ActiveProjectId"] = activeProjectId;

            var userId = _userContext.GetCurrentUserId();
            if (userId.HasValue)
            {
                var accessible = await _projectService.GetAccessibleProjectsAsync(userId.Value);
                controller.ViewData["AccessibleProjects"] = accessible;

                if (activeProjectId.HasValue)
                {
                    controller.ViewData["ActiveProject"] = accessible.FirstOrDefault(p => p.Id == activeProjectId.Value);
                }

                var accessLevel = await _userContext.GetActiveProjectAccessLevelAsync();
                controller.ViewData["UserAccessLevel"] = accessLevel;

                var currentUser = await _userContext.GetCurrentUserAsync();
                controller.ViewData["CurrentUser"] = currentUser;
            }
        }

        await next();
    }
}
