using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly BugCoreDbContext _context;
    private readonly IAccessControlService _accessControl;

    public ProjectService(BugCoreDbContext context, IAccessControlService accessControl)
    {
        _context = context;
        _accessControl = accessControl;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDisabled = false)
    {
        var query = _context.Projects
            .Include(p => p.Categories)
            .Include(p => p.Versions)
            .Include(p => p.Subprojects).ThenInclude(sp => sp.ChildProject)
            .AsQueryable();

        if (!includeDisabled)
        {
            query = query.Where(p => p.Enabled);
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetAccessibleProjectsAsync(int userId)
    {
        var allProjects = await GetAllProjectsAsync(includeDisabled: false);
        var accessible = new List<Project>();

        foreach (var project in allProjects)
        {
            if (await _accessControl.CanViewProjectAsync(userId, project.Id))
            {
                accessible.Add(project);
            }
        }

        return accessible;
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Categories)
            .Include(p => p.Versions)
            .Include(p => p.Subprojects).ThenInclude(sp => sp.ChildProject)
            .Include(p => p.ParentProjects).ThenInclude(pp => pp.ParentProject)
            .Include(p => p.UserAssignments).ThenInclude(ua => ua.User)
            .Include(p => p.CustomFields).ThenInclude(cf => cf.CustomField)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateProjectAsync(Project project, int? parentProjectId = null, bool inheritCategories = true)
    {
        project.InheritCategories = inheritCategories;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        if (parentProjectId.HasValue && parentProjectId.Value > 0)
        {
            _context.ProjectHierarchies.Add(new ProjectHierarchy
            {
                ParentProjectId = parentProjectId.Value,
                ChildProjectId = project.Id,
                InheritChild = false
            });
            await _context.SaveChangesAsync();
        }

        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Project>> GetSubprojectsAsync(int parentProjectId)
    {
        return await _context.ProjectHierarchies
            .Where(ph => ph.ParentProjectId == parentProjectId)
            .Select(ph => ph.ChildProject)
            .ToListAsync();
    }

    public async Task<bool> AddSubprojectAsync(int parentProjectId, int childProjectId, bool inheritChild = false)
    {
        if (parentProjectId == childProjectId) return false;

        var existing = await _context.ProjectHierarchies
            .FirstOrDefaultAsync(ph => ph.ParentProjectId == parentProjectId && ph.ChildProjectId == childProjectId);

        if (existing != null) return false;

        // Circular hierarchy prevention (matching BugCoreBT project_hierarchy_api.php)
        if (await IsDescendantProjectAsync(childProjectId, parentProjectId))
        {
            return false;
        }

        _context.ProjectHierarchies.Add(new ProjectHierarchy
        {
            ParentProjectId = parentProjectId,
            ChildProjectId = childProjectId,
            InheritChild = inheritChild
        });

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsDescendantProjectAsync(int parentId, int potentialDescendantId)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(parentId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Add(current))
            {
                var children = await _context.ProjectHierarchies
                    .Where(ph => ph.ParentProjectId == current)
                    .Select(ph => ph.ChildProjectId)
                    .ToListAsync();

                foreach (var child in children)
                {
                    if (child == potentialDescendantId) return true;
                    if (!visited.Contains(child))
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        return false;
    }

    public async Task<bool> RemoveSubprojectAsync(int parentProjectId, int childProjectId)
    {
        var link = await _context.ProjectHierarchies
            .FirstOrDefaultAsync(ph => ph.ParentProjectId == parentProjectId && ph.ChildProjectId == childProjectId);

        if (link == null) return false;

        _context.ProjectHierarchies.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProjectUser>> GetProjectUsersAsync(int projectId)
    {
        return await _context.ProjectUsers
            .Include(pu => pu.User)
            .Where(pu => pu.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<bool> AssignUserAsync(int projectId, int userId, AccessLevel accessLevel)
    {
        var record = await _context.ProjectUsers
            .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

        if (record == null)
        {
            _context.ProjectUsers.Add(new ProjectUser
            {
                ProjectId = projectId,
                UserId = userId,
                AccessLevel = accessLevel
            });
        }
        else
        {
            record.AccessLevel = accessLevel;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserAsync(int projectId, int userId)
    {
        var record = await _context.ProjectUsers
            .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

        if (record == null) return false;

        _context.ProjectUsers.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<(Project Project, int Level)>> GetHierarchicalProjectsAsync(bool includeDisabled = false)
    {
        var allProjects = await GetAllProjectsAsync(includeDisabled);
        var allHierarchies = await _context.ProjectHierarchies
            .Include(ph => ph.ChildProject)
            .Include(ph => ph.ParentProject)
            .ToListAsync();

        var childIds = allHierarchies.Select(h => h.ChildProjectId).ToHashSet();
        var rootProjects = allProjects.Where(p => !childIds.Contains(p.Id)).OrderBy(p => p.Name).ToList();

        var result = new List<(Project Project, int Level)>();
        var visited = new HashSet<int>();

        void Traverse(Project project, int level)
        {
            if (!visited.Add(project.Id)) return;
            result.Add((project, level));

            var children = allHierarchies
                .Where(h => h.ParentProjectId == project.Id)
                .Select(h => allProjects.FirstOrDefault(p => p.Id == h.ChildProjectId))
                .Where(p => p != null)
                .OrderBy(p => p!.Name);

            foreach (var child in children)
            {
                Traverse(child!, level + 1);
            }
        }

        foreach (var root in rootProjects)
        {
            Traverse(root, 0);
        }

        // Catch any disconnected or cyclic leftover projects that were not traversed
        foreach (var proj in allProjects.Where(p => !visited.Contains(p.Id)))
        {
            result.Add((proj, 0));
        }

        return result;
    }

    public async Task<IEnumerable<ProjectHierarchy>> GetSubprojectHierarchiesAsync(int parentProjectId)
    {
        return await _context.ProjectHierarchies
            .Include(ph => ph.ChildProject)
            .Where(ph => ph.ParentProjectId == parentProjectId)
            .OrderBy(ph => ph.ChildProject.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetEligibleSubprojectsAsync(int parentProjectId)
    {
        var allProjects = await _context.Projects
            .Where(p => p.Id != parentProjectId && p.Enabled)
            .OrderBy(p => p.Name)
            .ToListAsync();

        var existingSubIds = await _context.ProjectHierarchies
            .Where(ph => ph.ParentProjectId == parentProjectId)
            .Select(ph => ph.ChildProjectId)
            .ToListAsync();

        var eligible = new List<Project>();
        foreach (var p in allProjects)
        {
            if (existingSubIds.Contains(p.Id)) continue;
            if (await IsDescendantProjectAsync(p.Id, parentProjectId)) continue; // circular prevention
            eligible.Add(p);
        }

        return eligible;
    }

    public async Task<bool> UpdateSubprojectsInheritanceAsync(int parentProjectId, IDictionary<int, bool> inheritChildMap)
    {
        var links = await _context.ProjectHierarchies
            .Where(ph => ph.ParentProjectId == parentProjectId)
            .ToListAsync();

        foreach (var link in links)
        {
            if (inheritChildMap.TryGetValue(link.ChildProjectId, out var inherit))
            {
                link.InheritChild = inherit;
            }
            else
            {
                link.InheritChild = false;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUnassignedUsersAsync(int projectId)
    {
        var assignedUserIds = await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId)
            .Select(pu => pu.UserId)
            .ToListAsync();

        return await _context.Users
            .Where(u => u.Enabled && !assignedUserIds.Contains(u.Id))
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<bool> AssignUsersBatchAsync(int projectId, IEnumerable<int> userIds, AccessLevel accessLevel)
    {
        foreach (var userId in userIds)
        {
            var record = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (record == null)
            {
                _context.ProjectUsers.Add(new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = userId,
                    AccessLevel = accessLevel
                });
            }
            else
            {
                record.AccessLevel = accessLevel;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProjectUsersBatchAsync(int projectId, IDictionary<int, AccessLevel> accessLevelsToUpdate, IEnumerable<int> userIdsToRemove)
    {
        // Deletions
        var toRemoveSet = userIdsToRemove.ToHashSet();
        if (toRemoveSet.Count > 0)
        {
            var toDelete = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId && toRemoveSet.Contains(pu.UserId))
                .ToListAsync();
            _context.ProjectUsers.RemoveRange(toDelete);
        }

        // Updates
        var usersToUpdate = await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId && accessLevelsToUpdate.Keys.Contains(pu.UserId))
            .ToListAsync();

        foreach (var pu in usersToUpdate)
        {
            if (!toRemoveSet.Contains(pu.UserId) && accessLevelsToUpdate.TryGetValue(pu.UserId, out var newLevel))
            {
                pu.AccessLevel = newLevel;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAllUsersAsync(int projectId)
    {
        var users = await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId)
            .ToListAsync();

        _context.ProjectUsers.RemoveRange(users);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CopyUsersFromProjectAsync(int sourceProjectId, int targetProjectId)
    {
        var sourceUsers = await _context.ProjectUsers
            .Where(pu => pu.ProjectId == sourceProjectId)
            .ToListAsync();

        foreach (var su in sourceUsers)
        {
            var target = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == targetProjectId && pu.UserId == su.UserId);

            if (target == null)
            {
                _context.ProjectUsers.Add(new ProjectUser
                {
                    ProjectId = targetProjectId,
                    UserId = su.UserId,
                    AccessLevel = su.AccessLevel
                });
            }
            else
            {
                target.AccessLevel = su.AccessLevel;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
