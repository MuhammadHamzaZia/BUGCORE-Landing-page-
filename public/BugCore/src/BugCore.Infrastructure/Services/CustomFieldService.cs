using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class CustomFieldService : ICustomFieldService
{
    private readonly BugCoreDbContext _context;

    public CustomFieldService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomField>> GetAllFieldsAsync()
    {
        return await _context.CustomFields
            .OrderBy(cf => cf.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomField>> GetFieldsForProjectAsync(int projectId)
    {
        return await _context.CustomFieldProjects
            .Include(cfp => cfp.CustomField)
            .Where(cfp => cfp.ProjectId == projectId)
            .OrderBy(cfp => cfp.Sequence)
            .ThenBy(cfp => cfp.CustomField.Name)
            .Select(cfp => cfp.CustomField)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomFieldProject>> GetLinkedFieldsForProjectAsync(int projectId)
    {
        return await _context.CustomFieldProjects
            .Include(cfp => cfp.CustomField)
            .Where(cfp => cfp.ProjectId == projectId)
            .OrderBy(cfp => cfp.Sequence)
            .ThenBy(cfp => cfp.CustomField.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomField>> GetUnlinkedFieldsForProjectAsync(int projectId)
    {
        var linkedIds = await _context.CustomFieldProjects
            .Where(cfp => cfp.ProjectId == projectId)
            .Select(cfp => cfp.CustomFieldId)
            .ToListAsync();

        return await _context.CustomFields
            .Where(cf => !linkedIds.Contains(cf.Id))
            .OrderBy(cf => cf.Name)
            .ToListAsync();
    }

    public async Task<CustomField?> GetFieldByIdAsync(int id)
    {
        return await _context.CustomFields
            .Include(cf => cf.ProjectLinks)
            .FirstOrDefaultAsync(cf => cf.Id == id);
    }

    public async Task<CustomField> CreateFieldAsync(CustomField field)
    {
        _context.CustomFields.Add(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task<CustomField> UpdateFieldAsync(CustomField field)
    {
        _context.CustomFields.Update(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task<bool> DeleteFieldAsync(int id)
    {
        var field = await _context.CustomFields.FindAsync(id);
        if (field == null) return false;

        _context.CustomFields.Remove(field);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LinkFieldToProjectAsync(int fieldId, int projectId, int sequence)
    {
        var existing = await _context.CustomFieldProjects
            .FirstOrDefaultAsync(cfp => cfp.CustomFieldId == fieldId && cfp.ProjectId == projectId);

        if (existing != null)
        {
            existing.Sequence = sequence;
        }
        else
        {
            _context.CustomFieldProjects.Add(new CustomFieldProject
            {
                CustomFieldId = fieldId,
                ProjectId = projectId,
                Sequence = sequence
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFieldSequenceAsync(int fieldId, int projectId, int sequence)
    {
        var link = await _context.CustomFieldProjects
            .FirstOrDefaultAsync(cfp => cfp.CustomFieldId == fieldId && cfp.ProjectId == projectId);

        if (link == null) return false;

        link.Sequence = sequence;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlinkFieldFromProjectAsync(int fieldId, int projectId)
    {
        var link = await _context.CustomFieldProjects
            .FirstOrDefaultAsync(cfp => cfp.CustomFieldId == fieldId && cfp.ProjectId == projectId);

        if (link == null) return false;

        _context.CustomFieldProjects.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CopyFieldsAsync(int sourceProjectId, int targetProjectId)
    {
        var sourceFields = await _context.CustomFieldProjects
            .Where(cfp => cfp.ProjectId == sourceProjectId)
            .ToListAsync();

        foreach (var sf in sourceFields)
        {
            var target = await _context.CustomFieldProjects
                .FirstOrDefaultAsync(cfp => cfp.ProjectId == targetProjectId && cfp.CustomFieldId == sf.CustomFieldId);

            if (target == null)
            {
                _context.CustomFieldProjects.Add(new CustomFieldProject
                {
                    ProjectId = targetProjectId,
                    CustomFieldId = sf.CustomFieldId,
                    Sequence = sf.Sequence
                });
            }
            else
            {
                target.Sequence = sf.Sequence;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
