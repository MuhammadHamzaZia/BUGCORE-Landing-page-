using Microsoft.EntityFrameworkCore;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class AccessControlService : IAccessControlService
{
    private readonly BugCoreDbContext _context;

    public AccessControlService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<AccessLevel> GetEffectiveAccessLevelAsync(int userId, int? projectId = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.Enabled)
        {
            return AccessLevel.Nobody;
        }

        // Administrator has global unconstrained authority
        if (user.GlobalAccessLevel >= AccessLevel.Administrator)
        {
            return AccessLevel.Administrator;
        }

        // If a project is specified, check for project-level user assignment override
        if (projectId.HasValue && projectId.Value > 0)
        {
            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId.Value && pu.UserId == userId);

            if (projectUser != null)
            {
                return projectUser.AccessLevel;
            }

            // If private project and user has no explicit assignment, access is denied
            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project != null && project.ViewState == ProjectViewState.Private)
            {
                return AccessLevel.Nobody;
            }
        }

        return user.GlobalAccessLevel;
    }

    public async Task<bool> HasAccessLevelAsync(int userId, AccessLevel requiredLevel, int? projectId = null)
    {
        var effective = await GetEffectiveAccessLevelAsync(userId, projectId);
        return effective >= requiredLevel;
    }

    public async Task<bool> CanViewProjectAsync(int userId, int projectId)
    {
        return await HasAccessLevelAsync(userId, AccessLevel.Viewer, projectId);
    }

    public async Task<bool> CanManageProjectAsync(int userId, int projectId)
    {
        return await HasAccessLevelAsync(userId, AccessLevel.Manager, projectId);
    }

    public async Task<bool> CanReportIssueAsync(int userId, int projectId)
    {
        return await HasAccessLevelAsync(userId, AccessLevel.Reporter, projectId);
    }

    public async Task<bool> CanUpdateIssueAsync(int userId, int issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        // Reporter can update own issue if at least Reporter level, otherwise Updater is required
        if (issue.ReporterId == userId)
        {
            return await HasAccessLevelAsync(userId, AccessLevel.Reporter, issue.ProjectId);
        }

        return await HasAccessLevelAsync(userId, AccessLevel.Updater, issue.ProjectId);
    }

    public async Task<bool> CanDeleteIssueAsync(int userId, int issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        return await HasAccessLevelAsync(userId, AccessLevel.Developer, issue.ProjectId);
    }

    public async Task<bool> CanAddNoteAsync(int userId, int issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        return await HasAccessLevelAsync(userId, AccessLevel.Reporter, issue.ProjectId);
    }

    public async Task<bool> CanChangeStatusAsync(int userId, int issueId, IssueStatus targetStatus)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        // Closing or resolving typically requires Developer or Manager
        if (targetStatus == IssueStatus.Resolved || targetStatus == IssueStatus.Closed)
        {
            return await HasAccessLevelAsync(userId, AccessLevel.Developer, issue.ProjectId);
        }

        return await HasAccessLevelAsync(userId, AccessLevel.Updater, issue.ProjectId);
    }
}
