using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

/// <summary>
/// Filter criteria matching BugCoreBT view_all_inc.php multi-column filter options.
/// </summary>
public class IssueFilterCriteria
{
    public int? ProjectId { get; set; }
    public int? CategoryId { get; set; }
    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueSeverity? Severity { get; set; }
    public ResolutionType? Resolution { get; set; }
    public int? HandlerId { get; set; } // null = any, 0 or -1 = unassigned
    public int? ReporterId { get; set; }
    public ProjectViewState? ViewState { get; set; }
    public string? Search { get; set; }
    public string? SortField { get; set; } = "LastUpdated";
    public string? SortDirection { get; set; } = "DESC";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Issue / Bug operations, filtering, history logging, notes, attachments, and batch action groups.
/// </summary>
public interface IIssueService
{
    Task<Issue?> GetIssueByIdAsync(int id, bool includeDetails = true);
    Task<IEnumerable<Issue>> GetIssuesAsync(IssueFilterCriteria criteria);
    Task<int> GetIssueCountAsync(IssueFilterCriteria criteria);
    Task<Issue> CreateIssueAsync(Issue issue, int reporterUserId, IDictionary<int, string>? customFields = null);
    Task<Issue> UpdateIssueAsync(Issue issue, int currentUserId);
    Task<bool> DeleteIssueAsync(int id, int currentUserId);

    // Status transition & workflow
    Task<bool> ChangeStatusAsync(int issueId, IssueStatus newStatus, ResolutionType? resolution, string? noteText, int currentUserId);
    Task<bool> AssignHandlerAsync(int issueId, int? handlerId, int currentUserId);

    // Notes
    Task<IssueNote> AddNoteAsync(int issueId, string noteText, bool isPrivate, int timeTrackingMinutes, int currentUserId);
    Task<bool> DeleteNoteAsync(int noteId, int currentUserId);

    // Monitors
    Task<bool> ToggleMonitorAsync(int issueId, int userId);
    Task<bool> IsMonitoredByUserAsync(int issueId, int userId);
    Task<bool> AddMonitorUserAsync(int issueId, int userId);
    Task<bool> RemoveMonitorUserAsync(int issueId, int userId);

    // Sticky
    Task<bool> ToggleStickyAsync(int issueId, bool isSticky, int currentUserId);

    // Relationships
    Task<bool> AddRelationshipAsync(int sourceIssueId, int targetIssueId, RelationshipType type, int currentUserId);
    Task<bool> DeleteRelationshipAsync(int relationshipId, int currentUserId);

    // File Attachments
    Task<IssueAttachment> AddAttachmentAsync(int issueId, string fileName, string fileType, byte[] content, string description, int currentUserId);
    Task<IssueAttachment?> GetAttachmentByIdAsync(int attachmentId);
    Task<bool> DeleteAttachmentAsync(int attachmentId, int currentUserId);

    // Batch / Action Group Processing (matching bug_actiongroup.php)
    Task<bool> BatchChangeStatusAsync(IEnumerable<int> issueIds, IssueStatus status, ResolutionType? resolution, string? noteText, int currentUserId);
    Task<bool> BatchAssignAsync(IEnumerable<int> issueIds, int? handlerId, int currentUserId);
    Task<bool> BatchChangePriorityAsync(IEnumerable<int> issueIds, IssuePriority priority, int currentUserId);
    Task<bool> BatchChangeSeverityAsync(IEnumerable<int> issueIds, IssueSeverity severity, int currentUserId);
    Task<bool> BatchChangeViewStateAsync(IEnumerable<int> issueIds, ProjectViewState viewState, int currentUserId);
    Task<bool> BatchMoveProjectAsync(IEnumerable<int> issueIds, int targetProjectId, int currentUserId);
    Task<bool> BatchDeleteAsync(IEnumerable<int> issueIds, int currentUserId);
}
