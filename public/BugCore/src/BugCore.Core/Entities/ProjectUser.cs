using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_project_user_list_table.
/// Provides explicit project-level overrides for user access levels.
/// </summary>
public class ProjectUser
{
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public AccessLevel AccessLevel { get; set; } = AccessLevel.Reporter;
}
