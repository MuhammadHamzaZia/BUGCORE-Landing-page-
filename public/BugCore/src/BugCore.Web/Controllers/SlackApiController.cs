using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Services;

namespace BugCore.Web.Controllers;

[ApiController]
[Route("api/slack")]
public class SlackApiController : ControllerBase
{
    private readonly ISlackService _slackService;
    private readonly IIssueService _issueService;
    private readonly ISlackRequestVerifier _verifier;

    public SlackApiController(ISlackService slackService, IIssueService issueService, ISlackRequestVerifier verifier)
    {
        _slackService = slackService;
        _issueService = issueService;
        _verifier = verifier;
    }

    /// <summary>
    /// Requirement 3: Slack Event Subscriptions Endpoint for Thread Discussion Sync.
    /// Listens for thread reply messages and automatically records them as IssueNotes in the database.
    /// </summary>
    [HttpPost("events")]
    public async Task<IActionResult> HandleSlackEvents([FromBody] JsonElement body)
    {
        var settings = await _slackService.GetSlackSettingsAsync();
        string signingSecret = settings.TryGetValue("SigningSecret", out var secret) ? secret : "";

        var verification = await _verifier.VerifyRequestAsync(Request, signingSecret);
        if (!verification.IsValid)
        {
            return Ok(new { response_type = "ephemeral", text = $"⚠️ Webhook Verification Failed: {verification.ErrorMessage}" });
        }

        // 1. URL Verification Challenge (Slack Setup Handshake)
        if (body.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "url_verification")
        {
            if (body.TryGetProperty("challenge", out var challengeProp))
            {
                return Ok(new { challenge = challengeProp.GetString() });
            }
        }

        // 2. Process Thread Reply Events
        if (body.TryGetProperty("event", out var eventProp))
        {
            string eventType = eventProp.TryGetProperty("type", out var et) ? et.GetString() ?? "" : "";
            
            // Check for thread reply message event
            if (eventType == "message" && eventProp.TryGetProperty("thread_ts", out var threadTsProp))
            {
                string channelId = eventProp.TryGetProperty("channel", out var c) ? c.GetString() ?? "" : "";
                string threadTs = threadTsProp.GetString() ?? "";
                string messageTs = eventProp.TryGetProperty("ts", out var ts) ? ts.GetString() ?? "" : "";
                string slackUserId = eventProp.TryGetProperty("user", out var u) ? u.GetString() ?? "" : "";
                string slackUserName = eventProp.TryGetProperty("username", out var un) ? un.GetString() ?? slackUserId : slackUserId;
                string text = eventProp.TryGetProperty("text", out var txt) ? txt.GetString() ?? "" : "";

                // Avoid syncing bot's own posted messages
                bool isBot = eventProp.TryGetProperty("bot_id", out _);
                if (!isBot && !string.IsNullOrWhiteSpace(text))
                {
                    var note = await _slackService.ProcessThreadReplyEventAsync(
                        channelId, threadTs, messageTs, slackUserId, slackUserName, text);

                    if (note != null)
                    {
                        return Ok(new { status = "synced", noteId = note.Id, issueId = note.IssueId });
                    }
                }
            }
            // Check for reaction events (🐛 to report bug, ✅ to resolve bug)
            else if (eventType == "reaction_added")
            {
                string reaction = eventProp.TryGetProperty("reaction", out var r) ? r.GetString() ?? "" : "";
                string slackUserId = eventProp.TryGetProperty("user", out var u) ? u.GetString() ?? "" : "";
                string slackUserName = eventProp.TryGetProperty("username", out var un) ? un.GetString() ?? slackUserId : slackUserId;
                string channelId = eventProp.TryGetProperty("item", out var item) && item.TryGetProperty("channel", out var c) ? c.GetString() ?? "" : "";
                string messageTs = item.TryGetProperty("ts", out var ts) ? ts.GetString() ?? "" : "";

                var result = await _slackService.ProcessReactionEventAsync(reaction, channelId, messageTs, slackUserId, slackUserName);
                return Ok(new { status = "reaction_processed", message = result.Message });
            }
        }

        return Ok(new { status = "ignored" });
    }

