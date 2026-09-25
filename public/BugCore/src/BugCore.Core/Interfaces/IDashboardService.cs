using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public class MyViewBoxSettings
{
    public bool ShowUnassigned { get; set; } = true;
    public bool ShowAssignedToMe { get; set; } = true;
    public bool ShowReportedByMe { get; set; } = true;
    public bool ShowResolved { get; set; } = true;
    public bool ShowRecentlyModified { get; set; } = true;
    public bool ShowMonitoredByMe { get; set; } = true;
    public bool ShowTimeline { get; set; } = true;
}

public class DashboardSummary
{
    public IEnumerable<Issue> UnassignedIssues { get; set; } = new List<Issue>();
    public IEnumerable<Issue> AssignedToMe { get; set; } = new List<Issue>();
    public IEnumerable<Issue> ReportedByMe { get; set; } = new List<Issue>();
    public IEnumerable<Issue> MonitoredByMe { get; set; } = new List<Issue>();
    public IEnumerable<Issue> ResolvedIssues { get; set; } = new List<Issue>();
    public IEnumerable<Issue> RecentlyModified { get; set; } = new List<Issue>();
    public IEnumerable<IssueHistory> RecentActivity { get; set; } = new List<IssueHistory>();
    public MyViewBoxSettings BoxSettings { get; set; } = new MyViewBoxSettings();
}

public interface IDashboardService
{
    Task<DashboardSummary> GetUserDashboardAsync(int userId, int? currentProjectId = null, MyViewBoxSettings? settings = null);
}
