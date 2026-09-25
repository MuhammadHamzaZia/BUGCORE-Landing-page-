using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public class EmailPreferenceDto
{
    public bool EmailOnAssignedOnly { get; set; } = true;
    public bool MinimumCriticalSeverityOnly { get; set; } = false;
    public bool RedactStackTraces { get; set; } = true;
}

public interface IEmailGatewayService
{
    Task QueueIssueNotificationEmailAsync(Issue issue, string eventType, EmailPreferenceDto prefs);
    Task<IssueNote?> ProcessInboundEmailReplyAsync(int issueId, string rawEmailBody, string senderEmail);
    string RedactSensitiveContent(string rawText);
}