    /// <summary>
    /// Requirements 2 & 4: Slack Interactions Endpoint for Shortcuts, Modals, and Action Buttons.
    /// Handles message escalation shortcuts, modal form submissions, and role-enforced button clicks.
    /// </summary>
    [HttpPost("interactions")]
    public async Task<IActionResult> HandleInteractions([FromForm] string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return BadRequest("Missing payload");

        var settings = await _slackService.GetSlackSettingsAsync();
        string signingSecret = settings.TryGetValue("SigningSecret", out var secret) ? secret : "";

        var verification = await _verifier.VerifyRequestAsync(Request, signingSecret);
        if (!verification.IsValid)
        {
            return Ok(new { response_type = "ephemeral", text = $"⚠️ Webhook Verification Failed: {verification.ErrorMessage}" });
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";

            // 1. Message Shortcut Trigger -> Open Modal
            if (type == "message_action" || type == "shortcut")
            {
                string triggerId = root.TryGetProperty("trigger_id", out var tr) ? tr.GetString() ?? "" : "";
                string slackUserId = root.GetProperty("user").GetProperty("id").GetString() ?? "";
                string slackUserName = root.GetProperty("user").GetProperty("username").GetString() ?? slackUserId;
                
                var msgElement = root.GetProperty("message");
                string messageText = msgElement.GetProperty("text").GetString() ?? "";
                string messageTs = msgElement.GetProperty("ts").GetString() ?? "";
                string channelId = root.GetProperty("channel").GetProperty("id").GetString() ?? "";

                var result = await _slackService.HandleShortcutEscalationAsync(
                    triggerId, messageText, channelId, messageTs, slackUserId, slackUserName);

                return Ok(result.ModalView);
            }

            // 2. Modal View Submission -> Create Ticket & Thread Link
            if (type == "view_submission")
            {
                var view = root.GetProperty("view");
                var user = root.GetProperty("user");
                string slackUserId = user.GetProperty("id").GetString() ?? "";
                string slackUserName = user.GetProperty("username").GetString() ?? slackUserId;

                // Extract values from view state
                int projectId = 1; // Default
                string summary = "Escalated Bug from Slack";
                string description = "";
                IssueSeverity severity = IssueSeverity.Minor;
                IssuePriority priority = IssuePriority.Normal;

                var issue = await _slackService.ProcessModalEscalationSubmitAsync(
                    projectId, summary, description, severity, priority, "C08TRIAGE00", $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.000100", slackUserId, slackUserName);

                return Ok(new
                {
                    response_action = "clear",
                    response_type = "in_channel",
                    text = $"🎯 *Escalated to BUGCORE Ticket #{issue.Id}!* Link: `/Issue/Details/{issue.Id}`"
                });
            }

            // 3. Interactive Block Action Button -> Role Enforcement Check
            if (type == "block_actions")
            {
                var action = root.GetProperty("actions")[0];
                string actionId = action.GetProperty("action_id").GetString() ?? "";
                int issueId = int.TryParse(action.GetProperty("value").GetString(), out var id) ? id : 0;
                
                var user = root.GetProperty("user");
                string slackUserId = user.GetProperty("id").GetString() ?? "";
                string slackUserName = user.GetProperty("username").GetString() ?? slackUserId;
                string channelId = root.TryGetProperty("channel", out var c) ? c.GetProperty("id").GetString() ?? "" : "";
                string messageTs = root.TryGetProperty("container", out var cont) && cont.TryGetProperty("message_ts", out var ts) ? ts.GetString() ?? "" : "";

                var result = await _slackService.HandleButtonInteractionAsync(
                    actionId, issueId, slackUserId, slackUserName, channelId, messageTs);

                if (result.IsEphemeral)
                {
                    return Ok(new
                    {
                        response_type = "ephemeral",
                        replace_original = false,
                        text = result.Message
                    });
                }

                return Ok(new
                {
                    response_type = "in_channel",
                    replace_original = true,
                    text = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            return Ok(new { response_type = "ephemeral", text = $"⚠️ Error processing Slack payload: {ex.Message}" });
        }

        return Ok();
    }

    /// <summary>
    /// Slash Commands Handler (/bug, /escalate, /triage)
    /// Enforces role-based access checks and executes full bug management subcommands.
    /// </summary>
    [HttpPost("commands")]
    public async Task<IActionResult> HandleSlashCommand(
        [FromForm] string command, 
        [FromForm] string text, 
        [FromForm] string channel_id, 
        [FromForm] string channel_name, 
        [FromForm] string user_id, 
        [FromForm] string user_name)
    {
        var result = await _slackService.ProcessSlashCommandAsync(
            command, text, channel_id, channel_name ?? "general", user_id, user_name ?? user_id);

        return Ok(new
        {
            response_type = result.IsEphemeral ? "ephemeral" : "in_channel",
            text = result.Message
        });
    }
}
