using System.ComponentModel.DataAnnotations;
using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Issues;

public class EditIssueViewModel
{
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    [StringLength(128)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? StepsToReproduce { get; set; }

    public string? AdditionalInformation { get; set; }

    public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;

    public IssuePriority Priority { get; set; } = IssuePriority.Normal;

    public ReproducibilityType Reproducibility { get; set; } = ReproducibilityType.Always;

    public IssueStatus Status { get; set; } = IssueStatus.New;

    public ResolutionType Resolution { get; set; } = ResolutionType.Open;

    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;

    public string? TargetVersion { get; set; }

    public string? FixedInVersion { get; set; }

    public string? Build { get; set; }

    public int? HandlerId { get; set; }

    // Dropdowns & metadata
    public string ProjectName { get; set; } = string.Empty;
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();
    public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public IDictionary<int, string> CustomFields { get; set; } = new Dictionary<int, string>();
    public IEnumerable<CustomFieldProject> LinkedCustomFields { get; set; } = new List<CustomFieldProject>();
}
