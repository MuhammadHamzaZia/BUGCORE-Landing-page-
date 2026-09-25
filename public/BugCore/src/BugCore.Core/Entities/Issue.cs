using BugCore.Core.Enums;

namespace BugCore.Core.Entities;

/// <summary>
/// Replaces bugcore_bug_table and bugcore_bug_text_table.
/// Represents a defect/issue ticket.
/// </summary>
public class Issue : BaseEntity
{
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public int ReporterId { get; set; }
    public virtual ApplicationUser Reporter { get; set; } = null!;

    public int? HandlerId { get; set; }
    public virtual ApplicationUser? Handler { get; set; }

    public int? DuplicateId { get; set; }

    public IssuePriority Priority { get; set; } = IssuePriority.Normal;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;
    public ReproducibilityType Reproducibility { get; set; } = ReproducibilityType.Always;
    public IssueStatus Status { get; set; } = IssueStatus.New;
    public ResolutionType Resolution { get; set; } = ResolutionType.Open;
    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;

    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StepsToReproduce { get; set; } = string.Empty;
    public string AdditionalInformation { get; set; } = string.Empty;

    public string TargetVersion { get; set; } = string.Empty;
    public string FixedInVersion { get; set; } = string.Empty;
    public string Build { get; set; } = string.Empty;

    public bool Sticky { get; set; } = false;

    // Slack ChatOps Thread Tracking
    public string? SlackChannelId { get; set; }
    public string? SlackMessageTs { get; set; }
    public string? SlackThreadTs { get; set; }

    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation collections
    public virtual ICollection<IssueNote> Notes { get; set; } = new List<IssueNote>();
    public virtual ICollection<IssueHistory> History { get; set; } = new List<IssueHistory>();
    public virtual ICollection<IssueRelationship> SourceRelationships { get; set; } = new List<IssueRelationship>();
    public virtual ICollection<IssueRelationship> DestinationRelationships { get; set; } = new List<IssueRelationship>();
    public virtual ICollection<IssueMonitor> Monitors { get; set; } = new List<IssueMonitor>();
    public virtual ICollection<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
    public virtual ICollection<IssueTag> Tags { get; set; } = new List<IssueTag>();
    public virtual ICollection<CustomFieldValue> CustomValues { get; set; } = new List<CustomFieldValue>();
}
