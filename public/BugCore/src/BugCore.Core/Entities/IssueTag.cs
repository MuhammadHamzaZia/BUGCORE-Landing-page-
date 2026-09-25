namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bug_tag_table. Many-to-many link between bugs and tags.
/// </summary>
public class IssueTag
{
    public int IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public int TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;

    public DateTime DateAttached { get; set; } = DateTime.UtcNow;
}
