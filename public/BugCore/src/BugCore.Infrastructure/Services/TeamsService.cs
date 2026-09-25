using System.Text;
using System.Text.Json;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;

namespace BugCore.Infrastructure.Services;

public class TeamsService : ITeamsService
{
    private readonly HttpClient _httpClient;

    public TeamsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string BuildAdaptiveCardJson(Issue issue, string appBaseUrl)
    {
        string issueUrl = $"{appBaseUrl.TrimEnd('/')}/Issue/Details/{issue.Id}";

        var card = new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    contentUrl = (string?)null,
                    content = new
                    {
                        type = "AdaptiveCard",
                        body = new object[]
                        {
                            new
                            {
                                type = "TextBlock",
                                size = "Large",
                                weight = "Bolder",
                                text = $"🚨 [{issue.Project?.Name ?? "BUGCORE"}] Bug #{issue.Id}: {issue.Summary}",
                                wrap = true,
                                color = "Attention"
                            },
                            new
                            {
                                type = "FactSet",
                                facts = new[]
                                {
                                    new { title = "Status:", value = issue.Status.ToString() },
                                    new { title = "Severity:", value = issue.Severity.ToString() },
                                    new { title = "Priority:", value = issue.Priority.ToString() },
                                    new { title = "Reporter:", value = issue.Reporter?.RealName ?? "System User" }
                                }
                            },
                            new
                            {
                                type = "TextBlock",
                                text = $"**Description:**\n{issue.Description}",
                                wrap = true
                            }
                        },
                        actions = new object[]
                        {
                            new
                            {
                                type = "Action.OpenUrl",
                                title = "🎯 View Issue in BUGCORE",
                                url = issueUrl
                            }
                        },
                        schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                        version = "1.4"
                    }
                }
            }
        };

        return JsonSerializer.Serialize(card, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<(bool Success, string ResponseMessage)> PostAdaptiveCardWebhookAsync(string webhookUrl, Issue issue, string appBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return (false, "Microsoft Teams Webhook URL is empty.");
        }

        try
        {
            string jsonPayload = BuildAdaptiveCardJson(issue, appBaseUrl);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);
            string respText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Adaptive Card successfully posted to Microsoft Teams channel!");
            }

            return (false, $"Teams HTTP {response.StatusCode}: {respText}");
        }
        catch (Exception ex)
        {
            return (false, $"Network error dispatching Adaptive Card: {ex.Message}");
        }
    }

    public async Task<(bool Success, string SummaryMessage)> SimulateThrottledBatchUpdateAsync(
        int issueId, 
        List<string> changesList, 
        string webhookUrl, 
        string appBaseUrl)
    {
        await Task.Delay(200); // Simulate queue batching window
        string combinedSummary = $"⚡ **Throttled Batch Update (4 Events Aggregated):**\n" + string.Join("\n• ", changesList);
        return (true, $"Batch update queued and combined into 1 Adaptive Card update to respect MS Teams API rate limits.\nSummary: {combinedSummary}");
    }
}
