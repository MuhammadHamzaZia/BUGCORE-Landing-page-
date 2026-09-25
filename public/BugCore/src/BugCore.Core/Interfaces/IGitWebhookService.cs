using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public class GitWebhookResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CommitsParsed { get; set; }
    public List<int> LinkedIssueIds { get; set; } = new();
    public List<int> ResolvedIssueIds { get; set; } = new();
}

public interface IGitWebhookService
{
    Task<GitWebhookResult> ProcessWebhookPayloadAsync(
        string provider, 
        string signatureHeader, 
        string rawBody, 
        string webhookSecret);
}
