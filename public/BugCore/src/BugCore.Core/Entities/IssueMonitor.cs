namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bug_monitor_table. Tracks users subscribed to bug update notifications.
/// </summary>
public class IssueMonitor
{
    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}
