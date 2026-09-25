using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Issues;

public class IssueListViewModel
{
    public IEnumerable<Issue> Issues { get; set; } = new List<Issue>();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

    // Multi-column filter options (matching BugCoreBT view_all_inc.php)
    public int? ProjectId { get; set; }
    public int? CategoryId { get; set; }
    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueSeverity? Severity { get; set; }
    public ResolutionType? Resolution { get; set; }
    public int? HandlerId { get; set; }
    public int? ReporterId { get; set; }
    public ProjectViewState? ViewState { get; set; }
    public string? SearchQuery { get; set; }

    // Sorting
    public string SortField { get; set; } = "LastUpdated";
    public string SortDirection { get; set; } = "DESC";

    // Lookup collections
    public IEnumerable<Project> Projects { get; set; } = new List<Project>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    // Action Group selection
    public string? ActionGroupType { get; set; }
    public List<int> SelectedIssueIds { get; set; } = new();
}
