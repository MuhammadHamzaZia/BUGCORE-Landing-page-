using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class GitWebhookService : IGitWebhookService
{
    private readonly BugCoreDbContext _context;
    private readonly IIssueService _issueService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GitWebhookService(
        BugCoreDbContext context, 
        IIssueService issueService, 
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _issueService = issueService;
        _userManager = userManager;
    }

    public async Task<GitWebhookResult> ProcessWebhookPayloadAsync(
        string provider, 
        string signatureHeader, 
        string rawBody, 
        string webhookSecret)
    {
        // 1. HMAC Signature Verification
        if (!VerifyGitSignature(provider, signatureHeader, rawBody, webhookSecret))
        {
            return new GitWebhookResult
            {
                Success = false,
                Message = "❌ Invalid HMAC payload signature. Request rejected."
            };
        }

        var result = new GitWebhookResult { Success = true };

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            string branch = ExtractBranch(root);
            var commits = ExtractCommits(root);

            foreach (var commit in commits)
            {
                result.CommitsParsed++;
                string message = commit.Message;
                string commitHash = commit.Hash.Length >= 7 ? commit.Hash[..7] : commit.Hash;
                string authorName = commit.AuthorName;
                string authorEmail = commit.AuthorEmail;

                // Find user or map fallback
                var user = await MapGitAuthorUserAsync(authorEmail, authorName);

                // Regex search for issue syntax: "fixes #101", "fix #101", "closes #101", "refs #101"
                var fixMatches = Regex.Matches(message, @"(?i)(?:fixes|fix|closes|close|resolves|resolve)\s+#(\d+)");
                var refMatches = Regex.Matches(message, @"(?i)(?:refs|ref|see)\s+#(\d+)");

                foreach (Match match in fixMatches)
                {
                    if (int.TryParse(match.Groups[1].Value, out int issueId))
                    {
                        var issue = await _issueService.GetIssueByIdAsync(issueId, includeDetails: true);
                        if (issue != null)
                        {
                            // Transition status to Resolved
                            await _issueService.ChangeStatusAsync(
                                issue.Id, 
                                IssueStatus.Resolved, 
                                ResolutionType.Fixed, 
                                $"Resolved by git commit [{commitHash}] by {authorName} on branch '{branch}'", 
                                user.Id);

                            // Branch-to-Version Mapping: If commit is on main/master, assign Fixed In Version
                            if (branch == "main" || branch == "master")
                            {
                                var projVersion = await _context.ProjectVersions
                                    .FirstOrDefaultAsync(v => v.ProjectId == issue.ProjectId && !v.Released);
                                if (projVersion != null)
                                {
                                    issue.FixedInVersion = projVersion.Version;
                                    _context.Issues.Update(issue);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            // Add Issue Note
                            await _issueService.AddNoteAsync(
                                issue.Id, 
                                $"🔗 **[Git Commit {commitHash}]**: \"{message}\"\n*Author:* {authorName} ({authorEmail}) | *Branch:* `{branch}`", 
                                isPrivate: false, 
                                timeTrackingMinutes: 0, 
                                currentUserId: user.Id);

                            result.ResolvedIssueIds.Add(issue.Id);
                            result.LinkedIssueIds.Add(issue.Id);
                        }
                    }
                }

                foreach (Match match in refMatches)
                {
                    if (int.TryParse(match.Groups[1].Value, out int issueId) && !result.LinkedIssueIds.Contains(issueId))
                    {
                        var issue = await _issueService.GetIssueByIdAsync(issueId, includeDetails: true);
                        if (issue != null)
                        {
                            await _issueService.AddNoteAsync(
                                issue.Id, 
                                $"🔗 **[Git Commit Reference {commitHash}]**: \"{message}\"\n*Branch:* `{branch}`", 
                                isPrivate: false, 
                                timeTrackingMinutes: 0, 
                                currentUserId: user.Id);

                            result.LinkedIssueIds.Add(issue.Id);
                        }
                    }
                }
            }

            result.Message = $"Processed {result.CommitsParsed} commits. Linked {result.LinkedIssueIds.Count} issue(s), resolved {result.ResolvedIssueIds.Count} issue(s).";
            return result;
        }
        catch (Exception ex)
        {
            return new GitWebhookResult
            {
                Success = false,
                Message = $"Error parsing webhook payload: {ex.Message}"
            };
        }
    }

    private static bool VerifyGitSignature(string provider, string signatureHeader, string rawBody, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(signatureHeader)) return true; // Bypass if unconfigured

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        string expectedSignature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        var headerBytes = Encoding.UTF8.GetBytes(signatureHeader);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);

        if (headerBytes.Length != expectedBytes.Length) return false;
        return CryptographicOperations.FixedTimeEquals(headerBytes, expectedBytes);
    }

    private static string ExtractBranch(JsonElement root)
    {
        if (root.TryGetProperty("ref", out var refProp))
        {
            string refStr = refProp.GetString() ?? "";
            return refStr.Replace("refs/heads/", "");
        }
        return "main";
    }

    private static List<(string Hash, string Message, string AuthorName, string AuthorEmail)> ExtractCommits(JsonElement root)
    {
        var list = new List<(string, string, string, string)>();

        if (root.TryGetProperty("commits", out var commitsProp) && commitsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in commitsProp.EnumerateArray())
            {
                string id = c.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "a1b2c3d" : "a1b2c3d";
                string msg = c.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "" : "";
                
                string authorName = "Git Developer";
                string authorEmail = "dev@example.com";

                if (c.TryGetProperty("author", out var authorProp))
                {
                    authorName = authorProp.TryGetProperty("name", out var n) ? n.GetString() ?? authorName : authorName;
                    authorEmail = authorProp.TryGetProperty("email", out var e) ? e.GetString() ?? authorEmail : authorEmail;
                }

                list.Add((id, msg, authorName, authorEmail));
            }
        }

        return list;
    }

    private async Task<ApplicationUser> MapGitAuthorUserAsync(string email, string name)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null) return user;

        user = await _userManager.FindByNameAsync("developer") ?? await _userManager.FindByNameAsync("administrator");
        return user!;
    }
}
