namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_tag_table.
/// </summary>
public class Tag : BaseEntity
{
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<IssueTag> Issues { get; set; } = new List<IssueTag>();
}
