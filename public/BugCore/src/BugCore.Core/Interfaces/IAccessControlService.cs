using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

/// <summary>
/// Mirrors BugCoreBT access_api.php.
/// Evaluates user authorization thresholds taking into account global vs project-specific overrides.
/// </summary>
public interface IAccessControlService
{
    Task<AccessLevel> GetEffectiveAccessLevelAsync(int userId, int? projectId = null);
    Task<bool> HasAccessLevelAsync(int userId, AccessLevel requiredLevel, int? projectId = null);
    Task<bool> CanViewProjectAsync(int userId, int projectId);
    Task<bool> CanManageProjectAsync(int userId, int projectId);
    Task<bool> CanReportIssueAsync(int userId, int projectId);
    Task<bool> CanUpdateIssueAsync(int userId, int issueId);
    Task<bool> CanDeleteIssueAsync(int userId, int issueId);
    Task<bool> CanAddNoteAsync(int userId, int issueId);
    Task<bool> CanChangeStatusAsync(int userId, int issueId, IssueStatus targetStatus);
}
