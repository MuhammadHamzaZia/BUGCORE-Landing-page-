using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public interface ITeamsService
{
    string BuildAdaptiveCardJson(Issue issue, string appBaseUrl);
    Task<(bool Success, string ResponseMessage)> PostAdaptiveCardWebhookAsync(string webhookUrl, Issue issue, string appBaseUrl);
    Task<(bool Success, string SummaryMessage)> SimulateThrottledBatchUpdateAsync(int issueId, List<string> changesList, string webhookUrl, string appBaseUrl);
}
