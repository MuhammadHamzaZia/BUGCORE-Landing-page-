namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bug_history_table. Audit log of all changes to fields and status.
/// </summary>
public class IssueHistory : BaseEntity
{
    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public int Type { get; set; } = 0; // 0: field change, 1: new bug, 2: bugnote added, 3: relationship
    public DateTime DateModified { get; set; } = DateTime.UtcNow;
}
