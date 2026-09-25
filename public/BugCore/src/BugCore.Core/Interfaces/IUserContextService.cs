using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

public interface IUserContextService
{
    int? GetCurrentUserId();
    Task<ApplicationUser?> GetCurrentUserAsync();
    int? GetActiveProjectId();
    void SetActiveProjectId(int? projectId);
    Task<AccessLevel> GetActiveProjectAccessLevelAsync();
}
