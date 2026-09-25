using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Issues;

public class BugActionGroupViewModel
{
    public string Action { get; set; } = string.Empty;
    public List<int> IssueIds { get; set; } = new();
    public List<Issue> Issues { get; set; } = new();

    // Field values based on action
    public IssueStatus? NewStatus { get; set; }
    public ResolutionType? NewResolution { get; set; }
    public int? NewHandlerId { get; set; }
    public IssuePriority? NewPriority { get; set; }
    public IssueSeverity? NewSeverity { get; set; }
    public ProjectViewState? NewViewState { get; set; }
    public int? NewProjectId { get; set; }
    public string? ActionNote { get; set; }
    public string? TagNames { get; set; }

    // Dropdowns
    public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public IEnumerable<Project> Projects { get; set; } = new List<Project>();
}
