using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Web.Filters;

namespace BugCore.Web.Controllers;

[BugCoreAuthorize(AccessLevel.Viewer, globalOnly: true)]
public class SlackController : Controller
{
    private readonly ISlackService _slackService;
    private readonly IProjectService _projectService;
    private readonly IIssueService _issueService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SlackController(
        ISlackService slackService,
        IProjectService projectService,
        IIssueService issueService,
        UserManager<ApplicationUser> userManager)
    {
        _slackService = slackService;
        _projectService = projectService;
        _issueService = issueService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        bool isAdmin = user != null && (user.GlobalAccessLevel == AccessLevel.Administrator || User.IsInRole("Administrator"));
        
        // If user is admin, they can toggle or go to Admin portal; if user is non-admin, render clean Developer Workbench
        ViewBag.IsAdmin = isAdmin;
        ViewBag.Settings = await _slackService.GetSlackSettingsAsync();
        ViewBag.Channels = await _slackService.GetAvailableChannelsAsync();
        ViewBag.Projects = await _projectService.GetAllProjectsAsync();

        if (user != null)
        {
            var (slackId, slackUser) = await _slackService.GetUserSlackBindingAsync(user.Id);
            ViewBag.CurrentSlackUserId = slackId ?? user.SlackUserId ?? "U" + user.Id.ToString("D8");
            ViewBag.CurrentSlackUsername = slackUser ?? user.SlackUsername ?? user.UserName;
        }

        var recentIssues = await _issueService.GetIssuesAsync(new IssueFilterCriteria
        {
            PageSize = 20,
            SortField = "LastUpdated",
            SortDirection = "DESC"
        });

        ViewBag.RecentIssues = recentIssues;

        return View();
    }

    [HttpGet]
    [BugCoreAuthorize(AccessLevel.Administrator, globalOnly: true)]
    public async Task<IActionResult> Admin()
    {
        ViewBag.IsAdmin = true;
        ViewBag.Settings = await _slackService.GetSlackSettingsAsync();
        ViewBag.Channels = await _slackService.GetAvailableChannelsAsync();
        ViewBag.Projects = await _projectService.GetAllProjectsAsync();

        var recentIssues = await _issueService.GetIssuesAsync(new IssueFilterCriteria
        {
            PageSize = 20,
            SortField = "LastUpdated",
            SortDirection = "DESC"
        });

        ViewBag.RecentIssues = recentIssues;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BindIdentity(string slackUserId, string slackUsername)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index");

        bool success = await _slackService.BindUserSlackIdentityAsync(user.Id, slackUserId, slackUsername);
        if (success)
        {
            TempData["Success"] = $"Slack identity bound to @{slackUsername} ({slackUserId}) successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to update Slack identity binding.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(string botToken, string signingSecret, string webhookUrl, string defaultChannel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || (user.GlobalAccessLevel < AccessLevel.Administrator && !User.IsInRole("Administrator")))
        {
            TempData["Error"] = "Unauthorized: Only administrators can modify global Slack ChatOps infrastructure settings.";
            return RedirectToAction("Index");
        }

        await _slackService.SaveSlackSettingsAsync(botToken, signingSecret, webhookUrl, defaultChannel);
        TempData["Success"] = "Slack ChatOps infrastructure settings updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTestNotification(int projectId, string channelName)
    {
        var success = await _slackService.SendTestNotificationAsync(projectId, channelName);
        if (success)
        {
            TempData["Success"] = $"Test alert dispatched to Slack channel '{channelName}'.";
        }
        else
        {
            TempData["Error"] = "Failed to dispatch test Slack notification.";
        }
        return RedirectToAction("Index");
    }

    // AJAX Endpoint for Feature 2: Message Escalation Simulator
    [HttpPost]
    public async Task<IActionResult> SimulateModalSubmit(
        int projectId, string summary, string description, IssueSeverity severity, IssuePriority priority, string slackUserId, string slackUserName)
    {
        try
        {
            var issue = await _slackService.ProcessModalEscalationSubmitAsync(
                projectId, summary, description, severity, priority, "C08TRIAGE00", $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.000100", slackUserId, slackUserName);

            return Json(new
            {
                success = true,
                issueId = issue.Id,
                summary = issue.Summary,
                project = issue.Project?.Name ?? "Project",
                message = $"🎯 Ticket #{issue.Id} created successfully from Slack message escalation!"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // AJAX Endpoint for Feature 3: Synchronized Thread Triage Simulator
    [HttpPost]
    public async Task<IActionResult> SimulateThreadReply(int issueId, string slackUserId, string slackUserName, string replyText)
    {
        try
        {
            var issue = await _issueService.GetIssueByIdAsync(issueId);
            if (issue == null) return Json(new { success = false, message = "Issue not found." });

            string threadTs = string.IsNullOrEmpty(issue.SlackThreadTs) ? $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.000100" : issue.SlackThreadTs;
            string messageTs = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.{Random.Shared.Next(100000, 999999)}";

            var note = await _slackService.ProcessThreadReplyEventAsync(
                issue.SlackChannelId ?? "C08TRIAGE00", threadTs, messageTs, slackUserId, slackUserName, replyText);

            return Json(new
            {
                success = true,
                noteId = note?.Id,
                slackUserName = $"@{slackUserName}",
                noteText = replyText,
                message = $"💬 Thread message synchronized as formal IssueNote #{note?.Id} on Bug #{issueId}!"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
