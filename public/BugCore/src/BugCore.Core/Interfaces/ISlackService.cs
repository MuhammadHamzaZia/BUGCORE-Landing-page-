using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

public class SlackChannelDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int MemberCount { get; set; }
    public string Topic { get; set; } = string.Empty;
}

public class SlackInteractionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsEphemeral { get; set; } = true;
    public object? ModalView { get; set; }
    public object? UpdatedMessage { get; set; }
}

public interface ISlackService
{
    // 1. Channel-to-Project Routing
    Task<IEnumerable<SlackChannelDto>> GetAvailableChannelsAsync();
    Task<bool> PostIssueNotificationAsync(int issueId, string eventType = "Created");
    Task<bool> SendTestNotificationAsync(int projectId, string channelName, string? webhookUrl = null);

    // 2. Message-to-Ticket Escalation
    Task<SlackInteractionResult> HandleShortcutEscalationAsync(string triggerId, string messageText, string channelId, string messageTs, string slackUserId, string slackUserName);
    Task<Issue> ProcessModalEscalationSubmitAsync(int projectId, string summary, string description, IssueSeverity severity, IssuePriority priority, string channelId, string messageTs, string slackUserId, string slackUserName);

    // 3. Synchronized Thread Triage
    Task<IssueNote?> ProcessThreadReplyEventAsync(string channelId, string threadTs, string messageTs, string slackUserId, string slackUserName, string text);

    // 4. Organization-Level Role Enforcement & Interactive ChatOps
    Task<SlackInteractionResult> HandleButtonInteractionAsync(string actionId, int issueId, string slackUserId, string slackUserName, string channelId, string messageTs);
    Task<SlackInteractionResult> ProcessSlashCommandAsync(string command, string text, string channelId, string channelName, string slackUserId, string slackUserName);
    Task<SlackInteractionResult> ProcessReactionEventAsync(string reaction, string channelId, string messageTs, string slackUserId, string slackUserName);
    
    // Modal View Builders
    object BuildCreateBugModalView(string triggerId, int defaultProjectId = 1);
    object BuildEditBugModalView(int issueId, string triggerId);
    object BuildAddNoteModalView(int issueId, string triggerId);

    // User Identity Binding & Personalization
    Task<bool> BindUserSlackIdentityAsync(int userId, string slackUserId, string slackUsername);
    Task<(string? SlackUserId, string? SlackUsername)> GetUserSlackBindingAsync(int userId);

    // Slack Settings & Management
    Task<IDictionary<string, string>> GetSlackSettingsAsync();
    Task SaveSlackSettingsAsync(string botToken, string signingSecret, string webhookUrl, string defaultChannel);
}
