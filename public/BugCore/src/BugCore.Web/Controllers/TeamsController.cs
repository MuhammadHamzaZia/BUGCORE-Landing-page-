using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

[Authorize]
public class TeamsController : Controller
{
    private readonly ITeamsService _teamsService;
    private readonly IIssueService _issueService;

    public TeamsController(ITeamsService teamsService, IIssueService issueService)
    {
        _teamsService = teamsService;
        _issueService = issueService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var issues = await _issueService.GetIssuesAsync(new IssueFilterCriteria { PageSize = 10 });
        var sampleIssue = issues.FirstOrDefault();

        string sampleCardJson = sampleIssue != null
            ? _teamsService.BuildAdaptiveCardJson(sampleIssue, GetBaseUrl())
            : "{}";

        ViewBag.SampleIssue = sampleIssue;
        ViewBag.SampleCardJson = sampleCardJson;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTestCard(string webhookUrl, int issueId)
    {
        var issue = await _issueService.GetIssueByIdAsync(issueId, includeDetails: true);
        if (issue == null)
        {
            TempData["Error"] = $"Issue #{issueId} not found.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _teamsService.PostAdaptiveCardWebhookAsync(webhookUrl, issue, GetBaseUrl());
        if (result.Success)
        {
            TempData["Success"] = result.ResponseMessage;
        }
        else
        {
            TempData["Error"] = result.ResponseMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulateThrottledBatch(int issueId, string webhookUrl)
    {
        var changes = new List<string>
        {
            "Tag 'UI-Fix' attached",
            "Tag 'Regression' attached",
            "Priority upgraded from Normal to Urgent",
            "Status updated to In Progress"
        };

        var result = await _teamsService.SimulateThrottledBatchUpdateAsync(issueId, changes, webhookUrl, GetBaseUrl());
        TempData["Success"] = result.SummaryMessage;

        return RedirectToAction(nameof(Index));
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}";
    }
}
