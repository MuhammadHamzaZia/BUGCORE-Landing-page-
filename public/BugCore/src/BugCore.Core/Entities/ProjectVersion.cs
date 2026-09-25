namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_project_version_table.
/// Represents release milestones and target versions.
/// </summary>
public class ProjectVersion : BaseEntity
{
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Released { get; set; } = false;
    public bool Obsolete { get; set; } = false;
    public DateTime? DateOrder { get; set; }
}
