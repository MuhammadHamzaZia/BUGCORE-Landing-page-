namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_category_table.
/// If ProjectId is null or 0, it represents a global category available to all projects.
/// </summary>
public class Category : BaseEntity
{
    public int? ProjectId { get; set; }
    public virtual Project? Project { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? DefaultAssigneeId { get; set; }
    public virtual ApplicationUser? DefaultAssignee { get; set; }

    public int Status { get; set; } = 0; // Active / Obsolete

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
