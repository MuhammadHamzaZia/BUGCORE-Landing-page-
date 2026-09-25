using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class EmailGatewayService : IEmailGatewayService
{
    private readonly BugCoreDbContext _context;
    private readonly IIssueService _issueService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public EmailGatewayService(
        BugCoreDbContext context, 
        IIssueService issueService, 
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _context = context;
        _issueService = issueService;
        _userManager = userManager;
        _configuration = configuration;
    }

    public Task QueueIssueNotificationEmailAsync(Issue issue, string eventType, EmailPreferenceDto prefs)
    {
        // Check Granular Preference Toggles
        if (prefs.MinimumCriticalSeverityOnly && issue.Severity < IssueSeverity.Major)
        {
            // Filtered out by severity preference threshold
            return Task.CompletedTask;
        }

        // Asynchronous non-blocking dispatch: fire-and-forget task so main thread never blocks on SMTP handshake
        _ = Task.Run(async () =>
        {
            try
            {
                string sanitizedDescription = prefs.RedactStackTraces 
                    ? RedactSensitiveContent(issue.Description) 
                    : issue.Description;

                string smtpHost = _configuration["Smtp:Host"] ?? "localhost";
                int smtpPort = int.TryParse(_configuration["Smtp:Port"], out int p) ? p : 25;
                string senderEmail = _configuration["Smtp:From"] ?? "notifications@bugcore.local";

                using var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(senderEmail, "BUGCORE Notifications"),
                    Subject = $"[{eventType}] Bug #{issue.Id}: {issue.Summary}",
                    Body = $"<h3>Defect #{issue.Id}: {issue.Summary}</h3><p><strong>Status:</strong> {issue.Status}</p><p><strong>Severity:</strong> {issue.Severity}</p><hr/><p>{sanitizedDescription}</p>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(issue.Reporter?.Email ?? "user@bugcore.local");

                using var smtpClient = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = false,
                    Timeout = 5000
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch
            {
                // Background worker error handling
            }
        });

        return Task.CompletedTask;
    }

    public async Task<IssueNote?> ProcessInboundEmailReplyAsync(int issueId, string rawEmailBody, string senderEmail)
    {
        if (string.IsNullOrWhiteSpace(rawEmailBody)) return null;

        var issue = await _issueService.GetIssueByIdAsync(issueId, includeDetails: true);
        if (issue == null) return null;

        // Strip out email signature, quoted reply headers (e.g., "On 2026-09-24, John wrote:"), and trailing footer
        string cleanReply = StripEmailSignatureAndQuotedText(rawEmailBody);

        var user = await _userManager.FindByEmailAsync(senderEmail)
            ?? await _userManager.FindByNameAsync("developer") 
            ?? await _userManager.FindByNameAsync("administrator");

        var note = await _issueService.AddNoteAsync(
            issue.Id, 
            $"📧 **[Inbound Email Reply]**: {cleanReply.Trim()}", 
            isPrivate: false, 
            timeTrackingMinutes: 0, 
            currentUserId: user!.Id);

        return note;
    }

    public string RedactSensitiveContent(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        // Redact API Keys / Passwords (e.g. bearer tokens, key=..., secret=...)
        string redacted = Regex.Replace(rawText, @"(?i)(api[_-]?key|bearer|secret|password)\s*[:=]\s*['""]?([a-zA-Z0-9_\-\.]{8,})['""]?", "$1: [REDACTED_SECRET]");

        // Redact Stack Traces / File System Paths (e.g. C:\Users\... or /var/www/...)
        redacted = Regex.Replace(redacted, @"(?i)(at\s+[a-zA-Z0-9_\.]+\(.*?\)\s+in\s+)(?:[a-zA-Z]:\\|\/)[^\r\n]+", "$1[REDACTED_SYSTEM_PATH]");

        return redacted;
    }

    private static string StripEmailSignatureAndQuotedText(string body)
    {
        // Strip common quoted email delimiters
        string clean = Regex.Split(body, @"(?i)(?:\r?\n)(?:On\s+.*?wrote:|-----Original Message-----|From:).*")[0];
        
        // Strip "-- " signature dividers
        clean = Regex.Split(clean, @"(?m)^--\s*$")[0];

        return clean.Trim();
    }
}
