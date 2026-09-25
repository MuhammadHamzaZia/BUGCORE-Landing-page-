using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

[Authorize]
public class EmailGatewayController : Controller
{
    private readonly IEmailGatewayService _emailGatewayService;
    private readonly IIssueService _issueService;

    public EmailGatewayController(IEmailGatewayService emailGatewayService, IIssueService issueService)
    {
        _emailGatewayService = emailGatewayService;
        _issueService = issueService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var issues = await _issueService.GetIssuesAsync(new IssueFilterCriteria { PageSize = 10 });
        ViewBag.Issues = issues;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulateEmailDispatch(
        int issueId, 
        bool emailOnAssignedOnly, 
        bool minimumCriticalSeverityOnly, 
        bool redactStackTraces)
    {
        var issue = await _issueService.GetIssueByIdAsync(issueId, includeDetails: true);
        if (issue == null)
        {
            TempData["Error"] = $"Issue #{issueId} not found.";
            return RedirectToAction(nameof(Index));
        }

        var prefs = new EmailPreferenceDto
        {
            EmailOnAssignedOnly = emailOnAssignedOnly,
            MinimumCriticalSeverityOnly = minimumCriticalSeverityOnly,
            RedactStackTraces = redactStackTraces
        };

        await _emailGatewayService.QueueIssueNotificationEmailAsync(issue, "Updated", prefs);

        TempData["Success"] = $"Async SMTP notification queued for Issue #{issueId}. Web app thread remained unblocked.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulateInboundReply(int issueId, string rawEmailBody, string senderEmail)
    {
        var note = await _emailGatewayService.ProcessInboundEmailReplyAsync(issueId, rawEmailBody, senderEmail);
        if (note != null)
        {
            TempData["Success"] = $"Inbound email reply stripped & appended as Note #{note.Id} to Issue #{issueId}!";
        }
        else
        {
            TempData["Error"] = "Failed to process inbound email reply.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TestDataRedaction(string rawText)
    {
        string redacted = _emailGatewayService.RedactSensitiveContent(rawText);
        TempData["RedactedOutput"] = redacted;
        TempData["Success"] = "Sensitive data redaction filter executed.";
        return RedirectToAction(nameof(Index));
    }
}
