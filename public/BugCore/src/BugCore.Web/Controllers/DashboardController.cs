using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;
using BugCore.Web.ViewModels.Dashboard;

namespace BugCore.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserContextService _userContext;
    private readonly IProjectService _projectService;

    public DashboardController(
        IDashboardService dashboardService,
        IUserContextService userContext,
        IProjectService projectService)
    {
        _dashboardService = dashboardService;
        _userContext = userContext;
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MyViewBoxSettings? settings = null)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        // Restore box settings from cookie if not explicitly provided in query
        var boxSettings = settings ?? new MyViewBoxSettings();
        if (Request.Cookies.TryGetValue("bugcore_my_view_boxes", out var cookieValue) && settings == null)
        {
            var parts = cookieValue.Split(',');
            boxSettings = new MyViewBoxSettings
            {
                ShowUnassigned = parts.Contains("unassigned"),
                ShowAssignedToMe = parts.Contains("assigned"),
                ShowReportedByMe = parts.Contains("reported"),
                ShowResolved = parts.Contains("resolved"),
                ShowRecentlyModified = parts.Contains("recent"),
                ShowMonitoredByMe = parts.Contains("monitored"),
                ShowTimeline = parts.Contains("timeline")
            };
        }

        var activeProjectId = _userContext.GetActiveProjectId();
        var summary = await _dashboardService.GetUserDashboardAsync(userId.Value, activeProjectId, boxSettings);
        var accessibleProjects = await _projectService.GetAccessibleProjectsAsync(userId.Value);

        var viewModel = new DashboardViewModel
        {
            Summary = summary,
            AvailableProjects = accessibleProjects,
            CurrentProject = activeProjectId.HasValue ? accessibleProjects.FirstOrDefault(p => p.Id == activeProjectId.Value) : null,
            Settings = boxSettings
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveBoxSettings(MyViewBoxSettings settings)
    {
        var activeBoxes = new List<string>();
        if (settings.ShowUnassigned) activeBoxes.Add("unassigned");
        if (settings.ShowAssignedToMe) activeBoxes.Add("assigned");
        if (settings.ShowReportedByMe) activeBoxes.Add("reported");
        if (settings.ShowResolved) activeBoxes.Add("resolved");
        if (settings.ShowRecentlyModified) activeBoxes.Add("recent");
        if (settings.ShowMonitoredByMe) activeBoxes.Add("monitored");
        if (settings.ShowTimeline) activeBoxes.Add("timeline");

        Response.Cookies.Append("bugcore_my_view_boxes", string.Join(",", activeBoxes), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        TempData["Success"] = "Dashboard query boxes configuration updated.";
        return RedirectToAction(nameof(Index));
    }
}
