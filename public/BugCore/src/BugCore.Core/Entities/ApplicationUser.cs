using Microsoft.AspNetCore.Identity;
using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_user_table with ASP.NET Core Identity integration.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    public string RealName { get; set; } = string.Empty;
    public AccessLevel GlobalAccessLevel { get; set; } = AccessLevel.Reporter;
    public bool Enabled { get; set; } = true;
    public bool Protected { get; set; } = false;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastVisit { get; set; }

    // Slack Identity Mapping
    public string? SlackUserId { get; set; }
    public string? SlackUsername { get; set; }

    // Navigation properties
    public virtual ICollection<ProjectUser> ProjectAssignments { get; set; } = new List<ProjectUser>();
    public virtual ICollection<Issue> ReportedIssues { get; set; } = new List<Issue>();
    public virtual ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public virtual ICollection<IssueMonitor> MonitoredIssues { get; set; } = new List<IssueMonitor>();
    public virtual ICollection<IssueNote> AuthoredNotes { get; set; } = new List<IssueNote>();
}
