using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_project_table. Supports hierarchical subprojects,
/// category inheritance, versions, and user project ACLs.
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;
    public int Status { get; set; } = 10; // 10: development, 30: release, 50: stable, 70: obsolete
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool InheritCategories { get; set; } = true;

    // Slack ChatOps Channel Routing Properties
    public string? SlackChannelId { get; set; }
    public string? SlackChannelName { get; set; }
    public bool SlackNotificationsEnabled { get; set; } = true;
    public string? SlackWebhookUrl { get; set; }

    // Navigation properties
    public virtual ICollection<ProjectHierarchy> Subprojects { get; set; } = new List<ProjectHierarchy>();
    public virtual ICollection<ProjectHierarchy> ParentProjects { get; set; } = new List<ProjectHierarchy>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();
    public virtual ICollection<ProjectUser> UserAssignments { get; set; } = new List<ProjectUser>();
    public virtual ICollection<CustomFieldProject> CustomFields { get; set; } = new List<CustomFieldProject>();
    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
