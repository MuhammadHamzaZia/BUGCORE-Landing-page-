using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

public interface IIssueActionService
{
    Task<int> BatchUpdateStatusAsync(IEnumerable<int> issueIds, IssueStatus newStatus, ResolutionType? resolution, int userId);
    Task<int> BatchAssignHandlerAsync(IEnumerable<int> issueIds, int? handlerId, int userId);
    Task<int> BatchUpdatePriorityAsync(IEnumerable<int> issueIds, IssuePriority priority, int userId);
    Task<int> BatchDeleteAsync(IEnumerable<int> issueIds, int userId);
}
