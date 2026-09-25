using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Core.Interfaces;

/// <summary>
/// Project management operations (CRUD, hierarchical subprojects, user assignments, inheritance).
/// </summary>
public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDisabled = false);
    Task<IEnumerable<Project>> GetAccessibleProjectsAsync(int userId);
    Task<Project?> GetProjectByIdAsync(int id);
    Task<Project> CreateProjectAsync(Project project, int? parentProjectId = null, bool inheritCategories = true);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(int id);
    
    // Hierarchical subprojects
    Task<IEnumerable<(Project Project, int Level)>> GetHierarchicalProjectsAsync(bool includeDisabled = false);
    Task<IEnumerable<Project>> GetSubprojectsAsync(int parentProjectId);
    Task<IEnumerable<ProjectHierarchy>> GetSubprojectHierarchiesAsync(int parentProjectId);
    Task<IEnumerable<Project>> GetEligibleSubprojectsAsync(int parentProjectId);
    Task<bool> AddSubprojectAsync(int parentProjectId, int childProjectId, bool inheritChild = false);
    Task<bool> RemoveSubprojectAsync(int parentProjectId, int childProjectId);
    Task<bool> UpdateSubprojectsInheritanceAsync(int parentProjectId, IDictionary<int, bool> inheritChildMap);
    
    // User assignments & ACL
    Task<IEnumerable<ProjectUser>> GetProjectUsersAsync(int projectId);
    Task<IEnumerable<ApplicationUser>> GetUnassignedUsersAsync(int projectId);
    Task<bool> AssignUserAsync(int projectId, int userId, AccessLevel accessLevel);
    Task<bool> AssignUsersBatchAsync(int projectId, IEnumerable<int> userIds, AccessLevel accessLevel);
    Task<bool> UpdateProjectUsersBatchAsync(int projectId, IDictionary<int, AccessLevel> accessLevelsToUpdate, IEnumerable<int> userIdsToRemove);
    Task<bool> RemoveUserAsync(int projectId, int userId);
    Task<bool> RemoveAllUsersAsync(int projectId);
    Task<bool> CopyUsersFromProjectAsync(int sourceProjectId, int targetProjectId);
}
