using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class VersionService : IVersionService
{
    private readonly BugCoreDbContext _context;

    public VersionService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectVersion>> GetVersionsForProjectAsync(int projectId, bool includeObsolete = false)
    {
        var query = _context.ProjectVersions
            .Where(v => v.ProjectId == projectId);

        if (!includeObsolete)
        {
            query = query.Where(v => !v.Obsolete);
        }

        return await query.OrderByDescending(v => v.DateOrder ?? DateTime.MinValue).ToListAsync();
    }

    public async Task<ProjectVersion?> GetVersionByIdAsync(int id)
    {
        return await _context.ProjectVersions.FindAsync(id);
    }

    public async Task<ProjectVersion> CreateVersionAsync(ProjectVersion version)
    {
        _context.ProjectVersions.Add(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<ProjectVersion> UpdateVersionAsync(ProjectVersion version)
    {
        _context.ProjectVersions.Update(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<bool> DeleteVersionAsync(int id)
    {
        var version = await _context.ProjectVersions.FindAsync(id);
        if (version == null) return false;

        _context.ProjectVersions.Remove(version);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CopyVersionsAsync(int sourceProjectId, int targetProjectId)
    {
        var versions = await _context.ProjectVersions
            .Where(v => v.ProjectId == sourceProjectId)
            .ToListAsync();

        foreach (var v in versions)
        {
            var exists = await _context.ProjectVersions
                .AnyAsync(tv => tv.ProjectId == targetProjectId && tv.Version == v.Version);

            if (!exists)
            {
                _context.ProjectVersions.Add(new ProjectVersion
                {
                    ProjectId = targetProjectId,
                    Version = v.Version,
                    Description = v.Description,
                    Released = v.Released,
                    Obsolete = v.Obsolete,
                    DateOrder = v.DateOrder
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
