using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bugnote_table and bugcore_bugnote_text_table.
/// Represents a comment/note on an issue.
/// </summary>
public class IssueNote : BaseEntity
{
    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public int ReporterId { get; set; }
    public virtual ApplicationUser Reporter { get; set; } = null!;

    public string Note { get; set; } = string.Empty;
    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;
    public int TimeTrackingMinutes { get; set; } = 0;

    // Slack ChatOps Note Metadata
    public string? SlackMessageTs { get; set; }
    public string? SlackUserId { get; set; }
    public string? SlackUserName { get; set; }
    public bool IsFromSlack { get; set; } = false;

    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
