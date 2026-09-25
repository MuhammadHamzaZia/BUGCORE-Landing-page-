using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Interfaces;

namespace BugCore.Web.Controllers;

public class GitWebhookController : Controller
{
    private readonly IGitWebhookService _gitWebhookService;
    private readonly IIssueService _issueService;

    public GitWebhookController(IGitWebhookService gitWebhookService, IIssueService issueService)
    {
        _gitWebhookService = gitWebhookService;
        _issueService = issueService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var issues = await _issueService.GetIssuesAsync(new IssueFilterCriteria { PageSize = 10 });
        ViewBag.Issues = issues;
        return View();
    }

    [HttpPost("api/git/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleGitWebhook(
        [FromHeader(Name = "X-Hub-Signature-256")] string? githubSig,
        [FromHeader(Name = "X-Gitlab-Token")] string? gitlabSig)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        string rawBody = await reader.ReadToEndAsync();

        string signature = githubSig ?? gitlabSig ?? "";
        string provider = !string.IsNullOrEmpty(githubSig) ? "GitHub" : "GitLab";

        var result = await _gitWebhookService.ProcessWebhookPayloadAsync(provider, signature, rawBody, "secret123");

        if (!result.Success)
        {
            return BadRequest(new { status = "error", message = result.Message });
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulateCommitPush(int issueId, string commitMsg, string branch)
    {
        string fullCommitMsg = $"{commitMsg} Fixes #{issueId}";
        
        var payloadObj = new
        {
            @ref = $"refs/heads/{branch}",
            commits = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString("N")[..8],
                    message = fullCommitMsg,
                    author = new { name = "John Developer", email = "developer@bugcore.io" }
                }
            }
        };

        string rawJson = JsonSerializer.Serialize(payloadObj);
        var result = await _gitWebhookService.ProcessWebhookPayloadAsync("GitHub", "", rawJson, "");

        TempData["Success"] = $"Git Webhook Executed: {result.Message}";
        return RedirectToAction(nameof(Index));
    }
}
