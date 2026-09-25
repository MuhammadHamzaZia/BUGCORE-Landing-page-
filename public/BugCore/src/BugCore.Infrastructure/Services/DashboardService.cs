using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly BugCoreDbContext _context;

    public DashboardService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetUserDashboardAsync(int userId, int? currentProjectId = null, MyViewBoxSettings? settings = null)
    {
        settings ??= new MyViewBoxSettings();

        var baseQuery = _context.Issues
            .Include(i => i.Project)
            .Include(i => i.Reporter)
            .Include(i => i.Handler)
            .Include(i => i.Category)
            .AsQueryable();

        if (currentProjectId.HasValue && currentProjectId.Value > 0)
        {
            baseQuery = baseQuery.Where(i => i.ProjectId == currentProjectId.Value);
        }

        // Unassigned open issues
        var unassigned = settings.ShowUnassigned
            ? await baseQuery
                .Where(i => i.HandlerId == null && i.Status < IssueStatus.Resolved)
                .OrderByDescending(i => i.Priority)
                .Take(10)
                .ToListAsync()
            : new List<Issue>();

        // Assigned to active user
        var assignedToMe = settings.ShowAssignedToMe
            ? await baseQuery
                .Where(i => i.HandlerId == userId && i.Status < IssueStatus.Closed)
                .OrderByDescending(i => i.Priority)
                .Take(10)
                .ToListAsync()
            : new List<Issue>();

        // Reported by active user
        var reportedByMe = settings.ShowReportedByMe
            ? await baseQuery
                .Where(i => i.ReporterId == userId)
                .OrderByDescending(i => i.LastUpdated)
                .Take(10)
                .ToListAsync()
            : new List<Issue>();

        // Monitored by user
        List<Issue> monitoredByMe = new();
        if (settings.ShowMonitoredByMe)
        {
            var monitoredIssueIds = await _context.IssueMonitors
                .Where(m => m.UserId == userId)
                .Select(m => m.IssueId)
                .ToListAsync();

            monitoredByMe = await baseQuery
                .Where(i => monitoredIssueIds.Contains(i.Id))
                .OrderByDescending(i => i.LastUpdated)
                .Take(10)
                .ToListAsync();
        }

        // Resolved issues (status == Resolved / 80)
        var resolved = settings.ShowResolved
            ? await baseQuery
                .Where(i => i.Status == IssueStatus.Resolved)
                .OrderByDescending(i => i.LastUpdated)
                .Take(10)
                .ToListAsync()
            : new List<Issue>();

        // Recently modified issues
        var recentlyModified = settings.ShowRecentlyModified
            ? await baseQuery
                .OrderByDescending(i => i.LastUpdated)
                .Take(10)
                .ToListAsync()
            : new List<Issue>();

        // Recent audit activity timeline
        var recentActivity = settings.ShowTimeline
            ? await _context.IssueHistories
                .Include(h => h.User)
                .Include(h => h.Issue).ThenInclude(i => i.Project)
                .OrderByDescending(h => h.DateModified)
                .Take(15)
                .ToListAsync()
            : new List<IssueHistory>();

        return new DashboardSummary
        {
            UnassignedIssues = unassigned,
            AssignedToMe = assignedToMe,
            ReportedByMe = reportedByMe,
            MonitoredByMe = monitoredByMe,
            ResolvedIssues = resolved,
            RecentlyModified = recentlyModified,
            RecentActivity = recentActivity,
            BoxSettings = settings
        };
    }
}
